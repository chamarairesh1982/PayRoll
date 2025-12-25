using Payroll.Contracts.Payroll;

namespace Payroll.Application.Common.Interfaces;

public interface IPdfService
{
    byte[] GeneratePayslip(PayRunDetailDto payRun, PayRunLineItemDto lineItem);
}
