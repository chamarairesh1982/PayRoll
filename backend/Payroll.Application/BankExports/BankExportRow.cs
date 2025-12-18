namespace Payroll.Application.BankExports;

public record BankExportRow(
    string EmployeeCode,
    string EmployeeName,
    string AccountNumber,
    string BranchCode,
    decimal Amount);
