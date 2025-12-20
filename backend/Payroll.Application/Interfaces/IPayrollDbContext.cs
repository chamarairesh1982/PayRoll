using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Attendance;
using Payroll.Domain.Employees;
using Payroll.Domain.Loans;
using Payroll.Domain.Leave;
using Payroll.Domain.GeneralLedger;
using Payroll.Domain.Overtime;
using Payroll.Domain.Payroll;
using Payroll.Domain.PayrollConfig;
using Payroll.Domain.Auditing;
using Payroll.Domain.Organizations;

namespace Payroll.Application.Interfaces;

public interface IPayrollDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<EmployeeTaxProfile> EmployeeTaxProfiles { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<LeaveTypeDefinition> LeaveTypes { get; }
    DbSet<LeaveEncashmentRequest> LeaveEncashmentRequests { get; }
    DbSet<OTEntry> OTEntries { get; }
    DbSet<OTRule> OTRules { get; }
    DbSet<Loan> Loans { get; }
    DbSet<PayRun> PayRuns { get; }
    DbSet<PaySlip> PaySlips { get; }
    DbSet<PayslipDocument> PayslipDocuments { get; }
    DbSet<PayRunStatusHistory> PayRunStatusHistories { get; }
    DbSet<BankExportTemplate> BankExportTemplates { get; }
    DbSet<PayRunBankExport> PayRunBankExports { get; }
    DbSet<PayRunBankExportError> PayRunBankExportErrors { get; }
    DbSet<StatutoryReport> StatutoryReports { get; }
    DbSet<RecurringRule> RecurringRules { get; }
    DbSet<RecurringPayItemRule> RecurringPayItemRules { get; }
    DbSet<RecurringPayItemAssignment> RecurringPayItemAssignments { get; }
    DbSet<PayRunRecurringLine> PayRunRecurringLines { get; }
    DbSet<Company> Companies { get; }
    DbSet<Branch> Branches { get; }
    DbSet<CostCenter> CostCenters { get; }
    DbSet<AllowanceType> AllowanceTypes { get; }
    DbSet<DeductionType> DeductionTypes { get; }
    DbSet<EmployeeRecurringPayItem> EmployeeRecurringPayItems { get; }
    DbSet<EpfEtfRuleSet> EpfEtfRuleSets { get; }
    DbSet<TaxRuleSet> TaxRuleSets { get; }
    DbSet<TaxSlab> TaxSlabs { get; }
    DbSet<TaxRelief> TaxReliefs { get; }
    DbSet<EmployeePayItem> EmployeePayItems { get; }
    DbSet<PayrollSettings> PayrollSettings { get; }
    DbSet<AuditEvent> AuditEvents { get; }
    DbSet<GeneralLedgerAccountMapping> GeneralLedgerAccountMappings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
