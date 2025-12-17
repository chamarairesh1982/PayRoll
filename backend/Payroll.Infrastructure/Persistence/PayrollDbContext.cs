using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Leave;
using Payroll.Domain.Loans;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Infrastructure.Persistence;

public class PayrollDbContext : DbContext, IPayrollDbContext
{
    public PayrollDbContext(DbContextOptions<PayrollDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayRun> PayRuns => Set<PayRun>();
    public DbSet<PaySlip> PaySlips => Set<PaySlip>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<OvertimeRecord> OvertimeRecords => Set<OvertimeRecord>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanRepayment> LoanRepayments => Set<LoanRepayment>();
    public DbSet<AllowanceType> AllowanceTypes => Set<AllowanceType>();
    public DbSet<DeductionType> DeductionTypes => Set<DeductionType>();
    public DbSet<EpfEtfRuleSet> EpfEtfRuleSets => Set<EpfEtfRuleSet>();
    public DbSet<TaxRuleSet> TaxRuleSets => Set<TaxRuleSet>();
    public DbSet<TaxSlab> TaxSlabs => Set<TaxSlab>();
    public DbSet<EmployeePayItem> EmployeePayItems => Set<EmployeePayItem>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateEmployeePayItems();

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
}
