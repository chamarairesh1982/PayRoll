using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Payroll.Commands.CreatePayRun;
using Payroll.Application.Payroll.Queries.GetPayRunById;
using Payroll.Application.Payroll.Queries.GetPayRuns;
using Payroll.Application.Payroll.Queries.GetPayslipPdf;
using Payroll.Contracts.Payroll;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayRunsController : ControllerBase
{
    private readonly ISender _sender;

    public PayRunsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PayRunDto>>> GetPayRuns()
    {
        return Ok(await _sender.Send(new GetPayRunsQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PayRunDetailDto>> GetPayRun(Guid id)
    {
        var payRun = await _sender.Send(new GetPayRunByIdQuery(id));

        if (payRun == null)
            return NotFound();

        return payRun;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreatePayRun(CreatePayRunCommand command)
    {
        var id = await _sender.Send(command);
        return CreatedAtAction(nameof(GetPayRun), new { id }, id);
    }

    [HttpGet("{id}/employees/{employeeId}/payslip")]
    public async Task<IActionResult> GetPayslip(Guid id, Guid employeeId)
    {
        var pdfBytes = await _sender.Send(new GetPayslipPdfQuery(id, employeeId));

        if (pdfBytes == null)
            return NotFound();

        return File(pdfBytes, "application/pdf", $"Payslip_{employeeId}.pdf");
    }
}
