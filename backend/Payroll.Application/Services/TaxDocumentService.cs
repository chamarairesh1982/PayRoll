using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.DTOs;
using Payroll.Application.Exceptions;
using Payroll.Application.Interfaces;
using Payroll.Domain.Employees;
using Payroll.Domain.Organizations;
using Payroll.Domain.Payroll;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Payroll.Application.Services;

public class TaxDocumentService : ITaxDocumentService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly IPayrollDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITaxDocumentStorage _storage;

    public TaxDocumentService(
        IPayrollDbContext dbContext,
        ICurrentUserService currentUserService,
        ITaxDocumentStorage storage)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _storage = storage;
    }

    public async Task<TaxDocumentMetadataDto> GenerateMonthlyReportAsync(
        MonthlyTaxReportRequestDto request,
        bool regenerate,
        CancellationToken cancellationToken = default)
    {
        EnsureTaxRole("generate APIT reports");
        ValidateMonth(request.Month);

        await ValidateOrganizationScopeAsync(request.CompanyId, request.BranchId, request.CostCenterId, cancellationToken);

        var periodStart = new DateTime(request.Year, request.Month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var existing = await FindExistingDocumentAsync(
            GeneratedTaxDocumentType.MonthlyReport,
            periodStart,
            periodEnd,
            request.Year,
            null,
            request.CompanyId,
            request.BranchId,
            request.CostCenterId,
            cancellationToken);

        if (existing is not null && !regenerate)
        {
            return MapMetadata(existing);
        }

        var payRuns = await LoadPayRunsAsync(periodStart, periodEnd, request.CompanyId, request.BranchId, request.CostCenterId, cancellationToken);

        if (payRuns.Count == 0)
        {
            throw new InvalidOperationException("No locked pay runs found for the selected month.");
        }

        var report = BuildAggregatedReport(payRuns);
        report.Title = $"Monthly APIT Report - {periodStart:MMMM yyyy}";
        report.PeriodStart = periodStart;
        report.PeriodEnd = periodEnd;

        var format = NormalizeFormat(request.Format);
        var bytes = format == "pdf"
            ? BuildMonthlyPdf(report)
            : Encoding.UTF8.GetBytes(BuildMonthlyCsv(report));

        return await SaveDocumentAsync(
            GeneratedTaxDocumentType.MonthlyReport,
            format == "pdf" ? "application/pdf" : "text/csv",
            bytes,
            periodStart,
            periodEnd,
            request.Year,
            null,
            request.CompanyId,
            request.BranchId,
            request.CostCenterId,
            report,
            cancellationToken);
    }

    public async Task<TaxDocumentMetadataDto> GenerateAnnualReportAsync(
        AnnualTaxReportRequestDto request,
        bool regenerate,
        CancellationToken cancellationToken = default)
    {
        EnsureTaxRole("generate APIT reports");

        await ValidateOrganizationScopeAsync(request.CompanyId, request.BranchId, request.CostCenterId, cancellationToken);

        var periodStart = new DateTime(request.Year, 1, 1);
        var periodEnd = new DateTime(request.Year, 12, 31);

        var existing = await FindExistingDocumentAsync(
            GeneratedTaxDocumentType.AnnualReport,
            periodStart,
            periodEnd,
            request.Year,
            null,
            request.CompanyId,
            request.BranchId,
            request.CostCenterId,
            cancellationToken);

        if (existing is not null && !regenerate)
        {
            return MapMetadata(existing);
        }

        var payRuns = await LoadPayRunsAsync(periodStart, periodEnd, request.CompanyId, request.BranchId, request.CostCenterId, cancellationToken);

        if (payRuns.Count == 0)
        {
            throw new InvalidOperationException("No locked pay runs found for the selected year.");
        }

        var report = BuildAggregatedReport(payRuns);
        report.Title = $"Annual APIT Report - {request.Year}";
        report.PeriodStart = periodStart;
        report.PeriodEnd = periodEnd;
        report.IsCalendarYear = true;

        var format = NormalizeFormat(request.Format);
        var bytes = format == "pdf"
            ? BuildAnnualPdf(report)
            : Encoding.UTF8.GetBytes(BuildAnnualCsv(report));

        return await SaveDocumentAsync(
            GeneratedTaxDocumentType.AnnualReport,
            format == "pdf" ? "application/pdf" : "text/csv",
            bytes,
            periodStart,
            periodEnd,
            request.Year,
            null,
            request.CompanyId,
            request.BranchId,
            request.CostCenterId,
            report,
            cancellationToken);
    }

    public async Task<TaxDocumentMetadataDto> GenerateEmployeeCertificateAsync(
        TaxCertificateRequestDto request,
        bool regenerate,
        CancellationToken cancellationToken = default)
    {
        EnsureTaxRole("generate APIT certificates");

        var periodStart = new DateTime(request.Year, 1, 1);
        var periodEnd = new DateTime(request.Year, 12, 31);

        var employeeRecord = await _dbContext.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employeeRecord is null)
        {
            throw new KeyNotFoundException("Employee not found.");
        }

        var existing = await FindExistingDocumentAsync(
            GeneratedTaxDocumentType.EmployeeCertificate,
            periodStart,
            periodEnd,
            request.Year,
            request.EmployeeId,
            employeeRecord.CompanyId,
            null,
            null,
            cancellationToken);

        if (existing is not null && !regenerate)
        {
            return MapMetadata(existing);
        }

        var payRuns = await LoadPayRunsAsync(periodStart, periodEnd, null, null, null, cancellationToken);
        var paySlips = payRuns.SelectMany(pr => pr.PaySlips)
            .Where(ps => ps.EmployeeId == request.EmployeeId)
            .ToList();

        if (paySlips.Count == 0)
        {
            throw new InvalidOperationException("Employee has no locked tax data for the selected year.");
        }

        var employee = paySlips.Select(ps => ps.Employee).FirstOrDefault(e => e != null) ?? employeeRecord;
        var companyId = employee.CompanyId;
        Company? company = null;

        if (companyId.HasValue)
        {
            company = await _dbContext.Companies.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == companyId.Value, cancellationToken);
        }

        var totals = BuildEmployeeTotals(paySlips);
        var slabSetReferences = await ResolveSlabSetNamesAsync(totals.SlabSetIds, cancellationToken);

        var certificate = new EmployeeCertificatePayload
        {
            Employee = employee,
            Company = company,
            Year = request.Year,
            Totals = totals,
            SlabSetReferences = slabSetReferences,
            PayRunIds = payRuns.Select(pr => pr.Id).Distinct().ToList(),
            PayRunCodes = payRuns.Select(pr => pr.Code).Where(code => !string.IsNullOrWhiteSpace(code)).Distinct().ToList()
        };

        var pdfBytes = BuildCertificatePdf(certificate);

        return await SaveDocumentAsync(
            GeneratedTaxDocumentType.EmployeeCertificate,
            "application/pdf",
            pdfBytes,
            periodStart,
            periodEnd,
            request.Year,
            request.EmployeeId,
            company?.Id,
            null,
            null,
            certificate,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TaxDocumentHistoryDto>> GetDocumentsAsync(
        GeneratedTaxDocumentType? type = null,
        int? year = null,
        Guid? employeeId = null,
        CancellationToken cancellationToken = default)
    {
        EnsureTaxRole("view tax document history");

        var query = _dbContext.GeneratedTaxDocuments
            .AsNoTracking()
            .Include(doc => doc.Employee)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(doc => doc.Type == type.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(doc => doc.Year == year.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(doc => doc.EmployeeId == employeeId.Value);
        }

        var documents = await query
            .OrderByDescending(doc => doc.GeneratedAtUtc)
            .ToListAsync(cancellationToken);

        return documents.Select(doc => new TaxDocumentHistoryDto
        {
            Id = doc.Id,
            Type = doc.Type,
            PeriodStart = doc.PeriodStart,
            PeriodEnd = doc.PeriodEnd,
            Year = doc.Year,
            EmployeeId = doc.EmployeeId,
            EmployeeCode = doc.Employee?.EmployeeCode,
            EmployeeName = doc.Employee?.FullName,
            Status = doc.Status,
            GeneratedAtUtc = doc.GeneratedAtUtc,
            FileName = doc.FileName
        }).ToList();
    }

    public async Task<FileExportResultDto?> DownloadAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        EnsureTaxRole("download tax documents");

        var document = await _dbContext.GeneratedTaxDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(doc => doc.Id == documentId, cancellationToken);

        if (document is null)
        {
            return null;
        }

        if (document.Status != GeneratedTaxDocumentStatus.Generated)
        {
            throw new InvalidOperationException("Tax document is not available.");
        }

        if (string.IsNullOrWhiteSpace(document.FilePath) || string.IsNullOrWhiteSpace(document.FileName))
        {
            throw new InvalidOperationException("Tax document file path is missing.");
        }

        var bytes = await _storage.ReadAsync(document.FilePath, cancellationToken);

        return new FileExportResultDto
        {
            FileName = document.FileName,
            ContentType = string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType,
            ContentBase64 = Convert.ToBase64String(bytes)
        };
    }

    private async Task<TaxDocumentMetadataDto> SaveDocumentAsync(
        GeneratedTaxDocumentType type,
        string contentType,
        byte[] bytes,
        DateTime periodStart,
        DateTime periodEnd,
        int year,
        Guid? employeeId,
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId,
        object metadataPayload,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var actor = GetActor();
        var document = new GeneratedTaxDocument
        {
            Id = Guid.NewGuid(),
            Type = type,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Year = year,
            EmployeeId = employeeId,
            CompanyId = companyId,
            BranchId = branchId,
            CostCenterId = costCenterId,
            Status = GeneratedTaxDocumentStatus.Pending,
            GeneratedAtUtc = now,
            GeneratedByUserId = actor.UserId,
            GeneratedByUserName = actor.UserName,
            CreatedBy = actor.UserName
        };

        _dbContext.GeneratedTaxDocuments.Add(document);

        try
        {
            var version = await GetNextVersionAsync(type, periodStart, periodEnd, year, employeeId, companyId, branchId, costCenterId, cancellationToken);
            var timestamp = now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var extension = contentType == "application/pdf" ? "pdf" : "csv";
            var fileName = BuildFileName(type, year, employeeId, version, timestamp, extension);
            var filePath = await _storage.SaveAsync(fileName, bytes, cancellationToken);

            document.Status = GeneratedTaxDocumentStatus.Generated;
            document.FileName = fileName;
            document.FilePath = filePath;
            document.ContentType = contentType;
            document.ChecksumSha256 = ComputeChecksum(bytes);
            document.MetadataJson = JsonSerializer.Serialize(metadataPayload, JsonOptions);
            document.ErrorSummary = null;
        }
        catch (Exception ex)
        {
            document.Status = GeneratedTaxDocumentStatus.Failed;
            document.ErrorSummary = ex.Message;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapMetadata(document);
    }

    private async Task<GeneratedTaxDocument?> FindExistingDocumentAsync(
        GeneratedTaxDocumentType type,
        DateTime periodStart,
        DateTime periodEnd,
        int year,
        Guid? employeeId,
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.GeneratedTaxDocuments
            .AsNoTracking()
            .Where(doc => doc.Type == type
                && doc.Year == year
                && doc.PeriodStart == periodStart
                && doc.PeriodEnd == periodEnd
                && doc.EmployeeId == employeeId
                && doc.CompanyId == companyId
                && doc.BranchId == branchId
                && doc.CostCenterId == costCenterId
                && doc.Status == GeneratedTaxDocumentStatus.Generated)
            .OrderByDescending(doc => doc.GeneratedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<int> GetNextVersionAsync(
        GeneratedTaxDocumentType type,
        DateTime periodStart,
        DateTime periodEnd,
        int year,
        Guid? employeeId,
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId,
        CancellationToken cancellationToken)
    {
        var count = await _dbContext.GeneratedTaxDocuments
            .AsNoTracking()
            .CountAsync(doc => doc.Type == type
                && doc.Year == year
                && doc.PeriodStart == periodStart
                && doc.PeriodEnd == periodEnd
                && doc.EmployeeId == employeeId
                && doc.CompanyId == companyId
                && doc.BranchId == branchId
                && doc.CostCenterId == costCenterId, cancellationToken);

        return count + 1;
    }

    private static string BuildFileName(
        GeneratedTaxDocumentType type,
        int year,
        Guid? employeeId,
        int version,
        string timestamp,
        string extension)
    {
        var prefix = type switch
        {
            GeneratedTaxDocumentType.MonthlyReport => "APIT-Monthly",
            GeneratedTaxDocumentType.AnnualReport => "APIT-Annual",
            GeneratedTaxDocumentType.EmployeeCertificate => "APIT-Certificate",
            _ => "APIT-Document"
        };

        var employeeSuffix = employeeId.HasValue ? $"-{employeeId.Value.ToString("N")[..8]}" : string.Empty;
        return $"{prefix}-{year}{employeeSuffix}-v{version}-{timestamp}.{extension}";
    }

    private static string NormalizeFormat(string? format)
    {
        var normalized = string.IsNullOrWhiteSpace(format) ? "csv" : format.Trim().ToLowerInvariant();
        if (normalized is not ("csv" or "pdf"))
        {
            throw new ArgumentException("Format must be csv or pdf.");
        }

        return normalized;
    }

    private static void ValidateMonth(int month)
    {
        if (month < 1 || month > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12.");
        }
    }

    private async Task ValidateOrganizationScopeAsync(
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId,
        CancellationToken cancellationToken)
    {
        if (companyId.HasValue)
        {
            var companyExists = await _dbContext.Companies.AsNoTracking()
                .AnyAsync(c => c.Id == companyId.Value, cancellationToken);

            if (!companyExists)
            {
                throw new InvalidOperationException("Company scope was not found.");
            }
        }

        if (branchId.HasValue)
        {
            var branchExists = await _dbContext.Branches.AsNoTracking()
                .AnyAsync(b => b.Id == branchId.Value, cancellationToken);

            if (!branchExists)
            {
                throw new InvalidOperationException("Branch scope was not found.");
            }
        }

        if (costCenterId.HasValue)
        {
            var costCenterExists = await _dbContext.CostCenters.AsNoTracking()
                .AnyAsync(cc => cc.Id == costCenterId.Value, cancellationToken);

            if (!costCenterExists)
            {
                throw new InvalidOperationException("Cost center scope was not found.");
            }
        }
    }

    private async Task<List<PayRun>> LoadPayRunsAsync(
        DateTime periodStart,
        DateTime periodEnd,
        Guid? companyId,
        Guid? branchId,
        Guid? costCenterId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.PayRuns
            .AsNoTracking()
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Employee)
            .Include(pr => pr.PaySlips)
                .ThenInclude(ps => ps.Earnings)
            .Where(pr => pr.PayDate >= periodStart
                && pr.PayDate <= periodEnd
                && (pr.Status == PayRunStatus.Locked || pr.IsLocked));

        if (companyId.HasValue)
        {
            query = query.Where(pr => pr.CompanyId == companyId.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(pr => pr.BranchId == branchId.Value);
        }

        if (costCenterId.HasValue)
        {
            query = query.Where(pr => pr.CostCenterId == costCenterId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private static AggregatedReport BuildAggregatedReport(IEnumerable<PayRun> payRuns)
    {
        var aggregates = new Dictionary<Guid, AggregatedEmployeeRow>();
        var payRunRefs = new Dictionary<Guid, HashSet<string>>();
        var payRunIds = new HashSet<Guid>();
        var payRunCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var payRun in payRuns)
        {
            payRunIds.Add(payRun.Id);
            if (!string.IsNullOrWhiteSpace(payRun.Code))
            {
                payRunCodes.Add(payRun.Code);
            }

            foreach (var paySlip in payRun.PaySlips)
            {
                var summary = ParseTaxSummary(paySlip);
                var taxableEarnings = summary?.TaxableEarnings ?? paySlip.Earnings.Where(e => e.IsTaxable).Sum(e => e.Amount);
                var reliefTotal = summary?.ReliefTotal ?? 0m;
                var taxableBase = summary?.TaxableBase ?? Math.Max(0m, taxableEarnings - reliefTotal);

                if (!aggregates.TryGetValue(paySlip.EmployeeId, out var row))
                {
                    row = new AggregatedEmployeeRow
                    {
                        EmployeeId = paySlip.EmployeeId,
                        EmployeeCode = paySlip.Employee?.EmployeeCode,
                        EmployeeName = paySlip.Employee?.FullName,
                        NicNumber = paySlip.Employee?.NicNumber
                    };
                    aggregates[paySlip.EmployeeId] = row;
                    payRunRefs[paySlip.EmployeeId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                row.TaxableEarnings += taxableEarnings;
                row.ReliefTotal += reliefTotal;
                row.TaxableBase += taxableBase;
                row.ApitDeducted += paySlip.PayeTax;

                if (!string.IsNullOrWhiteSpace(payRun.Code))
                {
                    payRunRefs[paySlip.EmployeeId].Add(payRun.Code);
                }

                if (summary?.SlabSetId.HasValue == true)
                {
                    row.SlabSetIds.Add(summary.SlabSetId.Value);
                }
            }
        }

        foreach (var row in aggregates.Values)
        {
            if (payRunRefs.TryGetValue(row.EmployeeId, out var runs))
            {
                row.PayRunCodes = runs.OrderBy(code => code).ToList();
            }
        }

        return new AggregatedReport
        {
            PayRunIds = payRunIds.ToList(),
            PayRunCodes = payRunCodes.OrderBy(code => code).ToList(),
            Employees = aggregates.Values
                .OrderBy(r => r.EmployeeCode ?? r.EmployeeName ?? r.EmployeeId.ToString())
                .Select(r => r.Round())
                .ToList()
        };
    }

    private static TaxCalculationSummaryDto? ParseTaxSummary(PaySlip paySlip)
    {
        if (string.IsNullOrWhiteSpace(paySlip.TaxCalculationJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TaxCalculationSummaryDto>(paySlip.TaxCalculationJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static EmployeeTotals BuildEmployeeTotals(IEnumerable<PaySlip> paySlips)
    {
        var totals = new EmployeeTotals();

        foreach (var paySlip in paySlips)
        {
            var summary = ParseTaxSummary(paySlip);
            var taxableEarnings = summary?.TaxableEarnings ?? paySlip.Earnings.Where(e => e.IsTaxable).Sum(e => e.Amount);
            var reliefTotal = summary?.ReliefTotal ?? 0m;
            var taxableBase = summary?.TaxableBase ?? Math.Max(0m, taxableEarnings - reliefTotal);

            totals.TaxableEarnings += taxableEarnings;
            totals.ReliefTotal += reliefTotal;
            totals.TaxableBase += taxableBase;
            totals.ApitDeducted += paySlip.PayeTax;
            totals.PayRunCount++;

            if (summary?.SlabSetId.HasValue == true)
            {
                totals.SlabSetIds.Add(summary.SlabSetId.Value);
            }
        }

        totals.Round();
        return totals;
    }

    private async Task<IReadOnlyList<string>> ResolveSlabSetNamesAsync(IEnumerable<Guid> slabSetIds, CancellationToken cancellationToken)
    {
        var ids = slabSetIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return Array.Empty<string>();
        }

        return await _dbContext.TaxRuleSets
            .AsNoTracking()
            .Where(rs => ids.Contains(rs.Id))
            .Select(rs => $"{rs.Name} ({rs.YearOfAssessment})")
            .ToListAsync(cancellationToken);
    }

    private static string BuildMonthlyCsv(AggregatedReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("EmployeeCode,FullName,NIC,TaxableEarnings,ReliefTotal,TaxableBase,ApitDeducted,PayRunCodes");

        foreach (var row in report.Employees)
        {
            builder.AppendLine(string.Join(",",
                Escape(row.EmployeeCode),
                Escape(row.EmployeeName),
                Escape(row.NicNumber),
                row.TaxableEarnings.ToString("F2", CultureInfo.InvariantCulture),
                row.ReliefTotal.ToString("F2", CultureInfo.InvariantCulture),
                row.TaxableBase.ToString("F2", CultureInfo.InvariantCulture),
                row.ApitDeducted.ToString("F2", CultureInfo.InvariantCulture),
                Escape(string.Join(" | ", row.PayRunCodes))));
        }

        return builder.ToString();
    }

    private static string BuildAnnualCsv(AggregatedReport report)
    {
        return BuildMonthlyCsv(report);
    }

    private static byte[] BuildMonthlyPdf(AggregatedReport report)
    {
        return BuildSummaryPdf(report, "Monthly APIT Report");
    }

    private static byte[] BuildAnnualPdf(AggregatedReport report)
    {
        return BuildSummaryPdf(report, "Annual APIT Report");
    }

    private static byte[] BuildSummaryPdf(AggregatedReport report, string subtitle)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Content().Column(column =>
                {
                    column.Item().Text(report.Title).FontSize(18).SemiBold();
                    column.Item().Text(subtitle).FontSize(12).FontColor(Colors.Grey.Darken1);
                    column.Item().Text($"{report.PeriodStart:yyyy-MM-dd} to {report.PeriodEnd:yyyy-MM-dd}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2);
                    if (report.IsCalendarYear)
                    {
                        column.Item().Text("Calendar year basis (Jan 1 - Dec 31).")
                            .FontSize(9).FontColor(Colors.Grey.Darken2);
                    }

                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Emp Code").SemiBold();
                            header.Cell().Text("Employee").SemiBold();
                            header.Cell().Text("NIC").SemiBold();
                            header.Cell().AlignRight().Text("Taxable").SemiBold();
                            header.Cell().AlignRight().Text("Relief").SemiBold();
                            header.Cell().AlignRight().Text("APIT").SemiBold();
                        });

                        foreach (var row in report.Employees)
                        {
                            table.Cell().Text(row.EmployeeCode ?? "-");
                            table.Cell().Text(row.EmployeeName ?? "-");
                            table.Cell().Text(row.NicNumber ?? "-");
                            table.Cell().AlignRight().Text(row.TaxableEarnings.ToString("N2"));
                            table.Cell().AlignRight().Text(row.ReliefTotal.ToString("N2"));
                            table.Cell().AlignRight().Text(row.ApitDeducted.ToString("N2"));
                        }
                    });
                });
            });
        }).GeneratePdf();
    }

    private static byte[] BuildCertificatePdf(EmployeeCertificatePayload payload)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var now = DateTime.UtcNow;
        var checksum = ComputeChecksum(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, JsonOptions)));
        payload.GeneratedAtUtc = now;
        payload.Checksum = checksum;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(32);
                page.Size(PageSizes.A4);
                page.Content().Column(column =>
                {
                    column.Item().Text("APIT/PAYE Tax Certificate").FontSize(18).SemiBold();
                    column.Item().Text($"Year: {payload.Year}").FontSize(12);

                    column.Item().PaddingVertical(8).Text("Employee Details").SemiBold();
                    column.Item().Text($"Name: {payload.Employee?.FullName ?? "N/A"}");
                    column.Item().Text($"Employee Code: {payload.Employee?.EmployeeCode ?? payload.Employee?.Id.ToString() ?? "-"}");
                    column.Item().Text($"NIC: {payload.Employee?.NicNumber ?? "-"}");

                    column.Item().PaddingVertical(8).Text("Employer Details").SemiBold();
                    column.Item().Text($"Company: {payload.Company?.Name ?? "N/A"}");

                    column.Item().PaddingVertical(8).Text("Tax Summary").SemiBold();
                    column.Item().Text($"Taxable Earnings: {payload.Totals.TaxableEarnings:N2}");
                    column.Item().Text($"Reliefs: {payload.Totals.ReliefTotal:N2}");
                    column.Item().Text($"Taxable Base: {payload.Totals.TaxableBase:N2}");
                    column.Item().Text($"APIT/PAYE Deducted: {payload.Totals.ApitDeducted:N2}");

                    if (payload.SlabSetReferences.Count > 0)
                    {
                        column.Item().PaddingTop(6).Text("Slab Set References").SemiBold();
                        foreach (var reference in payload.SlabSetReferences)
                        {
                            column.Item().Text(reference);
                        }
                    }

                    column.Item().PaddingTop(12).Text("Authenticity").SemiBold();
                    column.Item().Text($"Generated (UTC): {payload.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss}");
                    column.Item().Text($"Checksum: {payload.Checksum}");
                });
            });
        }).GeneratePdf();
    }

    private static string ComputeChecksum(byte[] bytes)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var sanitized = value.Replace("\"", "\"\"");
        return $"\"{sanitized}\"";
    }

    private static decimal RoundCurrency(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private TaxDocumentMetadataDto MapMetadata(GeneratedTaxDocument document)
    {
        return new TaxDocumentMetadataDto
        {
            Id = document.Id,
            Type = document.Type,
            PeriodStart = document.PeriodStart,
            PeriodEnd = document.PeriodEnd,
            Year = document.Year,
            EmployeeId = document.EmployeeId,
            Status = document.Status,
            GeneratedAtUtc = document.GeneratedAtUtc,
            FileName = document.FileName,
            ChecksumSha256 = document.ChecksumSha256,
            DownloadUrl = document.Status == GeneratedTaxDocumentStatus.Generated
                ? $"/api/tax/documents/{document.Id}/download"
                : null,
            ErrorSummary = document.ErrorSummary
        };
    }

    private void EnsureTaxRole(string action)
    {
        var hasRole = _currentUserService.Roles.Any(role =>
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Payroll", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Finance", StringComparison.OrdinalIgnoreCase));

        if (!hasRole)
        {
            throw new ForbiddenAccessException($"Only payroll, finance, or admins can {action}.");
        }
    }

    private (string UserId, string UserName) GetActor()
    {
        var userName = string.IsNullOrWhiteSpace(_currentUserService.UserName) ? "Unknown" : _currentUserService.UserName!;
        var userId = _currentUserService.UserId ?? string.Empty;
        return (userId, userName);
    }

    private sealed class AggregatedReport
    {
        public string Title { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public bool IsCalendarYear { get; set; }
        public List<Guid> PayRunIds { get; set; } = new();
        public List<string> PayRunCodes { get; set; } = new();
        public List<AggregatedEmployeeRow> Employees { get; set; } = new();
    }

    private sealed class AggregatedEmployeeRow
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? NicNumber { get; set; }
        public decimal TaxableEarnings { get; set; }
        public decimal ReliefTotal { get; set; }
        public decimal TaxableBase { get; set; }
        public decimal ApitDeducted { get; set; }
        public List<string> PayRunCodes { get; set; } = new();
        public HashSet<Guid> SlabSetIds { get; } = new();

        public AggregatedEmployeeRow Round()
        {
            TaxableEarnings = RoundCurrency(TaxableEarnings);
            ReliefTotal = RoundCurrency(ReliefTotal);
            TaxableBase = RoundCurrency(TaxableBase);
            ApitDeducted = RoundCurrency(ApitDeducted);
            return this;
        }
    }

    private sealed class EmployeeTotals
    {
        public decimal TaxableEarnings { get; set; }
        public decimal ReliefTotal { get; set; }
        public decimal TaxableBase { get; set; }
        public decimal ApitDeducted { get; set; }
        public int PayRunCount { get; set; }
        public HashSet<Guid> SlabSetIds { get; } = new();

        public void Round()
        {
            TaxableEarnings = RoundCurrency(TaxableEarnings);
            ReliefTotal = RoundCurrency(ReliefTotal);
            TaxableBase = RoundCurrency(TaxableBase);
            ApitDeducted = RoundCurrency(ApitDeducted);
        }
    }

    private sealed class EmployeeCertificatePayload
    {
        public Employee? Employee { get; set; }
        public Company? Company { get; set; }
        public int Year { get; set; }
        public EmployeeTotals Totals { get; set; } = new();
        public IReadOnlyList<string> SlabSetReferences { get; set; } = Array.Empty<string>();
        public List<Guid> PayRunIds { get; set; } = new();
        public List<string> PayRunCodes { get; set; } = new();
        public DateTime GeneratedAtUtc { get; set; }
        public string? Checksum { get; set; }
    }
}
