using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Employees;
using Payroll.Domain.Leave;
using Payroll.Domain.Overtime;
using Payroll.Domain.PayrollConfig;

namespace Payroll.Application.Interfaces;

public interface IPayrollDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<OvertimeRecord> OvertimeRecords { get; }
    DbSet<AllowanceType> AllowanceTypes { get; }
    DbSet<DeductionType> DeductionTypes { get; }
    DbSet<EpfEtfRuleSet> EpfEtfRuleSets { get; }
    DbSet<TaxRuleSet> TaxRuleSets { get; }
    DbSet<TaxSlab> TaxSlabs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
