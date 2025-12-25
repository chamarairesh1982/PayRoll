using MediatR;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Common;
using Payroll.Application.Common.Interfaces;
using Payroll.Application.Payroll.Queries.GetPayRunById;

namespace Payroll.Application.Payroll.Queries.GetPayslipPdf;

public record GetPayslipPdfQuery(Guid PayRunId, Guid EmployeeId) : IRequest<byte[]?>;

public class GetPayslipPdfQueryHandler : IRequestHandler<GetPayslipPdfQuery, byte[]?>
{
    private readonly ISender _sender;
    private readonly IPdfService _pdfService;

    public GetPayslipPdfQueryHandler(ISender sender, IPdfService pdfService)
    {
        _sender = sender;
        _pdfService = pdfService;
    }

    public async Task<byte[]?> Handle(GetPayslipPdfQuery request, CancellationToken cancellationToken)
    {
        // Reuse the detail query to get data
        var payRun = await _sender.Send(new GetPayRunByIdQuery(request.PayRunId), cancellationToken);

        if (payRun == null)
            return null;

        var lineItem = payRun.LineItems.FirstOrDefault(x => x.EmployeeId == request.EmployeeId);

        if (lineItem == null)
            return null;

        return _pdfService.GeneratePayslip(payRun, lineItem);
    }
}
