using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var loans = await _loanService.GetLoansAsync(page, pageSize);
        return Ok(loans);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var loan = await _loanService.GetLoanAsync(id);
        if (loan is null)
        {
            return NotFound();
        }

        return Ok(loan);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LoanDto dto)
    {
        var created = await _loanService.CreateLoanAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/repay")]
    public async Task<IActionResult> Repay(Guid id, [FromBody] RepayLoanRequest request)
    {
        await _loanService.RecordRepaymentAsync(id, request.Amount);
        return NoContent();
    }
}
