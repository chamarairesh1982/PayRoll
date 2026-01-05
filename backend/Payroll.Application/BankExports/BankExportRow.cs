namespace Payroll.Application.BankExports;

public record BankExportRow(
    int RowNo,
    string BeneficiaryName,
    string AccountNumber,
    decimal Amount,
    string Reference,
    string EmployeeCode);
