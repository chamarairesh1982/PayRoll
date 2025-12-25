using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payroll.Application.Employees.Commands.CreateEmployee;
using Payroll.Application.Employees.Commands.DeleteEmployee;
using Payroll.Application.Employees.Commands.UpdateEmployee;
using Payroll.Application.Employees.Queries.GetEmployeeById;
using Payroll.Application.Employees.Queries.GetEmployees;
using Payroll.Contracts.Employees;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ISender _sender;

    public EmployeesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        return Ok(await _sender.Send(new GetEmployeesQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(Guid id)
    {
        var employee = await _sender.Send(new GetEmployeeByIdQuery(id));

        if (employee == null)
            return NotFound();

        return employee;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateEmployee(CreateEmployeeCommand command)
    {
        var id = await _sender.Send(command);
        return CreatedAtAction(nameof(GetEmployee), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(Guid id, UpdateEmployeeCommand command)
    {
        if (id != command.Id)
            return BadRequest("The ID in the URL does not match the ID in the body.");

        try 
        {
            await _sender.Send(command);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        try
        {
            await _sender.Send(new DeleteEmployeeCommand(id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }
}
