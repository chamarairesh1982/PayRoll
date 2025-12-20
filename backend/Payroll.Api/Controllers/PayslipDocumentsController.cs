using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/payslip-documents")]
public class PayslipDocumentsController : ControllerBase
{
    private readonly IPayslipDocumentService _payslipDocumentService;

    public PayslipDocumentsController(IPayslipDocumentService payslipDocumentService)
    {
        _payslipDocumentService = payslipDocumentService;
    }

    [HttpGet("{documentId:guid}/download")]
    public async Task<IActionResult> Download(Guid documentId, CancellationToken cancellationToken = default)
    {
        var file = await _payslipDocumentService.DownloadAsync(documentId, cancellationToken);
        return file is null ? NotFound() : Ok(file);
    }
}
