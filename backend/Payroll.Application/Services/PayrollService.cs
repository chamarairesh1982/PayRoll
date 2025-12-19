using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.BankExports;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Application.PayrollConfig;
using Payroll.Application.PayrollConfig.DTOs;
using Payroll.Application.Exceptions;
using Payroll.Domain.GeneralLedger;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Loans;
using Payroll.Domain.Leave;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;
using Payroll.Shared;

namespace Payroll.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IPayrollDbContext _dbContext;
    private readonly IEpfEtfRuleSetService _epfEtfRuleSetService;
    private readonly ITaxRuleSetService _taxRuleSetService;
    private readonly IAuditLogger _auditLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly BankExportTemplateResolver _bankExportResolver = new();

    public PayrollService(
        IPayrollDbContext dbContext,
        IEpfEtfRuleSetService epfEtfRuleSetService,
        ITaxRuleSetService taxRuleSetService,
        IAuditLogger auditLogger,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _epfEtfRuleSetService = epfEtfRuleSetService;
        _taxRuleSetService = taxRuleSetService;
        _auditLogger = auditLogger;
        _currentUserService = currentUserService;
    }

    public async Task<PayRunDetailDto> CreatePayRunAsync(CreatePayRunRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.IsConsolidated && request.CompanyId == null && request.BranchId == null && request.CostCenterId == null)
        {
            throw new InvalidOperationException("Non-consolidated pay runs must target a company, branch, or cost center.");
        }

        var validatedScope = await ValidateOrganizationScopeAsync(
            request.CompanyId,
            request.BranchId,
            request.CostCenterId,
            cancellationToken);

        var payRun = new PayRun
        {
            Reference = $"PR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Code = request.Name,
            Name = request.Name,
            PeriodType = request.PeriodType,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            PayDate = request.PayDate == default ? request.PeriodEnd : request.PayDate,
            CompanyId = validatedScope.CompanyId,
            BranchId = validatedScope.BranchId,
            CostCenterId = validatedScope.CostCenterId,
            IsConsolidated = request.IsConsolidated,
            Status = PayRunStatus.Draft,
            IsLocked = false
        };

        var employeeIds = request.EmployeeIds?.Where(id => id != Guid.Empty).Distinct().ToList() ?? new List<Guid>();
        if (!employeeIds.Any())
        {
            var employeesQuery = ApplyScopeFilter(_dbContext.Employees.AsNoTracking(), payRun.CompanyId, payRun.BranchId, payRun.CostCenterId);

            if (request.IncludeActiveEmployeesOnly)
            {
                employeesQuery = employeesQuery.Where(e => e.IsActive);
            }

            employeeIds = await employeesQuery.Select(e => e.Id).ToListAsync(cancellationToken);
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        payRun.PaySlips = await GeneratePaySlipsForPayRunAsync(payRun, employeeIds, cancellationToken);
        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunCreated",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            null,
            cancellationToken);

        await _dbContext.PayRuns.AddAsync(payRun, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetailDto(payRun);
    }

    public async Task RecalculatePayRunAsync(Guid id, RecalculatePayRunRequest request, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Deductions)
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status == PayRunStatus.Locked || payRun.IsLocked)
        {
            throw new InvalidOperationException("Cannot recalculate a locked pay run.");
        }

        if (payRun.Status == PayRunStatus.Approved)
        {
            throw new InvalidOperationException("Cannot recalculate an approved pay run. Unlock and move it back to draft before recalculating.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        var employeeIds = payRun.PaySlips.Select(ps => ps.EmployeeId).ToList();

        _dbContext.PaySlips.RemoveRange(payRun.PaySlips);
        payRun.PaySlips.Clear();

        payRun.PaySlips = await GeneratePaySlipsForPayRunAsync(payRun, employeeIds, cancellationToken);
        var recalculatedStatus = payRun.Status == PayRunStatus.Prepared ? PayRunStatus.Prepared : PayRunStatus.Draft;
        payRun.Status = recalculatedStatus;
        payRun.ExportStatus = BankExportStatus.Pending;
        payRun.ExportedBank = null;
        payRun.ExportedAt = null;
        payRun.ExportDownloadedAt = null;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunRecalculated",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            null,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return;
    }

    public async Task ChangeStatusAsync(Guid id, ChangePayRunStatusRequest request, CancellationToken cancellationToken = default)
    {
        switch (request.Status)
        {
            case PayRunStatus.Prepared:
                await PreparePayRunAsync(id, new PayRunActionRequest(), cancellationToken);
                break;
            case PayRunStatus.Approved:
                await ApprovePayRunAsync(id, new PayRunActionRequest(), cancellationToken);
                break;
            case PayRunStatus.Locked:
                await LockPayRunAsync(id, new PayRunActionRequest(), cancellationToken);
                break;
            default:
                throw new InvalidOperationException("Unsupported pay run status transition.");
        }
    }

    public async Task PreparePayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureRole("Maker", "prepare pay runs");

        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status == PayRunStatus.Locked || payRun.IsLocked)
        {
            throw new InvalidOperationException("Cannot prepare a locked pay run.");
        }

        if (!payRun.Status.CanTransitionTo(PayRunStatus.Prepared))
        {
            throw new InvalidOperationException("Only draft pay runs can be prepared.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        var actor = GetActor();
        AddStatusHistory(payRun, PayRunStatus.Draft, PayRunStatus.Prepared, actor, request.Comment);
        payRun.PreparedAt = DateTime.UtcNow;
        payRun.PreparedByUserId = actor.UserId;
        payRun.PreparedByUserName = actor.UserName;
        payRun.Status = PayRunStatus.Prepared;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunPrepared",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            actor.UserName,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ApprovePayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureRole("Approver", "approve pay runs");

        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (!payRun.Status.CanTransitionTo(PayRunStatus.Approved))
        {
            throw new InvalidOperationException("Only prepared pay runs can be approved.");
        }

        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new InvalidOperationException("Approval comment is required.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        var actor = GetActor();
        AddStatusHistory(payRun, PayRunStatus.Prepared, PayRunStatus.Approved, actor, request.Comment);
        payRun.ApprovedAt = DateTime.UtcNow;
        payRun.ApprovedByUserId = actor.UserId;
        payRun.ApprovedByUserName = actor.UserName;
        payRun.Status = PayRunStatus.Approved;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunApproved",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            actor.UserName,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task LockPayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureRole("Approver", "lock pay runs");

        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.Approvals)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (!payRun.Status.CanTransitionTo(PayRunStatus.Locked))
        {
            throw new InvalidOperationException("Only approved pay runs can be locked.");
        }

        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new InvalidOperationException("Lock comment is required.");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        var actor = GetActor();
        AddStatusHistory(payRun, PayRunStatus.Approved, PayRunStatus.Locked, actor, request.Comment);
        payRun.LockedAt = DateTime.UtcNow;
        payRun.LockedByUserId = actor.UserId;
        payRun.LockedByUserName = actor.UserName;
        payRun.Status = PayRunStatus.Locked;
        payRun.IsLocked = true;

        var overtimeEntries = await _dbContext.OTEntries
            .Where(o => o.PayRunId == payRun.Id && o.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var overtimeEntry in overtimeEntries)
        {
            overtimeEntry.IsLockedForPayroll = true;
        }

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "PayRunLocked",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            actor.UserName,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockPayRunAsync(Guid id, PayRunActionRequest request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Unlocking pay runs is not permitted in the current workflow.");
    }

    private void AddStatusHistory(PayRun payRun, PayRunStatus fromStatus, PayRunStatus toStatus, (string UserId, string UserName) actor, string? comment)
    {
        payRun.Approvals.Add(new PayRunApproval
        {
            Id = Guid.NewGuid(),
            PayRunId = payRun.Id,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ActorUserId = string.IsNullOrWhiteSpace(actor.UserId) ? null : actor.UserId,
            ActorUserName = actor.UserName,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            ActionedAt = DateTime.UtcNow
        });
    }

    private (string UserId, string UserName) GetActor()
    {
        var userName = string.IsNullOrWhiteSpace(_currentUserService.UserName) ? "Unknown" : _currentUserService.UserName!;
        var userId = _currentUserService.UserId ?? string.Empty;
        return (userId, userName);
    }

    private void EnsureRole(string role, string action)
    {
        var hasRole = _currentUserService.Roles?.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)) == true;
        if (!hasRole)
        {
            throw new ForbiddenAccessException($"Only users with the {role} role can {action}.");
        }
    }

    private static object CreatePayRunSnapshot(PayRun payRun)
    {
        return new
        {
            payRun.Id,
            payRun.Status,
            payRun.IsLocked,
            payRun.IsConsolidated,
            payRun.CompanyId,
            payRun.BranchId,
            payRun.CostCenterId,
            payRun.PreparedAt,
            payRun.PreparedByUserName,
            payRun.ApprovedAt,
            payRun.ApprovedByUserName,
            payRun.LockedAt,
            payRun.LockedByUserName,
            payRun.GeneralLedgerStatus,
            payRun.GeneralLedgerReviewedAt,
            payRun.GeneralLedgerReviewedByUserName,
            payRun.GeneralLedgerApprovedAt,
            payRun.GeneralLedgerApprovedByUserName,
            payRun.GeneralLedgerExportedAt,
            payRun.PeriodStart,
            payRun.PeriodEnd,
            payRun.PayDate,
            PaySlipCount = payRun.PaySlips.Count,
            PaySlips = payRun.PaySlips.Select(ps => new
            {
                ps.Id,
                ps.EmployeeId,
                ps.BasicSalary,
                ps.TotalEarnings,
                ps.TotalDeductions,
                ps.NetPay
            }).ToList()
        };
    }

    public async Task<PayRunDetailDto?> GetPayRunAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payRun = await LoadPayRunWithSlipsAsync(id, cancellationToken);

        return payRun is null ? null : MapToDetailDto(payRun);
    }

    public async Task<PaginatedResult<PayRunSummaryDto>> GetPayRunsAsync(PayRunQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, query.PageSize);

        var payRuns = _dbContext.PayRuns.AsNoTracking();
        if (query.Status.HasValue)
        {
            payRuns = payRuns.Where(p => p.Status == query.Status);
        }

        if (query.CompanyId.HasValue)
        {
            payRuns = payRuns.Where(p => p.CompanyId == query.CompanyId);
        }

        if (query.BranchId.HasValue)
        {
            payRuns = payRuns.Where(p => p.BranchId == query.BranchId);
        }

        if (query.CostCenterId.HasValue)
        {
            payRuns = payRuns.Where(p => p.CostCenterId == query.CostCenterId);
        }

        if (query.IsConsolidated.HasValue)
        {
            payRuns = payRuns.Where(p => p.IsConsolidated == query.IsConsolidated.Value);
        }

        var totalCount = await payRuns.CountAsync(cancellationToken);
        var items = await payRuns
            .OrderByDescending(pr => pr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PayRunSummaryDto>
        {
            Items = items.Select(MapToSummaryDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaySlipDto?> GetPaySlipAsync(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default)
    {
        var paySlip = await _dbContext.PaySlips
            .Include(ps => ps.Earnings)
            .Include(ps => ps.Deductions)
            .Include(ps => ps.Employee)
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.Id == paySlipId && ps.PayRunId == payRunId, cancellationToken);

        return paySlip is null ? null : MapToDto(paySlip);
    }

    public async Task<FileExportResultDto?> ExportPaySlipAsync(Guid payRunId, Guid paySlipId, string format = "pdf", CancellationToken cancellationToken = default)
    {
        var payRun = await LoadPayRunWithSlipsAsync(payRunId, cancellationToken);
        if (payRun is null)
        {
            return null;
        }

        if (payRun.Status != PayRunStatus.Locked && !payRun.IsLocked)
        {
            throw new InvalidOperationException("Payslips can only be exported after the pay run is locked.");
        }

        var paySlip = payRun.PaySlips.FirstOrDefault(ps => ps.Id == paySlipId);
        if (paySlip is null)
        {
            return null;
        }

        var authenticityHash = BuildPayslipHash(payRun, paySlip);

        if (string.Equals(format, "html", StringComparison.OrdinalIgnoreCase))
        {
            var html = BuildPaySlipHtml(payRun, paySlip, authenticityHash);
            return new FileExportResultDto
            {
                FileName = $"Payslip-{paySlip.Employee?.EmployeeCode ?? paySlip.EmployeeId.ToString()}-{payRun.PayDate:yyyyMMdd}.html",
                ContentType = "text/html",
                ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(html))
            };
        }

        var pdfBytes = BuildPaySlipPdf(payRun, paySlip, authenticityHash);

        return new FileExportResultDto
        {
            FileName = $"Payslip-{paySlip.Employee?.EmployeeCode ?? paySlip.EmployeeId.ToString()}-{payRun.PayDate:yyyyMMdd}.pdf",
            ContentType = "application/pdf",
            ContentBase64 = Convert.ToBase64String(pdfBytes)
        };
    }

    public async Task<BankExportResultDto> GenerateBankExportAsync(Guid payRunId, BankExportRequest request, CancellationToken cancellationToken = default)
    {
        var template = _bankExportResolver.Resolve(request.Bank);
        var payRun = await _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Employee)
            .FirstOrDefaultAsync(pr => pr.Id == payRunId, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status != PayRunStatus.Locked && !payRun.IsLocked)
        {
            throw new InvalidOperationException("Bank exports can only be generated for locked pay runs.");
        }

        var failures = new List<BankExportFailureDto>();
        var rows = new List<BankExportRow>();

        foreach (var paySlip in payRun.PaySlips)
        {
            var employee = paySlip.Employee;
            if (employee is null)
            {
                failures.Add(new BankExportFailureDto
                {
                    EmployeeId = paySlip.EmployeeId,
                    Reason = "Employee details unavailable"
                });
                continue;
            }

            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(employee.BankAccountNumber))
            {
                missingFields.Add("account number");
            }

            if (string.IsNullOrWhiteSpace(employee.BranchCode))
            {
                missingFields.Add("branch code");
            }

            if (string.IsNullOrWhiteSpace(employee.BankCode))
            {
                missingFields.Add("bank code");
            }

            if (string.IsNullOrWhiteSpace(employee.BankName))
            {
                missingFields.Add("bank name");
            }

            if (missingFields.Any())
            {
                failures.Add(new BankExportFailureDto
                {
                    EmployeeId = employee.Id,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    Reason = $"Missing bank details: {string.Join(", ", missingFields)}"
                });
                continue;
            }

            rows.Add(new BankExportRow(
                employee.EmployeeCode,
                employee.FullName,
                employee.BankAccountNumber!,
                employee.BranchCode!,
                paySlip.NetPay));
        }

        if (failures.Any())
        {
            return new BankExportResultDto
            {
                Status = payRun.ExportStatus,
                Bank = request.Bank,
                Failures = failures
            };
        }

        var reference = string.IsNullOrWhiteSpace(payRun.Reference) ? payRun.Code : payRun.Reference;
        var content = template.Render(rows, reference);
        var fileName = $"{payRun.Code}-{template.Bank}-{DateTime.UtcNow:yyyyMMddHHmmss}.{template.FileExtension}";

        var beforeSnapshot = CreatePayRunSnapshot(payRun);
        payRun.ExportStatus = BankExportStatus.Generated;
        payRun.ExportedBank = template.Bank;
        payRun.ExportedAt = DateTime.UtcNow;
        payRun.ExportDownloadedAt = null;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "BankExportGenerated",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            _currentUserService.UserName,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BankExportResultDto
        {
            Status = payRun.ExportStatus,
            Bank = template.Bank,
            FileName = fileName,
            ContentType = template.ContentType,
            ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(content)),
            Failures = failures
        };
    }

    public async Task MarkBankExportDownloadedAsync(Guid payRunId, CancellationToken cancellationToken = default)
    {
        var payRun = await _dbContext.PayRuns.FirstOrDefaultAsync(pr => pr.Id == payRunId, cancellationToken);

        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);
        payRun.ExportStatus = BankExportStatus.Downloaded;
        payRun.ExportDownloadedAt = DateTime.UtcNow;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "BankExportDownloaded",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            _currentUserService.UserName,
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<GeneralLedgerExportDto> GenerateGeneralLedgerExportAsync(Guid payRunId, CancellationToken cancellationToken = default)
    {
        EnsureRole("Maker", "generate general ledger exports");

        var payRun = await LoadPayRunWithSlipsForUpdateAsync(payRunId, cancellationToken);
        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.Status != PayRunStatus.Approved && payRun.Status != PayRunStatus.Locked)
        {
            throw new InvalidOperationException("General ledger exports can only be generated for approved or locked pay runs.");
        }

        var mappings = await _dbContext.GeneralLedgerAccountMappings.AsNoTracking().ToListAsync(cancellationToken);
        if (!mappings.Any())
        {
            throw new InvalidOperationException("No general ledger account mappings have been configured.");
        }

        var export = BuildGeneralLedgerExport(payRun, mappings);

        var beforeSnapshot = CreatePayRunSnapshot(payRun);
        payRun.GeneralLedgerStatus = GeneralLedgerExportStatus.Generated;
        payRun.GeneralLedgerExportedAt = null;
        export.Status = payRun.GeneralLedgerStatus;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "GeneralLedgerGenerated",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            _currentUserService.UserName,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return export;
    }

    public async Task ReviewGeneralLedgerExportAsync(Guid payRunId, GeneralLedgerActionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureRole("Approver", "review general ledger exports");

        var payRun = await _dbContext.PayRuns.FirstOrDefaultAsync(pr => pr.Id == payRunId, cancellationToken);
        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.GeneralLedgerStatus != GeneralLedgerExportStatus.Generated)
        {
            throw new InvalidOperationException("Only generated exports can be moved to review.");
        }

        var actor = GetActor();
        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        payRun.GeneralLedgerStatus = GeneralLedgerExportStatus.Reviewed;
        payRun.GeneralLedgerReviewedAt = DateTime.UtcNow;
        payRun.GeneralLedgerReviewedByUserId = string.IsNullOrWhiteSpace(actor.UserId) ? null : actor.UserId;
        payRun.GeneralLedgerReviewedByUserName = actor.UserName;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "GeneralLedgerReviewed",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            actor.UserName,
            cancellationToken,
            request.Comment);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveGeneralLedgerExportAsync(Guid payRunId, GeneralLedgerActionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureRole("Approver", "approve general ledger exports");

        var payRun = await _dbContext.PayRuns.FirstOrDefaultAsync(pr => pr.Id == payRunId, cancellationToken);
        if (payRun is null)
        {
            throw new KeyNotFoundException("Pay run not found");
        }

        if (payRun.GeneralLedgerStatus != GeneralLedgerExportStatus.Reviewed)
        {
            throw new InvalidOperationException("General ledger exports must be reviewed before approval.");
        }

        var actor = GetActor();
        var beforeSnapshot = CreatePayRunSnapshot(payRun);

        payRun.GeneralLedgerStatus = GeneralLedgerExportStatus.Approved;
        payRun.GeneralLedgerApprovedAt = DateTime.UtcNow;
        payRun.GeneralLedgerApprovedByUserId = string.IsNullOrWhiteSpace(actor.UserId) ? null : actor.UserId;
        payRun.GeneralLedgerApprovedByUserName = actor.UserName;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "GeneralLedgerApproved",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            actor.UserName,
            cancellationToken,
            request.Comment);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<FileExportResultDto?> ExportGeneralLedgerAsync(Guid payRunId, CancellationToken cancellationToken = default)
    {
        EnsureRole("Approver", "export general ledger postings");

        var payRun = await LoadPayRunWithSlipsForUpdateAsync(payRunId, cancellationToken);
        if (payRun is null)
        {
            return null;
        }

        if (payRun.GeneralLedgerStatus != GeneralLedgerExportStatus.Approved)
        {
            throw new InvalidOperationException("General ledger exports can only be generated after approval.");
        }

        var mappings = await _dbContext.GeneralLedgerAccountMappings.AsNoTracking().ToListAsync(cancellationToken);
        if (!mappings.Any())
        {
            throw new InvalidOperationException("No general ledger account mappings have been configured.");
        }

        var export = BuildGeneralLedgerExport(payRun, mappings);
        var builder = new StringBuilder();
        builder.AppendLine("DebitAccount,CreditAccount,Amount,Narrative");
        foreach (var entry in export.Entries)
        {
            builder.AppendLine($"{entry.DebitAccount},{entry.CreditAccount},{entry.Amount:N2},\"{entry.Narrative.Replace("\"", "''")}\"");
        }

        var beforeSnapshot = CreatePayRunSnapshot(payRun);
        payRun.GeneralLedgerStatus = GeneralLedgerExportStatus.Exported;
        payRun.GeneralLedgerExportedAt = DateTime.UtcNow;

        await _auditLogger.LogAsync(
            nameof(PayRun),
            payRun.Id.ToString(),
            "GeneralLedgerExported",
            beforeSnapshot,
            CreatePayRunSnapshot(payRun),
            _currentUserService.UserName,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FileExportResultDto
        {
            FileName = $"GL-{payRun.Code}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
            ContentType = "text/csv",
            ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(builder.ToString()))
        };
    }

    public async Task<List<GeneralLedgerAccountMappingDto>> GetGeneralLedgerAccountMappingsAsync(CancellationToken cancellationToken = default)
    {
        var mappings = await _dbContext.GeneralLedgerAccountMappings
            .AsNoTracking()
            .OrderBy(m => m.MappingType)
            .ThenBy(m => m.Code)
            .ToListAsync(cancellationToken);

        return mappings.Select(MapToDto).ToList();
    }

    public async Task<GeneralLedgerAccountMappingDto> UpsertGeneralLedgerAccountMappingAsync(UpsertGeneralLedgerAccountMappingRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new ValidationException("Mapping code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DebitAccount) || string.IsNullOrWhiteSpace(request.CreditAccount))
        {
            throw new ValidationException("Both debit and credit accounts must be provided.");
        }

        GeneralLedgerAccountMapping mapping;
        if (request.Id.HasValue)
        {
            mapping = await _dbContext.GeneralLedgerAccountMappings.FirstOrDefaultAsync(m => m.Id == request.Id.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Mapping not found");
        }
        else
        {
            mapping = new GeneralLedgerAccountMapping
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserName ?? "System"
            };
            await _dbContext.GeneralLedgerAccountMappings.AddAsync(mapping, cancellationToken);
        }

        mapping.Code = request.Code.Trim();
        mapping.Name = request.Name.Trim();
        mapping.MappingType = request.MappingType;
        mapping.DebitAccount = request.DebitAccount.Trim();
        mapping.CreditAccount = request.CreditAccount.Trim();
        mapping.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        mapping.ModifiedAt = DateTime.UtcNow;
        mapping.ModifiedBy = _currentUserService.UserName;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(mapping);
    }

    public async Task<ApitReportDto?> GetApitReportAsync(Guid payRunId, CancellationToken cancellationToken = default)
    {
        var payRun = await LoadPayRunWithSlipsAsync(payRunId, cancellationToken);
        if (payRun is null)
        {
            return null;
        }

        var taxRuleSet = await _taxRuleSetService.GetActiveRuleForDateAsync(DateOnly.FromDateTime(payRun.PayDate));
        var yearStart = new DateTime(payRun.PayDate.Year, 1, 1);
        var employeeIds = payRun.PaySlips.Select(ps => ps.EmployeeId).Distinct().ToList();

        var yearToDateTaxes = await _dbContext.PaySlips
            .Include(ps => ps.PayRun)
            .Where(ps => employeeIds.Contains(ps.EmployeeId)
                        && ps.PayRun != null
                        && ps.PayRun.PayDate >= yearStart
                        && ps.PayRun.PayDate <= payRun.PayDate)
            .GroupBy(ps => ps.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Tax = g.Sum(ps => ps.PayeTax) })
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Tax, cancellationToken);

        var report = new ApitReportDto
        {
            PayRunId = payRun.Id,
            PayRunCode = payRun.Code,
            PayRunName = payRun.Name,
            PeriodStart = payRun.PeriodStart,
            PeriodEnd = payRun.PeriodEnd,
            PayDate = payRun.PayDate,
            PeriodType = payRun.PeriodType,
            EmployeeCount = payRun.PaySlips.Count,
        };

        foreach (var paySlip in payRun.PaySlips.OrderBy(ps => ps.Employee?.EmployeeCode))
        {
            var taxableEarnings = paySlip.Earnings.Where(e => e.IsTaxable).Sum(e => e.Amount);
            var preTaxDeductions = paySlip.Deductions.Where(d => d.IsPreTax).Sum(d => d.Amount);
            var payeResult = CalculatePaye(taxRuleSet, taxableEarnings, preTaxDeductions, payRun.PeriodType);

            var ytdTax = yearToDateTaxes.TryGetValue(paySlip.EmployeeId, out var total)
                ? total
                : paySlip.PayeTax;

            report.Employees.Add(new ApitReportEmployeeDto
            {
                PaySlipId = paySlip.Id,
                EmployeeId = paySlip.EmployeeId,
                EmployeeCode = paySlip.Employee?.EmployeeCode,
                EmployeeName = paySlip.Employee?.FullName,
                TaxableEarnings = RoundCurrency(taxableEarnings),
                PreTaxDeductions = RoundCurrency(preTaxDeductions),
                ReliefAmount = payeResult.ReliefAmount,
                RebateAmount = payeResult.RebateAmount,
                TaxableAfterRelief = payeResult.TaxableAfterRelief,
                ApitWithheld = paySlip.PayeTax,
                YearToDateApit = ytdTax
            });
        }

        report.TotalTaxForPeriod = report.Employees.Sum(e => e.ApitWithheld);
        report.TotalTaxYearToDate = report.Employees.Sum(e => e.YearToDateApit);

        return report;
    }

    public async Task<FileExportResultDto?> GenerateApitCertificateAsync(Guid payRunId, Guid paySlipId, CancellationToken cancellationToken = default)
    {
        var payRun = await LoadPayRunWithSlipsAsync(payRunId, cancellationToken);
        if (payRun is null)
        {
            return null;
        }

        var paySlip = payRun.PaySlips.FirstOrDefault(ps => ps.Id == paySlipId);
        if (paySlip is null)
        {
            return null;
        }

        var taxRuleSet = await _taxRuleSetService.GetActiveRuleForDateAsync(DateOnly.FromDateTime(payRun.PayDate));
        var taxableEarnings = paySlip.Earnings.Where(e => e.IsTaxable).Sum(e => e.Amount);
        var preTaxDeductions = paySlip.Deductions.Where(d => d.IsPreTax).Sum(d => d.Amount);
        var payeResult = CalculatePaye(taxRuleSet, taxableEarnings, preTaxDeductions, payRun.PeriodType);

        var yearStart = new DateTime(payRun.PayDate.Year, 1, 1);
        var yearToDateTax = await _dbContext.PaySlips
            .Include(ps => ps.PayRun)
            .Where(ps => ps.EmployeeId == paySlip.EmployeeId
                        && ps.PayRun != null
                        && ps.PayRun.PayDate >= yearStart
                        && ps.PayRun.PayDate <= payRun.PayDate)
            .SumAsync(ps => ps.PayeTax, cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine($"APIT Certificate - {payRun.Name}");
        builder.AppendLine($"Period: {payRun.PeriodStart:yyyy-MM-dd} to {payRun.PeriodEnd:yyyy-MM-dd}");
        builder.AppendLine($"Pay Date: {payRun.PayDate:yyyy-MM-dd}");
        builder.AppendLine();
        builder.AppendLine($"Employee: {paySlip.Employee?.FullName ?? "N/A"} ({paySlip.Employee?.EmployeeCode ?? paySlip.EmployeeId.ToString()})");
        builder.AppendLine($"NIC: {paySlip.Employee?.NicNumber ?? "-"}");
        builder.AppendLine();
        builder.AppendLine($"Taxable Earnings: {RoundCurrency(taxableEarnings):N2}");
        builder.AppendLine($"Pre-Tax Deductions: {RoundCurrency(preTaxDeductions):N2}");
        builder.AppendLine($"Income Relief Applied: {payeResult.ReliefAmount:N2}");
        builder.AppendLine($"Taxable After Relief: {payeResult.TaxableAfterRelief:N2}");
        builder.AppendLine($"Tax Rebates Applied: {payeResult.RebateAmount:N2}");
        builder.AppendLine($"APIT Withheld (Period): {paySlip.PayeTax:N2}");
        builder.AppendLine($"APIT Year To Date: {yearToDateTax:N2}");

        var fileName = $"APIT-Certificate-{paySlip.Employee?.EmployeeCode ?? paySlip.EmployeeId.ToString()}-{payRun.PayDate:yyyyMMdd}.txt";

        return new FileExportResultDto
        {
            FileName = fileName,
            ContentType = "text/plain",
            ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(builder.ToString()))
        };
    }

    private async Task<List<PaySlip>> GeneratePaySlipsForPayRunAsync(PayRun payRun, List<Guid> employeeIds, CancellationToken ct)
    {
        var employees = await ApplyScopeFilter(_dbContext.Employees, payRun.CompanyId, payRun.BranchId, payRun.CostCenterId)
            .Where(e => employeeIds.Contains(e.Id) && e.IsActive)
            .ToListAsync(ct);

        var periodStart = DateOnly.FromDateTime(payRun.PeriodStart);
        var periodEnd = DateOnly.FromDateTime(payRun.PeriodEnd);

        var attendance = await _dbContext.AttendanceRecords
            .Where(a => employeeIds.Contains(a.EmployeeId)
                        && a.Period.Start <= periodEnd
                        && a.Period.End >= periodStart)
            .ToListAsync(ct);

        var payrollSettings = await GetPayrollSettingsAsync(ct);
        var overtimeRule = await GetOvertimeRuleSnapshotAsync(ct);

        var overtime = await _dbContext.OTEntries
            .Where(o => employeeIds.Contains(o.EmployeeId)
                        && o.Date >= periodStart
                        && o.Date <= periodEnd
                        && o.Status == OvertimeStatus.Approved
                        && (!o.IsLockedForPayroll || o.PayRunId == payRun.Id)
                        && (o.PayRunId == null || o.PayRunId == payRun.Id))
            .ToListAsync(ct);

        var loans = await _dbContext.Loans
            .Include(l => l.Repayments)
            .Where(l => employeeIds.Contains(l.EmployeeId) && l.Status == LoanStatus.Active)
            .ToListAsync(ct);

        var allowanceTypes = await _dbContext.AllowanceTypes
            .AsNoTracking()
            .ToDictionaryAsync(a => a.Code, ct);

        var deductionTypes = await _dbContext.DeductionTypes
            .AsNoTracking()
            .ToDictionaryAsync(d => d.Code, ct);

        var payItems = await _dbContext.EmployeePayItems
            .AsNoTracking()
            .Where(pi => employeeIds.Contains(pi.EmployeeId)
                        && pi.IsActive
                        && pi.EffectiveFrom <= periodEnd
                        && (pi.EffectiveTo == null || pi.EffectiveTo >= periodStart))
            .ToListAsync(ct);

        var recurringPayItems = await _dbContext.EmployeeRecurringPayItems
            .AsNoTracking()
            .Include(pi => pi.AllowanceType)
            .Include(pi => pi.DeductionType)
            .Where(pi => employeeIds.Contains(pi.EmployeeId)
                        && pi.IsActive
                        && pi.EffectiveFrom <= periodEnd
                        && (pi.EffectiveTo == null || pi.EffectiveTo >= periodStart))
            .ToListAsync(ct);

        var recurringRules = await _dbContext.RecurringRules
            .AsNoTracking()
            .Where(r => r.IsActive
                        && r.Frequency == payRun.PeriodType
                        && r.StartDate <= DateOnly.FromDateTime(periodEnd)
                        && (r.EndDate == null || r.EndDate >= DateOnly.FromDateTime(periodStart)))
            .ToListAsync(ct);

        var leaveRequests = await _dbContext.LeaveRequests
            .AsNoTracking()
            .Where(lr => employeeIds.Contains(lr.EmployeeId)
                         && lr.IsActive
                         && lr.Status == LeaveStatus.Approved
                         && lr.StartDate <= periodEnd
                         && lr.EndDate >= periodStart)
            .ToListAsync(ct);

        var payDateOnly = DateOnly.FromDateTime(payRun.PayDate);
        var epfEtfRule = await _epfEtfRuleSetService.GetActiveRuleForDateAsync(payDateOnly);
        var taxRuleSet = await _taxRuleSetService.GetActiveRuleForDateAsync(payDateOnly);

        var paySlips = new List<PaySlip>();

        var recurringKeySet = new HashSet<string>();

        foreach (var employee in employees)
        {
            var paySlip = await CalculatePaySlipForEmployeeAsync(
                payRun,
                employee,
                epfEtfRule,
                taxRuleSet,
                new PaySlipCalculationContext
                {
                    Employee = employee,
                    PaySlipId = Guid.NewGuid(),
                    Attendance = attendance.Where(a => a.EmployeeId == employee.Id).ToList(),
                    Overtime = overtime.Where(o => o.EmployeeId == employee.Id).ToList(),
                    ActiveLoans = loans.Where(l => l.EmployeeId == employee.Id).ToList(),
                    PayItems = payItems.Where(pi => pi.EmployeeId == employee.Id).ToList(),
                    RecurringPayItems = recurringPayItems.Where(pi => pi.EmployeeId == employee.Id).ToList(),
                    RecurringRules = recurringRules.Where(r => r.EmployeeId == employee.Id).ToList(),
                    LeaveRequests = leaveRequests.Where(lr => lr.EmployeeId == employee.Id).ToList(),
                    AllowanceTypes = allowanceTypes,
                    DeductionTypes = deductionTypes,
                    WorkingDaysPerMonth = payrollSettings.WorkingDaysPerMonth,
                    WorkingHoursPerDay = payrollSettings.WorkingHoursPerDay,
                    NoPayCalculationBasis = payrollSettings.NoPayCalculationBasis,
                    AttendanceHalfDayHours = payrollSettings.AttendanceHalfDayHours,
                    WeekdayOvertimeMultiplier = overtimeRule.WeekdayOvertimeMultiplier,
                    WeekendOvertimeMultiplier = overtimeRule.WeekendOvertimeMultiplier,
                    HolidayOvertimeMultiplier = overtimeRule.HolidayOvertimeMultiplier,
                    OvertimeRoundingMinutes = overtimeRule.OvertimeRoundingMinutes,
                    OvertimeDailyCapHours = overtimeRule.OvertimeDailyCapHours,
                    OvertimePayRunCapHours = overtimeRule.OvertimePayRunCapHours,
                    AppliesOnWeekend = overtimeRule.AppliesOnWeekend,
                    AppliesOnHoliday = overtimeRule.AppliesOnHoliday
                },
                recurringKeySet,
                ct);

            paySlip.PayRunId = payRun.Id;
            paySlips.Add(paySlip);
        }

        return paySlips;
    }

    private Task<PayRun?> LoadPayRunWithSlipsAsync(Guid payRunId, CancellationToken cancellationToken)
    {
        return _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Deductions)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Employee)
            .Include(pr => pr.Approvals)
            .AsNoTracking()
            .FirstOrDefaultAsync(pr => pr.Id == payRunId, cancellationToken);
    }

    private Task<PayRun?> LoadPayRunWithSlipsForUpdateAsync(Guid payRunId, CancellationToken cancellationToken)
    {
        return _dbContext.PayRuns
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Deductions)
            .FirstOrDefaultAsync(pr => pr.Id == payRunId, cancellationToken);
    }

    private async Task<PayrollSettingsSnapshot> GetPayrollSettingsAsync(CancellationToken ct)
    {
        var settings = await _dbContext.PayrollSettings.AsNoTracking().FirstOrDefaultAsync(ct);

        return new PayrollSettingsSnapshot
        {
            WorkingDaysPerMonth = settings?.WorkingDaysPerMonth ?? PayrollSettingsDefaults.WorkingDaysPerMonth,
            WorkingHoursPerDay = settings?.WorkingHoursPerDay ?? PayrollSettingsDefaults.WorkingHoursPerDay,
            NoPayCalculationBasis = settings?.NoPayCalculationBasis ?? PayrollSettingsDefaults.NoPayCalculationBasis,
            AttendanceHalfDayHours = settings?.AttendanceHalfDayHours ?? PayrollSettingsDefaults.AttendanceHalfDayHours,
            WeekdayOvertimeMultiplier = settings?.WeekdayOvertimeMultiplier ?? PayrollSettingsDefaults.WeekdayOvertimeMultiplier,
            WeekendOvertimeMultiplier = settings?.WeekendOvertimeMultiplier ?? PayrollSettingsDefaults.WeekendOvertimeMultiplier,
            HolidayOvertimeMultiplier = settings?.HolidayOvertimeMultiplier ?? PayrollSettingsDefaults.HolidayOvertimeMultiplier,
            OvertimeRoundingMinutes = settings?.OvertimeRoundingMinutes ?? PayrollSettingsDefaults.OvertimeRoundingMinutes,
            OvertimeDailyCapHours = settings?.OvertimeDailyCapHours ?? PayrollSettingsDefaults.OvertimeDailyCapHours,
            OvertimePayRunCapHours = settings?.OvertimePayRunCapHours ?? PayrollSettingsDefaults.OvertimePayRunCapHours
        };
    }

    private async Task<OvertimeRuleSnapshot> GetOvertimeRuleSnapshotAsync(CancellationToken ct)
    {
        var rule = await _dbContext.OTRules
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.ModifiedAt ?? r.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return new OvertimeRuleSnapshot
        {
            WeekdayOvertimeMultiplier = rule?.WeekdayMultiplier ?? PayrollSettingsDefaults.WeekdayOvertimeMultiplier,
            WeekendOvertimeMultiplier = rule?.WeekendMultiplier ?? PayrollSettingsDefaults.WeekendOvertimeMultiplier,
            HolidayOvertimeMultiplier = rule?.HolidayMultiplier ?? PayrollSettingsDefaults.HolidayOvertimeMultiplier,
            OvertimeRoundingMinutes = rule?.RoundingMinutes ?? PayrollSettingsDefaults.OvertimeRoundingMinutes,
            OvertimeDailyCapHours = rule?.DailyCapHours ?? PayrollSettingsDefaults.OvertimeDailyCapHours,
            OvertimePayRunCapHours = rule?.PayRunCapHours ?? PayrollSettingsDefaults.OvertimePayRunCapHours,
            AppliesOnWeekend = rule?.AppliesOnWeekend ?? true,
            AppliesOnHoliday = rule?.AppliesOnHoliday ?? true
        };
    }

    private async Task<PaySlip> CalculatePaySlipForEmployeeAsync(
        PayRun payRun,
        Employee employee,
        EpfEtfRuleSetDto? epfEtfRule,
        TaxRuleSetDto? taxRuleSet,
        PaySlipCalculationContext ctx,
        HashSet<string> recurringKeys,
        CancellationToken ct)
    {
        ctx.BasicSalary = employee.BaseSalary;

        var paySlip = new PaySlip
        {
            Id = ctx.PaySlipId,
            EmployeeId = employee.Id,
            PayRunId = payRun.Id,
            CreatedBy = "system"
        };

        await ApplyBasicSalaryAsync(ctx, payRun);
        await ApplyNoPayDeductionsAsync(ctx, payRun);
        await ApplyOvertimeEarningsAsync(ctx, payRun);
        await ApplyFixedAllowancesAsync(ctx, payRun);
        await ApplyFixedDeductionsAsync(ctx, payRun);
        await ApplyRecurringRulesAsync(ctx, payRun, recurringKeys);
        await ApplyLoansAsync(ctx, payRun);
        await ApplyStatutoryContributionsAsync(ctx, payRun, epfEtfRule, taxRuleSet);

        var totalEarnings = ctx.TotalEarnings;
        var totalDeductions = ctx.TotalDeductions;
        var netPay = RoundCurrency(totalEarnings - totalDeductions);

        paySlip.BasicSalary = RoundCurrency(ctx.BasicSalary);
        paySlip.TotalEarnings = RoundCurrency(totalEarnings);
        paySlip.TotalDeductions = RoundCurrency(totalDeductions);
        paySlip.NetPay = netPay;
        paySlip.EmployeeEpf = ctx.EmployeeEpf;
        paySlip.EmployerEpf = ctx.EmployerEpf;
        paySlip.EmployerEtf = ctx.EmployerEtf;
        paySlip.PayeTax = ctx.PayeTax;
        paySlip.Earnings = ctx.Earnings;
        paySlip.Deductions = ctx.Deductions;

        return paySlip;
    }

    private Task ApplyBasicSalaryAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        ctx.Earnings.Add(new EarningLine
        {
            Id = Guid.NewGuid(),
            PaySlipId = ctx.PaySlipId,
            Code = "BASIC",
            Description = "Basic Salary",
            Amount = RoundCurrency(ctx.BasicSalary),
            IsEpfApplicable = true,
            IsEtfApplicable = true,
            IsTaxable = true
        });

        return Task.CompletedTask;
    }

    private Task ApplyNoPayDeductionsAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        var periodStart = DateOnly.FromDateTime(payRun.PeriodStart);
        var periodEnd = DateOnly.FromDateTime(payRun.PeriodEnd);

        var attendanceAbsences = BuildAttendanceAbsenceSummary(
            ctx.Attendance,
            periodStart,
            periodEnd,
            ctx.WorkingHoursPerDay,
            ctx.AttendanceHalfDayHours);
        var absentDayUnits = attendanceAbsences.DayUnits;
        var missingHoursByDay = attendanceAbsences.MissingHours;
        var encashableAbsences = new Dictionary<DateOnly, decimal>(absentDayUnits);

        var dailyRate = RoundCurrency(ctx.BasicSalary / ctx.WorkingDaysPerMonth);

        foreach (var leave in ctx.LeaveRequests)
        {
            var leaveDayUnits = GetOverlappingLeaveDayUnits(leave, periodStart, periodEnd);
            if (leaveDayUnits.Count == 0)
            {
                continue;
            }

            var leaveDays = leaveDayUnits.Values.Sum();
            var (overlapStart, overlapEnd) = GetOverlapRange(leave, periodStart, periodEnd);

            if (leave.LeaveType == LeaveTypeCode.NoPay)
            {
                var noPayAmount = RoundCurrency(dailyRate * leaveDays);
                if (noPayAmount > 0)
                {
                    ctx.Deductions.Add(new DeductionLine
                    {
                        Id = Guid.NewGuid(),
                        PaySlipId = ctx.PaySlipId,
                        Code = "LEAVE_NOPAY",
                        Description = $"No Pay Leave ({leave.LeaveType} {overlapStart:yyyy-MM-dd} to {overlapEnd:yyyy-MM-dd}, Ref: {leave.Id})",
                        Source = "Leave",
                        Amount = noPayAmount,
                        IsPreTax = true,
                        IsPostTax = false
                    });
                }

                foreach (var unit in leaveDayUnits)
                {
                    ReduceDayUnit(absentDayUnits, unit.Key, unit.Value);
                    ReduceDayUnit(encashableAbsences, unit.Key, unit.Value);
                    ReduceDayUnit(missingHoursByDay, unit.Key, unit.Value * ctx.WorkingHoursPerDay);
                }
            }
            else
            {
                var encashUnitsForLeave = 0m;

                foreach (var unit in leaveDayUnits)
                {
                    if (!encashableAbsences.TryGetValue(unit.Key, out var available))
                    {
                        continue;
                    }

                    var encashUnits = Math.Min(unit.Value, available);
                    encashUnitsForLeave += encashUnits;

                    ReduceDayUnit(encashableAbsences, unit.Key, encashUnits);
                }

                if (encashUnitsForLeave > 0)
                {
                    ctx.Earnings.Add(new EarningLine
                    {
                        Id = Guid.NewGuid(),
                        PaySlipId = ctx.PaySlipId,
                        Code = "LEAVE_ENCASH",
                        Description = $"Leave Encashment ({leave.LeaveType} {overlapStart:yyyy-MM-dd} to {overlapEnd:yyyy-MM-dd}, Ref: {leave.Id})",
                        Amount = RoundCurrency(dailyRate * encashUnitsForLeave),
                        IsEpfApplicable = false,
                        IsEtfApplicable = false,
                        IsTaxable = false
                    });
                }
            }
        }

        var noPayAmountForAttendance = ctx.NoPayCalculationBasis == CalculationBasis.PerHour
            ? RoundCurrency((ctx.BasicSalary / (ctx.WorkingDaysPerMonth * ctx.WorkingHoursPerDay)) * missingHoursByDay.Values.Sum())
            : RoundCurrency(dailyRate * absentDayUnits.Values.Sum());

        if (noPayAmountForAttendance > 0)
        {
            var unitLabel = ctx.NoPayCalculationBasis == CalculationBasis.PerHour ? "hours" : "days";
            var unitTotal = ctx.NoPayCalculationBasis == CalculationBasis.PerHour
                ? missingHoursByDay.Values.Sum()
                : absentDayUnits.Values.Sum();
            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "NOPAY",
                Description = $"No Pay for Attendance ({unitTotal:0.##} {unitLabel})",
                Source = "Attendance",
                Amount = noPayAmountForAttendance,
                IsPreTax = true,
                IsPostTax = false
            });
        }

        return Task.CompletedTask;
    }

    private static AttendanceAbsenceSummary BuildAttendanceAbsenceSummary(
        IEnumerable<AttendanceRecord> attendance,
        DateOnly periodStart,
        DateOnly periodEnd,
        int workingHoursPerDay,
        decimal attendanceHalfDayHours)
    {
        var dayUnits = new Dictionary<DateOnly, decimal>();
        var missingHours = new Dictionary<DateOnly, decimal>();
        var maxHalfDayHours = Math.Min(attendanceHalfDayHours, workingHoursPerDay);

        foreach (var record in attendance)
        {
            var overlapStart = record.Period.Start > periodStart ? record.Period.Start : periodStart;
            var overlapEnd = record.Period.End < periodEnd ? record.Period.End : periodEnd;

            if (overlapEnd < overlapStart)
            {
                continue;
            }

            var hoursWorked = Math.Clamp(record.HoursWorked, 0, workingHoursPerDay);
            var missingHoursForDay = workingHoursPerDay - hoursWorked;

            if (missingHoursForDay <= 0)
            {
                continue;
            }

            var dayUnit = hoursWorked <= 0
                ? 1m
                : hoursWorked >= maxHalfDayHours
                    ? 0.5m
                    : 1m;

            foreach (var day in EnumerateDays(overlapStart, overlapEnd))
            {
                dayUnits[day] = dayUnits.TryGetValue(day, out var existingDayUnit)
                    ? Math.Min(1m, Math.Max(existingDayUnit, dayUnit))
                    : dayUnit;
                missingHours[day] = missingHours.TryGetValue(day, out var existingMissing)
                    ? Math.Min(workingHoursPerDay, Math.Max(existingMissing, missingHoursForDay))
                    : missingHoursForDay;
            }
        }

        return new AttendanceAbsenceSummary(dayUnits, missingHours);
    }

    private static Dictionary<DateOnly, decimal> GetOverlappingLeaveDayUnits(LeaveRequest leave, DateOnly periodStart, DateOnly periodEnd)
    {
        var overlapStart = leave.StartDate > periodStart ? leave.StartDate : periodStart;
        var overlapEnd = leave.EndDate < periodEnd ? leave.EndDate : periodEnd;

        if (overlapEnd < overlapStart)
        {
            return new Dictionary<DateOnly, decimal>();
        }

        var requestedUnits = leave.TotalDays > 0 ? (decimal)leave.TotalDays : overlapEnd.DayNumber - overlapStart.DayNumber + 1;

        if (leave.IsHalfDay == true)
        {
            requestedUnits = Math.Min(requestedUnits, 0.5m);
        }

        var overlapDays = overlapEnd.DayNumber - overlapStart.DayNumber + 1;
        requestedUnits = Math.Min(requestedUnits, overlapDays);

        var unitsByDay = new Dictionary<DateOnly, decimal>();
        var remaining = requestedUnits;

        for (var i = 0; i < overlapDays && remaining > 0; i++)
        {
            var day = overlapStart.AddDays(i);
            var allocation = Math.Min(1m, remaining);

            if (leave.IsHalfDay == true && requestedUnits <= 0.5m)
            {
                allocation = Math.Min(0.5m, remaining);
            }

            unitsByDay[day] = allocation;
            remaining -= allocation;
        }

        return unitsByDay;
    }

    private static (DateOnly Start, DateOnly End) GetOverlapRange(LeaveRequest leave, DateOnly periodStart, DateOnly periodEnd)
    {
        var overlapStart = leave.StartDate > periodStart ? leave.StartDate : periodStart;
        var overlapEnd = leave.EndDate < periodEnd ? leave.EndDate : periodEnd;

        return (overlapStart, overlapEnd);
    }

    private static void ReduceDayUnit(IDictionary<DateOnly, decimal> bucket, DateOnly day, decimal amount)
    {
        if (!bucket.TryGetValue(day, out var existing))
        {
            return;
        }

        var remaining = existing - amount;
        if (remaining <= 0)
        {
            bucket.Remove(day);
        }
        else
        {
            bucket[day] = remaining;
        }
    }

    private static IEnumerable<DateOnly> EnumerateDays(DateOnly start, DateOnly end)
    {
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            yield return day;
        }
    }

    private Task ApplyOvertimeEarningsAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        var baseHourlyRate = ctx.BasicSalary / (ctx.WorkingDaysPerMonth * ctx.WorkingHoursPerDay);
        var remainingPayRunCap = ctx.OvertimePayRunCapHours > 0
            ? (decimal)ctx.OvertimePayRunCapHours
            : decimal.MaxValue;

        foreach (var overtime in ctx.Overtime.OrderBy(o => o.Date))
        {
            var multiplier = overtime.Type switch
            {
                OvertimeType.Weekend when ctx.AppliesOnWeekend => ctx.WeekendOvertimeMultiplier,
                OvertimeType.PublicHoliday when ctx.AppliesOnHoliday => ctx.HolidayOvertimeMultiplier,
                _ => ctx.WeekdayOvertimeMultiplier
            };

            var adjustedHours = CalculateRoundedOvertimeHours(overtime, ctx, ref remainingPayRunCap);
            var otAmount = RoundCurrency(adjustedHours * baseHourlyRate * multiplier);

            if (otAmount <= 0 || adjustedHours <= 0)
            {
                continue;
            }

            ctx.Earnings.Add(new EarningLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "OT",
                Description = $"Overtime ({overtime.Type}, {adjustedHours:0.##}h)",
                Amount = otAmount,
                IsEpfApplicable = true,
                IsEtfApplicable = true,
                IsTaxable = true
            });

            overtime.PayRunId = payRun.Id;
            overtime.IsLockedForPayroll = true;
        }

        return Task.CompletedTask;
    }

    private Task ApplyFixedAllowancesAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        var allowanceLines = ctx.PayItems
            .Where(pi => pi.PayItemType == PayItemType.Allowance)
            .Select(pi => (AllowanceType: ctx.AllowanceTypes.TryGetValue(pi.PayItemCode, out var allowanceType)
                ? allowanceType
                : null,
                Amount: ResolvePayItemAmount(pi, ctx.BasicSalary)))
            .Concat(ctx.RecurringPayItems
                .Where(pi => pi.PayItemKind == PayItemKind.Allowance)
                .Select(pi => (AllowanceType: pi.AllowanceType, Amount: ResolveRecurringPayItemAmount(pi, ctx.BasicSalary))));

        foreach (var allowance in allowanceLines)
        {
            if (allowance.AllowanceType is null)
            {
                continue;
            }

            if (allowance.Amount <= 0)
            {
                continue;
            }

            ctx.Earnings.Add(new EarningLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = allowance.AllowanceType.Code,
                Description = allowance.AllowanceType.Name,
                Amount = allowance.Amount,
                IsEpfApplicable = allowance.AllowanceType.IsEpfApplicable,
                IsEtfApplicable = allowance.AllowanceType.IsEtfApplicable,
                IsTaxable = allowance.AllowanceType.IsTaxable
            });
        }

        return Task.CompletedTask;
    }

    private Task ApplyFixedDeductionsAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        var deductionLines = ctx.PayItems
            .Where(pi => pi.PayItemType == PayItemType.Deduction)
            .Select(pi => (DeductionType: ctx.DeductionTypes.TryGetValue(pi.PayItemCode, out var deductionType)
                ? deductionType
                : null,
                Amount: ResolvePayItemAmount(pi, ctx.BasicSalary)))
            .Concat(ctx.RecurringPayItems
                .Where(pi => pi.PayItemKind == PayItemKind.Deduction)
                .Select(pi => (DeductionType: pi.DeductionType, Amount: ResolveRecurringPayItemAmount(pi, ctx.BasicSalary))));

        foreach (var deduction in deductionLines)
        {
            if (deduction.DeductionType is null)
            {
                continue;
            }

            if (deduction.Amount <= 0)
            {
                continue;
            }

            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = deduction.DeductionType.Code,
                Description = deduction.DeductionType.Name,
                Amount = deduction.Amount,
                IsPreTax = deduction.DeductionType.IsPreTax,
                IsPostTax = deduction.DeductionType.IsPostTax
            });
        }

        return Task.CompletedTask;
    }

    private Task ApplyRecurringRulesAsync(PaySlipCalculationContext ctx, PayRun payRun, HashSet<string> recurringKeys)
    {
        var periodKey = $"{DateOnly.FromDateTime(payRun.PeriodStart):yyyyMMdd}-{DateOnly.FromDateTime(payRun.PeriodEnd):yyyyMMdd}";

        foreach (var rule in ctx.RecurringRules)
        {
            var key = $"{rule.Id}:{ctx.Employee.Id}:{periodKey}";
            if (!recurringKeys.Add(key))
            {
                continue;
            }

            var amount = RoundCurrency(rule.Amount);
            if (amount <= 0)
            {
                continue;
            }

            if (rule.RuleType == RecurringRuleType.Allowance)
            {
                ctx.Earnings.Add(new EarningLine
                {
                    Id = Guid.NewGuid(),
                    PaySlipId = ctx.PaySlipId,
                    Code = rule.Code,
                    Description = rule.Name,
                    Amount = amount,
                    IsEpfApplicable = rule.IsEpfApplicable,
                    IsEtfApplicable = rule.IsEtfApplicable,
                    IsTaxable = rule.IsTaxable
                });
            }
            else
            {
                ctx.Deductions.Add(new DeductionLine
                {
                    Id = Guid.NewGuid(),
                    PaySlipId = ctx.PaySlipId,
                    Code = rule.Code,
                    Description = rule.Name,
                    Amount = amount,
                    IsPreTax = false,
                    IsPostTax = true
                });
            }
        }

        return Task.CompletedTask;
    }

    private Task ApplyLoansAsync(PaySlipCalculationContext ctx, PayRun payRun)
    {
        foreach (var loan in ctx.ActiveLoans)
        {
            if (loan.Status != LoanStatus.Active || loan.OutstandingPrincipal <= 0)
            {
                continue;
            }

            var scheduledRepayment = loan.Repayments
                .Where(r => !r.IsPaid)
                .OrderBy(r => r.DueDate)
                .FirstOrDefault();

            var installment = scheduledRepayment?.Amount ?? loan.InstallmentAmount;

            installment = Math.Min(loan.OutstandingPrincipal, installment);
            installment = RoundCurrency(installment);

            if (installment <= 0)
            {
                continue;
            }

            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "LOAN",
                Description = "Loan Installment",
                Amount = installment,
                IsPreTax = true,
                IsPostTax = false
            });

            loan.OutstandingPrincipal -= installment;
            if (scheduledRepayment is not null)
            {
                scheduledRepayment.IsPaid = true;
            }

            if (loan.OutstandingPrincipal <= 0)
            {
                loan.Status = LoanStatus.Closed;
            }
        }

        return Task.CompletedTask;
    }

    private Task ApplyStatutoryContributionsAsync(
        PaySlipCalculationContext ctx,
        PayRun payRun,
        EpfEtfRuleSetDto? epfEtfRule,
        TaxRuleSetDto? taxRuleSet)
    {
        ApplyEpfEtf(ctx, epfEtfRule);
        ApplyPaye(ctx, taxRuleSet, payRun.PeriodType);

        return Task.CompletedTask;
    }

    private void ApplyEpfEtf(PaySlipCalculationContext ctx, EpfEtfRuleSetDto? epfEtfRule)
    {
        if (epfEtfRule is null)
        {
            ctx.EmployeeEpf = 0;
            ctx.EmployerEpf = 0;
            ctx.EmployerEtf = 0;
            return;
        }

        var epfBase = ctx.Earnings.Where(e => e.IsEpfApplicable).Sum(e => e.Amount);
        var etfBase = ctx.Earnings.Where(e => e.IsEtfApplicable).Sum(e => e.Amount);

        if (epfEtfRule.MinimumWageForEpf.HasValue && epfBase < epfEtfRule.MinimumWageForEpf.Value)
        {
            epfBase = 0;
        }

        if (epfEtfRule.MaximumEarningForEpf.HasValue)
        {
            epfBase = Math.Min(epfBase, epfEtfRule.MaximumEarningForEpf.Value);
        }

        if (epfEtfRule.MaximumEarningForEtf.HasValue)
        {
            etfBase = Math.Min(etfBase, epfEtfRule.MaximumEarningForEtf.Value);
        }

        var employeeEpf = RoundCurrency(epfBase * epfEtfRule.EmployeeEpfRate / 100m);
        var employerEpf = RoundCurrency(epfBase * epfEtfRule.EmployerEpfRate / 100m);
        var employerEtf = RoundCurrency(etfBase * epfEtfRule.EmployerEtfRate / 100m);

        if (employeeEpf > 0)
        {
            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "EPF_EE",
                Description = "Employee EPF",
                Amount = employeeEpf,
                IsPreTax = true,
                IsPostTax = false
            });
        }

        ctx.EmployeeEpf = employeeEpf;
        ctx.EmployerEpf = employerEpf;
        ctx.EmployerEtf = employerEtf;
    }

    private void ApplyPaye(PaySlipCalculationContext ctx, TaxRuleSetDto? taxRuleSet, PayPeriodType periodType)
    {
        var taxableEarnings = ctx.Earnings.Where(e => e.IsTaxable).Sum(e => e.Amount);
        var preTaxDeductions = ctx.Deductions.Where(d => d.IsPreTax).Sum(d => d.Amount);
        var payeResult = CalculatePaye(taxRuleSet, taxableEarnings, preTaxDeductions, periodType);

        if (payeResult.CalculatedTax > 0)
        {
            ctx.Deductions.Add(new DeductionLine
            {
                Id = Guid.NewGuid(),
                PaySlipId = ctx.PaySlipId,
                Code = "PAYE",
                Description = "PAYE Tax",
                Amount = payeResult.CalculatedTax,
                IsPreTax = false,
                IsPostTax = true
            });
        }

        ctx.PayeTax = payeResult.CalculatedTax;
    }

    private PayeComputationResult CalculatePaye(
        TaxRuleSetDto? taxRuleSet,
        decimal taxableEarnings,
        decimal preTaxDeductions,
        PayPeriodType periodType)
    {
        var taxableIncome = taxableEarnings - preTaxDeductions;

        if (taxableIncome <= 0 || taxRuleSet is null || taxRuleSet.Slabs.Count == 0)
        {
            return new PayeComputationResult(0, 0, 0, 0, 0);
        }

        var incomeRelief = taxRuleSet.Reliefs
            .Where(r => r.ReliefType == TaxReliefType.IncomeRelief)
            .Sum(r => GetReliefPortion(r, periodType));

        var taxableAfterRelief = Math.Max(0, taxableIncome - incomeRelief);

        decimal totalTax = 0;
        var sortedSlabs = taxRuleSet.Slabs.OrderBy(s => s.Order).ToList();

        foreach (var slab in sortedSlabs)
        {
            if (taxableAfterRelief <= slab.FromAmount)
            {
                continue;
            }

            var upperBound = slab.ToAmount ?? decimal.MaxValue;
            var chargeable = Math.Min(taxableAfterRelief, upperBound) - slab.FromAmount;
            if (chargeable < 0)
            {
                chargeable = 0;
            }

            totalTax += chargeable * slab.RatePercent / 100m;
        }

        var rebate = taxRuleSet.Reliefs
            .Where(r => r.ReliefType == TaxReliefType.TaxRebate)
            .Sum(r => GetReliefPortion(r, periodType));

        var paye = RoundCurrency(Math.Max(0, totalTax - rebate));

        return new PayeComputationResult(
            RoundCurrency(taxableIncome),
            RoundCurrency(incomeRelief),
            RoundCurrency(rebate),
            RoundCurrency(taxableAfterRelief),
            paye);
    }

    private static decimal GetReliefPortion(TaxReliefDto relief, PayPeriodType periodType)
    {
        return relief.Frequency == TaxReliefFrequency.Monthly
            ? relief.Amount
            : relief.Amount / 12m;
    }

    private static PayRunSummaryDto MapToSummaryDto(PayRun payRun)
    {
        var employeeCount = payRun.PaySlips.Count;
        var totalNet = payRun.PaySlips.Sum(ps => ps.NetPay);

        return new PayRunSummaryDto
        {
            Id = payRun.Id,
            Code = payRun.Code,
            Name = payRun.Name,
            PeriodType = payRun.PeriodType,
            PeriodStart = payRun.PeriodStart,
            PeriodEnd = payRun.PeriodEnd,
            PayDate = payRun.PayDate,
            CompanyId = payRun.CompanyId,
            BranchId = payRun.BranchId,
            CostCenterId = payRun.CostCenterId,
            IsConsolidated = payRun.IsConsolidated,
            Status = payRun.Status,
            ExportStatus = payRun.ExportStatus,
            ExportedBank = payRun.ExportedBank,
            ExportedAt = payRun.ExportedAt,
            ExportDownloadedAt = payRun.ExportDownloadedAt,
            GeneralLedgerStatus = payRun.GeneralLedgerStatus,
            GeneralLedgerReviewedAt = payRun.GeneralLedgerReviewedAt,
            GeneralLedgerReviewedByUserName = payRun.GeneralLedgerReviewedByUserName,
            GeneralLedgerApprovedAt = payRun.GeneralLedgerApprovedAt,
            GeneralLedgerApprovedByUserName = payRun.GeneralLedgerApprovedByUserName,
            GeneralLedgerExportedAt = payRun.GeneralLedgerExportedAt,
            IsLocked = payRun.IsLocked,
            EmployeeCount = employeeCount,
            TotalNetPay = totalNet
        };
    }

    private static PayRunDetailDto MapToDetailDto(PayRun payRun)
    {
        var summary = MapToSummaryDto(payRun);
        return new PayRunDetailDto
        {
            Id = summary.Id,
            Code = summary.Code,
            Name = summary.Name,
            PeriodType = summary.PeriodType,
            PeriodStart = summary.PeriodStart,
            PeriodEnd = summary.PeriodEnd,
            PayDate = summary.PayDate,
            CompanyId = summary.CompanyId,
            BranchId = summary.BranchId,
            CostCenterId = summary.CostCenterId,
            IsConsolidated = summary.IsConsolidated,
            Status = summary.Status,
            ExportStatus = summary.ExportStatus,
            ExportedBank = summary.ExportedBank,
            ExportedAt = summary.ExportedAt,
            ExportDownloadedAt = summary.ExportDownloadedAt,
            IsLocked = summary.IsLocked,
            EmployeeCount = summary.EmployeeCount,
            TotalNetPay = summary.TotalNetPay,
            PaySlips = payRun.PaySlips.Select(MapToDto).ToList(),
            AttendanceNoPaySummaries = payRun.PaySlips
                .Select(paySlip => new AttendanceNoPaySummaryDto
                {
                    EmployeeId = paySlip.EmployeeId,
                    EmployeeCode = paySlip.Employee?.Code,
                    EmployeeName = paySlip.Employee?.FullName,
                    NoPayAmount = paySlip.Deductions
                        .Where(d => d.Code == "NOPAY" && d.Source == "Attendance")
                        .Sum(d => d.Amount)
                })
                .Where(summary => summary.NoPayAmount > 0)
                .ToList(),
            PreparedAt = payRun.PreparedAt,
            PreparedByUserName = payRun.PreparedByUserName,
            ApprovedAt = payRun.ApprovedAt,
            ApprovedByUserName = payRun.ApprovedByUserName,
            LockedAt = payRun.LockedAt,
            LockedByUserName = payRun.LockedByUserName,
            StatusHistory = payRun.Approvals
                .OrderByDescending(a => a.ActionedAt)
                .Select(MapToDto)
                .ToList()
        };
    }

    private static PayRunStatusHistoryDto MapToDto(PayRunApproval approval)
    {
        return new PayRunStatusHistoryDto
        {
            Id = approval.Id,
            FromStatus = approval.FromStatus,
            ToStatus = approval.ToStatus,
            ActorUserId = approval.ActorUserId,
            ActorUserName = approval.ActorUserName,
            Comment = approval.Comment,
            ActionedAt = approval.ActionedAt
        };
    }

    private static PaySlipDto MapToDto(PaySlip paySlip)
    {
        return new PaySlipDto
        {
            Id = paySlip.Id,
            PayRunId = paySlip.PayRunId,
            EmployeeId = paySlip.EmployeeId,
            EmployeeCode = paySlip.Employee?.Code,
            EmployeeName = paySlip.Employee?.FullName,
            Currency = "LKR",
            BasicSalary = paySlip.BasicSalary,
            TotalEarnings = paySlip.TotalEarnings,
            TotalDeductions = paySlip.TotalDeductions,
            NetPay = paySlip.NetPay,
            EmployeeEpf = paySlip.EmployeeEpf,
            EmployerEpf = paySlip.EmployerEpf,
            EmployerEtf = paySlip.EmployerEtf,
            PayeTax = paySlip.PayeTax,
            Earnings = paySlip.Earnings.Select(e => new EarningDto(e.Id, e.Code, e.Description, e.Amount, e.IsEpfApplicable, e.IsEtfApplicable, e.IsTaxable)).ToList(),
            Deductions = paySlip.Deductions.Select(d => new DeductionDto(d.Id, d.Code, d.Description, d.Source, d.Amount, d.IsPreTax, d.IsPostTax)).ToList()
        };
    }

    private static GeneralLedgerAccountMappingDto MapToDto(GeneralLedgerAccountMapping mapping)
    {
        return new GeneralLedgerAccountMappingDto
        {
            Id = mapping.Id,
            Code = mapping.Code,
            Name = mapping.Name,
            MappingType = mapping.MappingType,
            DebitAccount = mapping.DebitAccount,
            CreditAccount = mapping.CreditAccount,
            Notes = mapping.Notes
        };
    }

    private GeneralLedgerExportDto BuildGeneralLedgerExport(PayRun payRun, List<GeneralLedgerAccountMapping> mappings)
    {
        var journal = new Dictionary<(string Debit, string Credit, string Narrative), decimal>(StringComparer.OrdinalIgnoreCase);

        void AddEntry(GeneralLedgerAccountMapping mapping, decimal amount, string narrative)
        {
            if (amount == 0)
            {
                return;
            }

            var key = (mapping.DebitAccount, mapping.CreditAccount, narrative);
            journal[key] = journal.TryGetValue(key, out var existing)
                ? existing + amount
                : amount;
        }

        foreach (var paySlip in payRun.PaySlips)
        {
            foreach (var earning in paySlip.Earnings)
            {
                var mapping = ResolveMapping(mappings, earning.Code, GeneralLedgerMappingType.Earning);
                AddEntry(mapping, earning.Amount, earning.Description);
            }

            foreach (var deduction in paySlip.Deductions)
            {
                var mapping = ResolveMapping(mappings, deduction.Code, GeneralLedgerMappingType.Deduction);
                AddEntry(mapping, deduction.Amount, deduction.Description);
            }

            if (paySlip.EmployerEpf > 0)
            {
                var mapping = TryResolveMapping(mappings, "EPF_ER", GeneralLedgerMappingType.EmployerContribution);
                if (mapping is not null)
                {
                    AddEntry(mapping, paySlip.EmployerEpf, "Employer EPF");
                }
            }

            if (paySlip.EmployerEtf > 0)
            {
                var mapping = TryResolveMapping(mappings, "ETF_ER", GeneralLedgerMappingType.EmployerContribution);
                if (mapping is not null)
                {
                    AddEntry(mapping, paySlip.EmployerEtf, "Employer ETF");
                }
            }
        }

        var netPayTotal = payRun.PaySlips.Sum(ps => ps.NetPay);
        var netPayMapping = TryResolveMapping(mappings, "NET_PAY", GeneralLedgerMappingType.NetPayClearing);
        if (netPayMapping is not null)
        {
            AddEntry(netPayMapping, netPayTotal, "Net pay clearing");
        }

        var entries = journal
            .Select(kvp => new GeneralLedgerJournalEntryDto
            {
                DebitAccount = kvp.Key.Debit,
                CreditAccount = kvp.Key.Credit,
                Narrative = kvp.Key.Narrative,
                Amount = RoundCurrency(kvp.Value)
            })
            .OrderBy(e => e.DebitAccount)
            .ThenBy(e => e.CreditAccount)
            .ThenBy(e => e.Narrative)
            .ToList();

        var total = entries.Sum(e => e.Amount);

        return new GeneralLedgerExportDto
        {
            PayRunId = payRun.Id,
            Status = payRun.GeneralLedgerStatus,
            Entries = entries,
            TotalDebits = total,
            TotalCredits = total,
            IsBalanced = true,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static GeneralLedgerAccountMapping ResolveMapping(IEnumerable<GeneralLedgerAccountMapping> mappings, string code, GeneralLedgerMappingType type)
    {
        var mapping = TryResolveMapping(mappings, code, type);
        if (mapping is null)
        {
            throw new InvalidOperationException($"No general ledger mapping configured for {code} ({type}).");
        }

        return mapping;
    }

    private static GeneralLedgerAccountMapping? TryResolveMapping(IEnumerable<GeneralLedgerAccountMapping> mappings, string code, GeneralLedgerMappingType type)
    {
        var direct = mappings.FirstOrDefault(m => m.MappingType == type && string.Equals(m.Code, code, StringComparison.OrdinalIgnoreCase));
        if (direct is not null)
        {
            return direct;
        }

        return mappings.FirstOrDefault(m => m.MappingType == type && m.Code == "*");
    }

    private static string BuildPayslipHash(PayRun payRun, PaySlip paySlip)
    {
        var material = $"{payRun.Id}|{paySlip.Id}|{paySlip.EmployeeId}|{paySlip.NetPay}|{payRun.PayDate:O}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(hash);
    }

    private static string BuildPaySlipHtml(PayRun payRun, PaySlip paySlip, string authenticityHash)
    {
        var earnings = paySlip.Earnings.OrderBy(e => e.Code).ToList();
        var deductions = paySlip.Deductions.OrderBy(d => d.Code).ToList();
        var builder = new StringBuilder();

        builder.AppendLine("<html><head><style>");
        builder.AppendLine("body { font-family: Arial, sans-serif; color: #1f2937; }");
        builder.AppendLine("h1 { margin-bottom: 4px; }");
        builder.AppendLine(".meta { margin-bottom: 12px; }");
        builder.AppendLine(".grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 8px; }");
        builder.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 8px; }");
        builder.AppendLine("th, td { border: 1px solid #e5e7eb; padding: 6px 8px; text-align: left; }");
        builder.AppendLine("th { background: #f3f4f6; }");
        builder.AppendLine(".totals { margin-top: 12px; }");
        builder.AppendLine(".statutory { margin-top: 12px; }");
        builder.AppendLine(".footer { margin-top: 16px; font-size: 12px; color: #4b5563; }");
        builder.AppendLine("</style></head><body>");

        builder.AppendLine($"<h1>Payslip - {payRun.Name}</h1>");
        builder.AppendLine($"<div class='meta'>Period: {payRun.PeriodStart:yyyy-MM-dd} to {payRun.PeriodEnd:yyyy-MM-dd} | Pay Date: {payRun.PayDate:yyyy-MM-dd}</div>");
        builder.AppendLine("<div class='grid'>");
        builder.AppendLine($"<div><strong>Employee:</strong> {paySlip.Employee?.FullName ?? "N/A"}</div>");
        builder.AppendLine($"<div><strong>Employee Code:</strong> {paySlip.Employee?.EmployeeCode ?? "-"}</div>");
        builder.AppendLine($"<div><strong>Basic Salary:</strong> {paySlip.BasicSalary:N2}</div>");
        builder.AppendLine($"<div><strong>Net Pay:</strong> {paySlip.NetPay:N2}</div>");
        builder.AppendLine("</div>");

        builder.AppendLine("<h3>Earnings</h3>");
        builder.AppendLine("<table><tr><th>Code</th><th>Description</th><th>Amount</th></tr>");
        foreach (var earning in earnings)
        {
            builder.AppendLine($"<tr><td>{earning.Code}</td><td>{earning.Description}</td><td style='text-align:right'>{earning.Amount:N2}</td></tr>");
        }
        builder.AppendLine($"<tr><th colspan='2'>Total Earnings</th><th style='text-align:right'>{paySlip.TotalEarnings:N2}</th></tr></table>");

        builder.AppendLine("<h3>Deductions</h3>");
        builder.AppendLine("<table><tr><th>Code</th><th>Description</th><th>Amount</th></tr>");
        foreach (var deduction in deductions)
        {
            builder.AppendLine($"<tr><td>{deduction.Code}</td><td>{deduction.Description}</td><td style='text-align:right'>-{deduction.Amount:N2}</td></tr>");
        }
        builder.AppendLine($"<tr><th colspan='2'>Total Deductions</th><th style='text-align:right'>-{paySlip.TotalDeductions:N2}</th></tr></table>");

        builder.AppendLine("<div class='totals'><strong>Net Pay:</strong> " + paySlip.NetPay.ToString("N2") + "</div>");

        builder.AppendLine("<div class='statutory'><h3>Statutory Contributions</h3><ul>");
        builder.AppendLine($"<li>Employee EPF: {paySlip.EmployeeEpf:N2}</li>");
        builder.AppendLine($"<li>Employer EPF: {paySlip.EmployerEpf:N2}</li>");
        builder.AppendLine($"<li>Employer ETF: {paySlip.EmployerEtf:N2}</li>");
        builder.AppendLine($"<li>PAYE/APIT Withheld: {paySlip.PayeTax:N2}</li>");
        builder.AppendLine("</ul></div>");

        builder.AppendLine("<div class='footer'>");
        builder.AppendLine($"Authenticity hash: {authenticityHash}<br/>");
        builder.AppendLine($"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        builder.AppendLine("</div>");

        builder.AppendLine("</body></html>");

        return builder.ToString();
    }

    private static byte[] BuildPaySlipPdf(PayRun payRun, PaySlip paySlip, string authenticityHash)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var earnings = paySlip.Earnings.OrderBy(e => e.Code).ToList();
        var deductions = paySlip.Deductions.OrderBy(d => d.Code).ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("Payslip").FontSize(18).SemiBold();
                        column.Item().Text(payRun.Name).FontSize(12).Bold();
                        column.Item().Text($"Period: {payRun.PeriodStart:yyyy-MM-dd} to {payRun.PeriodEnd:yyyy-MM-dd}");
                        column.Item().Text($"Pay Date: {payRun.PayDate:yyyy-MM-dd}");
                    });

                    row.ConstantItem(220).Column(column =>
                    {
                        column.Item().Text("Employee Details").Bold();
                        column.Item().Text($"Name: {paySlip.Employee?.FullName ?? "N/A"}");
                        column.Item().Text($"Code: {paySlip.Employee?.EmployeeCode ?? "-"}");
                        column.Item().Text($"NIC: {paySlip.Employee?.NicNumber ?? "-"}");
                    });
                });

                page.Content().Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(inner =>
                        {
                            inner.Item().Text("Earnings").Bold();
                            inner.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Code").SemiBold();
                                    header.Cell().Text("Description").SemiBold();
                                    header.Cell().Text("Amount").SemiBold().AlignRight();
                                });

                                foreach (var earning in earnings)
                                {
                                    table.Cell().Text(earning.Code);
                                    table.Cell().Text(earning.Description);
                                    table.Cell().AlignRight().Text(earning.Amount.ToString("N2"));
                                }

                                table.Cell().ColumnSpan(2).Text("Total Earnings").SemiBold();
                                table.Cell().AlignRight().Text(paySlip.TotalEarnings.ToString("N2")).SemiBold();
                            });
                        });

                        row.Spacing(10);

                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(inner =>
                        {
                            inner.Item().Text("Deductions").Bold();
                            inner.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Code").SemiBold();
                                    header.Cell().Text("Description").SemiBold();
                                    header.Cell().Text("Amount").SemiBold().AlignRight();
                                });

                                foreach (var deduction in deductions)
                                {
                                    table.Cell().Text(deduction.Code);
                                    table.Cell().Text(deduction.Description);
                                    table.Cell().AlignRight().Text($"-{deduction.Amount:N2}");
                                }

                                table.Cell().ColumnSpan(2).Text("Total Deductions").SemiBold();
                                table.Cell().AlignRight().Text($"-{paySlip.TotalDeductions:N2}").SemiBold();
                            });
                        });
                    });

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(inner =>
                        {
                            inner.Item().Text("Summary").Bold();
                            inner.Item().Text($"Basic Salary: {paySlip.BasicSalary:N2}");
                            inner.Item().Text($"Total Earnings: {paySlip.TotalEarnings:N2}");
                            inner.Item().Text($"Total Deductions: {paySlip.TotalDeductions:N2}");
                            inner.Item().Text($"Net Pay: {paySlip.NetPay:N2}").FontSize(12).Bold();
                        });

                        row.Spacing(10);

                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(inner =>
                        {
                            inner.Item().Text("Statutory Contributions").Bold();
                            inner.Item().Text($"Employee EPF: {paySlip.EmployeeEpf:N2}");
                            inner.Item().Text($"Employer EPF: {paySlip.EmployerEpf:N2}");
                            inner.Item().Text($"Employer ETF: {paySlip.EmployerEtf:N2}");
                            inner.Item().Text($"PAYE/APIT: {paySlip.PayeTax:N2}");
                        });
                    });
                });

                page.Footer().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    column.Item().Text($"Authenticity hash: {authenticityHash}").FontSize(8).Color(Colors.Grey.Darken1);
                    column.Item().Text($"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).Color(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();
    }

    private static decimal RoundCurrency(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal CalculateRoundedOvertimeHours(
        OTEntry overtime,
        PaySlipCalculationContext ctx,
        ref decimal remainingPayRunCap)
    {
        var hours = (decimal)overtime.Hours;

        if (ctx.OvertimeDailyCapHours > 0)
        {
            hours = Math.Min(hours, (decimal)ctx.OvertimeDailyCapHours);
        }

        if (ctx.OvertimePayRunCapHours > 0)
        {
            hours = Math.Min(hours, remainingPayRunCap);
        }

        hours = ApplyOvertimeRounding(hours, ctx.OvertimeRoundingMinutes);

        if (ctx.OvertimeDailyCapHours > 0)
        {
            hours = Math.Min(hours, (decimal)ctx.OvertimeDailyCapHours);
        }

        if (ctx.OvertimePayRunCapHours > 0)
        {
            hours = Math.Min(hours, remainingPayRunCap);
            remainingPayRunCap = Math.Max(0, remainingPayRunCap - hours);
        }

        return hours;
    }

    private static decimal ApplyOvertimeRounding(decimal hours, int roundingMinutes)
    {
        if (roundingMinutes <= 0)
        {
            return hours;
        }

        var minutes = hours * 60m;
        var step = (decimal)roundingMinutes;
        var roundedMinutes = Math.Round(minutes / step, MidpointRounding.AwayFromZero) * step;
        return roundedMinutes / 60m;
    }

    private decimal ResolvePayItemAmount(EmployeePayItem payItem, decimal basicSalary)
    {
        if (payItem.Amount.HasValue)
        {
            return RoundCurrency(payItem.Amount.Value);
        }

        if (payItem.Percentage.HasValue)
        {
            return RoundCurrency(basicSalary * payItem.Percentage.Value / 100m);
        }

        return 0;
    }

    private decimal ResolveRecurringPayItemAmount(EmployeeRecurringPayItem payItem, decimal basicSalary)
    {
        if (payItem.Amount.HasValue)
        {
            return RoundCurrency(payItem.Amount.Value);
        }

        if (payItem.Percentage.HasValue)
        {
            return RoundCurrency(basicSalary * payItem.Percentage.Value / 100m);
        }

        return 0;
    }

    private IQueryable<Employee> ApplyScopeFilter(
        IQueryable<Employee> query,
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId)
    {
        if (companyId.HasValue)
        {
            query = query.Where(e => e.CompanyId == companyId);
        }

        if (branchId.HasValue)
        {
            query = query.Where(e => e.BranchId == branchId);
        }

        if (costCenterId.HasValue)
        {
            query = query.Where(e => e.CostCenterId == costCenterId);
        }

        return query;
    }

    private async Task<(Guid? CompanyId, Guid? BranchId, Guid? CostCenterId)> ValidateOrganizationScopeAsync(
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId,
        CancellationToken cancellationToken)
    {
        Guid? validatedCompanyId = companyId;
        Guid? validatedBranchId = branchId;

        if (validatedCompanyId.HasValue)
        {
            var companyExists = await _dbContext.Companies.AsNoTracking()
                .AnyAsync(c => c.Id == validatedCompanyId.Value, cancellationToken);

            if (!companyExists)
            {
                throw new InvalidOperationException("Company scope was not found.");
            }
        }

        if (validatedBranchId.HasValue)
        {
            var branch = await _dbContext.Branches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == validatedBranchId.Value, cancellationToken);

            if (branch is null)
            {
                throw new InvalidOperationException("Branch scope was not found.");
            }

            if (validatedCompanyId.HasValue && branch.CompanyId != validatedCompanyId.Value)
            {
                throw new InvalidOperationException("Branch does not belong to the specified company.");
            }

            validatedCompanyId ??= branch.CompanyId;
        }

        if (costCenterId.HasValue)
        {
            var costCenter = await _dbContext.CostCenters.AsNoTracking()
                .FirstOrDefaultAsync(cc => cc.Id == costCenterId.Value, cancellationToken);

            if (costCenter is null)
            {
                throw new InvalidOperationException("Cost center scope was not found.");
            }

            if (costCenter.BranchId.HasValue)
            {
                if (validatedBranchId.HasValue && costCenter.BranchId != validatedBranchId.Value)
                {
                    throw new InvalidOperationException("Cost center does not belong to the specified branch.");
                }

                validatedBranchId ??= costCenter.BranchId;
            }

            if (costCenter.CompanyId.HasValue)
            {
                if (validatedCompanyId.HasValue && costCenter.CompanyId != validatedCompanyId.Value)
                {
                    throw new InvalidOperationException("Cost center does not belong to the specified company.");
                }

                validatedCompanyId ??= costCenter.CompanyId;
            }
        }

        if (validatedBranchId.HasValue && !validatedCompanyId.HasValue)
        {
            validatedCompanyId = await _dbContext.Branches.AsNoTracking()
                .Where(b => b.Id == validatedBranchId.Value)
                .Select(b => b.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return (validatedCompanyId, validatedBranchId, costCenterId);
    }

    private sealed record AttendanceAbsenceSummary(
        Dictionary<DateOnly, decimal> DayUnits,
        Dictionary<DateOnly, decimal> MissingHours);

    private sealed class PaySlipCalculationContext
    {
        public Guid PaySlipId { get; init; }
        public Employee Employee { get; init; } = null!;
        public decimal BasicSalary { get; set; }
        public List<AttendanceRecord> Attendance { get; init; } = new();
        public List<OTEntry> Overtime { get; init; } = new();
        public List<Loan> ActiveLoans { get; init; } = new();
        public List<EmployeePayItem> PayItems { get; init; } = new();
        public List<EmployeeRecurringPayItem> RecurringPayItems { get; init; } = new();
        public List<RecurringRule> RecurringRules { get; init; } = new();
        public List<LeaveRequest> LeaveRequests { get; init; } = new();
        public IReadOnlyDictionary<string, AllowanceType> AllowanceTypes { get; init; } = new Dictionary<string, AllowanceType>();
        public IReadOnlyDictionary<string, DeductionType> DeductionTypes { get; init; } = new Dictionary<string, DeductionType>();
        public List<EarningLine> Earnings { get; } = new();
        public List<DeductionLine> Deductions { get; } = new();
        public decimal EmployeeEpf { get; set; }
        public decimal EmployerEpf { get; set; }
        public decimal EmployerEtf { get; set; }
        public decimal PayeTax { get; set; }
        public decimal TotalEarnings => Earnings.Sum(x => x.Amount);
        public decimal TotalDeductions => Deductions.Sum(x => x.Amount);
        public int WorkingDaysPerMonth { get; init; }
        public int WorkingHoursPerDay { get; init; }
        public CalculationBasis NoPayCalculationBasis { get; init; }
        public decimal AttendanceHalfDayHours { get; init; }
        public decimal WeekdayOvertimeMultiplier { get; init; }
        public decimal WeekendOvertimeMultiplier { get; init; }
        public decimal HolidayOvertimeMultiplier { get; init; }
        public int OvertimeRoundingMinutes { get; init; }
        public double OvertimeDailyCapHours { get; init; }
        public double OvertimePayRunCapHours { get; init; }
        public bool AppliesOnWeekend { get; init; }
        public bool AppliesOnHoliday { get; init; }
    }

    private sealed record PayeComputationResult(
        decimal TaxableIncome,
        decimal ReliefAmount,
        decimal RebateAmount,
        decimal TaxableAfterRelief,
        decimal CalculatedTax);

    private sealed class PayrollSettingsSnapshot
    {
        public int WorkingDaysPerMonth { get; init; }
        public int WorkingHoursPerDay { get; init; }
        public CalculationBasis NoPayCalculationBasis { get; init; }
        public decimal AttendanceHalfDayHours { get; init; }
        public decimal WeekdayOvertimeMultiplier { get; init; }
        public decimal WeekendOvertimeMultiplier { get; init; }
        public decimal HolidayOvertimeMultiplier { get; init; }
        public int OvertimeRoundingMinutes { get; init; }
        public double OvertimeDailyCapHours { get; init; }
        public double OvertimePayRunCapHours { get; init; }
    }

    private sealed class OvertimeRuleSnapshot
    {
        public decimal WeekdayOvertimeMultiplier { get; init; }
        public decimal WeekendOvertimeMultiplier { get; init; }
        public decimal HolidayOvertimeMultiplier { get; init; }
        public int OvertimeRoundingMinutes { get; init; }
        public double OvertimeDailyCapHours { get; init; }
        public double OvertimePayRunCapHours { get; init; }
        public bool AppliesOnWeekend { get; init; }
        public bool AppliesOnHoliday { get; init; }
    }

    // TODO: Add integration tests to cover basic, overtime, and statutory calculation scenarios.
}
