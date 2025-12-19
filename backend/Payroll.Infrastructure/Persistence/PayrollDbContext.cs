using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.GeneralLedger;
using Payroll.Domain.Leave;
using Payroll.Domain.Loans;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;
using Payroll.Domain.Auditing;
using Payroll.Domain.Organizations;

namespace Payroll.Infrastructure.Persistence;

public class PayrollDbContext : DbContext, IPayrollDbContext
{
    public PayrollDbContext(DbContextOptions<PayrollDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayRun> PayRuns => Set<PayRun>();
    public DbSet<PaySlip> PaySlips => Set<PaySlip>();
    public DbSet<PayRunStatusHistory> PayRunStatusHistories => Set<PayRunStatusHistory>();
    public DbSet<StatutoryReport> StatutoryReports => Set<StatutoryReport>();
    public DbSet<RecurringRule> RecurringRules => Set<RecurringRule>();
    public DbSet<RecurringPayItemRule> RecurringPayItemRules => Set<RecurringPayItemRule>();
    public DbSet<RecurringPayItemAssignment> RecurringPayItemAssignments => Set<RecurringPayItemAssignment>();
    public DbSet<PayRunRecurringLine> PayRunRecurringLines => Set<PayRunRecurringLine>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<OTEntry> OTEntries => Set<OTEntry>();
    public DbSet<OTRule> OTRules => Set<OTRule>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanRepayment> LoanRepayments => Set<LoanRepayment>();
    public DbSet<AllowanceType> AllowanceTypes => Set<AllowanceType>();
    public DbSet<DeductionType> DeductionTypes => Set<DeductionType>();
    public DbSet<EmployeeRecurringPayItem> EmployeeRecurringPayItems => Set<EmployeeRecurringPayItem>();
    public DbSet<EpfEtfRuleSet> EpfEtfRuleSets => Set<EpfEtfRuleSet>();
    public DbSet<TaxRuleSet> TaxRuleSets => Set<TaxRuleSet>();
    public DbSet<TaxSlab> TaxSlabs => Set<TaxSlab>();
    public DbSet<TaxRelief> TaxReliefs => Set<TaxRelief>();
    public DbSet<EmployeePayItem> EmployeePayItems => Set<EmployeePayItem>();
    public DbSet<PayrollSettings> PayrollSettings => Set<PayrollSettings>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<GeneralLedgerAccountMapping> GeneralLedgerAccountMappings => Set<GeneralLedgerAccountMapping>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateEmployeePayItems();
        ValidateEmployeeRecurringPayItems();
        ValidateRecurringPayItemRules();
        ValidateRecurringPayItemAssignments();
        EnsureAuditEventEntriesAreAppendOnly();
        EnsurePayRunStatusHistoryIsAppendOnly();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PayrollDbContext).Assembly);
    }

    private void ValidateEmployeePayItems()
    {
        var pendingPayItems = ChangeTracker.Entries<EmployeePayItem>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        foreach (var payItem in pendingPayItems)
        {
            var hasAmount = payItem.Amount.HasValue;
            var hasPercentage = payItem.Percentage.HasValue;

            if (hasAmount == hasPercentage)
            {
                throw new InvalidOperationException("Employee pay items must have either amount or percentage set, but not both.");
            }

            if ((hasAmount && payItem.Amount <= 0) || (hasPercentage && payItem.Percentage <= 0))
            {
                throw new InvalidOperationException("Employee pay item values must be greater than zero.");
            }

            var start = payItem.EffectiveFrom;
            var end = payItem.EffectiveTo ?? DateOnly.MaxValue;

            var overlaps = EmployeePayItems
                .AsNoTracking()
                .Any(existing => existing.Id != payItem.Id
                    && existing.EmployeeId == payItem.EmployeeId
                    && existing.PayItemCode == payItem.PayItemCode
                    && existing.PayItemType == payItem.PayItemType
                    && existing.IsActive
                    && existing.EffectiveFrom <= end
                    && (existing.EffectiveTo == null || existing.EffectiveTo >= start));

            if (overlaps)
            {
                throw new InvalidOperationException("Overlapping employee pay item date ranges are not allowed.");
            }
        }
    }

    private void ValidateEmployeeRecurringPayItems()
    {
        var pendingPayItems = ChangeTracker.Entries<EmployeeRecurringPayItem>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        foreach (var payItem in pendingPayItems)
        {
            var hasAmount = payItem.Amount.HasValue;
            var hasPercentage = payItem.Percentage.HasValue;

            if (hasAmount == hasPercentage)
            {
                throw new InvalidOperationException(
                    "Employee recurring pay items must have either amount or percentage set, but not both.");
            }

            if ((hasAmount && payItem.Amount <= 0) || (hasPercentage && payItem.Percentage <= 0))
            {
                throw new InvalidOperationException("Employee recurring pay item values must be greater than zero.");
            }

            if (payItem.PayItemKind == PayItemKind.Allowance)
            {
                if (payItem.AllowanceTypeId == null || payItem.DeductionTypeId != null)
                {
                    throw new InvalidOperationException(
                        "Allowance recurring pay items must reference an allowance type and not a deduction type.");
                }
            }
            else if (payItem.PayItemKind == PayItemKind.Deduction)
            {
                if (payItem.DeductionTypeId == null || payItem.AllowanceTypeId != null)
                {
                    throw new InvalidOperationException(
                        "Deduction recurring pay items must reference a deduction type and not an allowance type.");
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid pay item kind for recurring pay item.");
            }

            if (payItem.EffectiveTo.HasValue && payItem.EffectiveTo.Value < payItem.EffectiveFrom)
            {
                throw new InvalidOperationException(
                    "Recurring pay item effective to date cannot be earlier than effective from date.");
            }
        }
    }

    private void ValidateRecurringPayItemRules()
    {
        var pendingRules = ChangeTracker.Entries<RecurringPayItemRule>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        foreach (var rule in pendingRules)
        {
            if (rule.Amount <= 0)
            {
                throw new InvalidOperationException("Recurring pay item rule amount must be greater than zero.");
            }

            if (rule.EndDate.HasValue && rule.EndDate.Value < rule.StartDate)
            {
                throw new InvalidOperationException("Recurring pay item rule end date cannot be earlier than start date.");
            }

            if (rule.RuleType == RecurringRuleType.Allowance)
            {
                if (rule.AllowanceTypeId == null || rule.DeductionTypeId != null)
                {
                    throw new InvalidOperationException(
                        "Allowance recurring rule must reference an allowance type and not a deduction type.");
                }
            }
            else if (rule.RuleType == RecurringRuleType.Deduction)
            {
                if (rule.DeductionTypeId == null || rule.AllowanceTypeId != null)
                {
                    throw new InvalidOperationException(
                        "Deduction recurring rule must reference a deduction type and not an allowance type.");
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid recurring rule type.");
            }
        }
    }

    private void ValidateRecurringPayItemAssignments()
    {
        var pendingAssignments = ChangeTracker.Entries<RecurringPayItemAssignment>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        foreach (var assignment in pendingAssignments)
        {
            if (assignment.EndDate.HasValue && assignment.EndDate.Value < assignment.StartDate)
            {
                throw new InvalidOperationException(
                    "Recurring pay item assignment end date cannot be earlier than start date.");
            }

            var start = assignment.StartDate;
            var end = assignment.EndDate ?? DateOnly.MaxValue;

            var overlaps = RecurringPayItemAssignments
                .AsNoTracking()
                .Any(existing => existing.Id != assignment.Id
                    && existing.EmployeeId == assignment.EmployeeId
                    && existing.RuleId == assignment.RuleId
                    && existing.IsActive
                    && existing.StartDate <= end
                    && (existing.EndDate == null || existing.EndDate >= start));

            if (overlaps)
            {
                throw new InvalidOperationException("Overlapping recurring pay item assignments are not allowed.");
            }
        }
    }

    private void EnsureAuditEventEntriesAreAppendOnly()
    {
        var tamperedAuditLogs = ChangeTracker.Entries<AuditEvent>()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        if (tamperedAuditLogs.Any())
        {
            throw new InvalidOperationException("Audit events are append-only and cannot be modified or removed.");
        }
    }

    private void EnsurePayRunStatusHistoryIsAppendOnly()
    {
        var tamperedHistory = ChangeTracker.Entries<PayRunStatusHistory>()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        if (tamperedHistory.Any())
        {
            throw new InvalidOperationException("Pay run status history entries are append-only and cannot be modified or removed.");
        }
    }
}
