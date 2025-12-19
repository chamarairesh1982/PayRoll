namespace Payroll.Application.StatutoryReports;

public record EpfEtfReportRow(
    string? EmployeeCode,
    string? EmployeeName,
    string? NicNumber,
    string? EpfNumber,
    decimal ContributableBase,
    decimal EmployeeEpf,
    decimal EmployerEpf,
    decimal EmployerEtf);

public record EpfEtfReportSummary(
    int EmployeeCount,
    decimal ContributableBaseTotal,
    decimal EmployeeEpfTotal,
    decimal EmployerEpfTotal,
    decimal EmployerEtfTotal);

public record EpfEtfReportData(
    string PayRunCode,
    string PayRunName,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    DateTime PayDate,
    EpfEtfReportSummary Summary,
    IReadOnlyList<EpfEtfReportRow> Rows);
