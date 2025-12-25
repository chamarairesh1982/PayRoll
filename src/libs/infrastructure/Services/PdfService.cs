using Payroll.Application.Common.Interfaces;
using Payroll.Contracts.Payroll;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Payroll.Infrastructure.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GeneratePayslip(PayRunDetailDto payRun, PayRunLineItemDto lineItem)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                page.Header()
                    .Text($"PAYSLIP - {payRun.Name}")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        // Employee Details
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                            });

                            table.Cell().Text("Employee:");
                            table.Cell().Text($"{lineItem.EmployeeName} ({lineItem.EmployeeCode})").Bold();
                            
                            table.Cell().Text("Pay Period:");
                            table.Cell().Text($"{payRun.PeriodStart:yyyy-MM-dd} to {payRun.PeriodEnd:yyyy-MM-dd}");

                            table.Cell().Text("Pay Date:");
                            table.Cell().Text($"{payRun.PaymentDate:yyyy-MM-dd}");
                        });

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Earnings Table
                        column.Item().Text("Earnings").SemiBold().FontSize(14);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Description").SemiBold();
                                header.Cell().AlignRight().Text("Amount").SemiBold();
                                header.Cell().ColumnSpan(2).PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);
                            });

                            table.Cell().Text("Base Salary");
                            table.Cell().AlignRight().Text($"{lineItem.BaseSalary:N2}");

                            if (lineItem.Allowances > 0)
                            {
                                table.Cell().Text("Allowances");
                                table.Cell().AlignRight().Text($"{lineItem.Allowances:N2}");
                            }

                            if (lineItem.Overtime > 0)
                            {
                                table.Cell().Text("Overtime");
                                table.Cell().AlignRight().Text($"{lineItem.Overtime:N2}");
                            }

                            table.Cell().PaddingTop(5).Text("Total Gross").Bold();
                            table.Cell().PaddingTop(5).AlignRight().Text($"{lineItem.GrossAmount:N2}").Bold();
                        });

                        column.Item().PaddingVertical(10);

                        // Deductions Table
                        column.Item().Text("Deductions").SemiBold().FontSize(14);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Description").SemiBold();
                                header.Cell().AlignRight().Text("Amount").SemiBold();
                                header.Cell().ColumnSpan(2).PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);
                            });

                            if (lineItem.EpfEmployee > 0)
                            {
                                table.Cell().Text("EPF (Employee)");
                                table.Cell().AlignRight().Text($"{lineItem.EpfEmployee:N2}");
                            }

                            if (lineItem.Tax > 0)
                            {
                                table.Cell().Text("PAYE Tax");
                                table.Cell().AlignRight().Text($"{lineItem.Tax:N2}");
                            }

                            if (lineItem.OtherDeductions > 0)
                            {
                                table.Cell().Text("Other Deductions");
                                table.Cell().AlignRight().Text($"{lineItem.OtherDeductions:N2}");
                            }

                            table.Cell().PaddingTop(5).Text("Total Deductions").Bold();
                            table.Cell().PaddingTop(5).AlignRight().Text($"{lineItem.TotalDeductions:N2}").Bold();
                        });

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Net Pay
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("NET PAY").FontSize(16).Bold();
                            row.RelativeItem().AlignRight().Text($"LKR {lineItem.NetAmount:N2}").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated by Payroll vNext");
                        x.Span($" | Page {x.CurrentPageNumber} of {x.TotalPages}");
                    });
            });
        });

        return document.GeneratePdf();
    }
}
