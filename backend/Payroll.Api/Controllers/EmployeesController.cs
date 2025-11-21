using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var employees = await _employeeService.GetEmployeesAsync(page, pageSize);
        return Ok(employees);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        var dto = new EmployeeDto
        {
            EmployeeNumber = request.EmployeeNumber,
            FullName = request.FullName,
            EmploymentStatus = request.EmploymentStatus,
            EmployeeType = request.EmployeeType
        };

        var created = await _employeeService.CreateEmployeeAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
    {
        var dto = new EmployeeDto
        {
            EmployeeNumber = request.EmployeeNumber,
            FullName = request.FullName,
            EmploymentStatus = request.EmploymentStatus,
            EmployeeType = request.EmployeeType
        };

        await _employeeService.UpdateEmployeeAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _employeeService.DeleteEmployeeAsync(id);
        return NoContent();
    }
}

public record CreateEmployeeRequest(string EmployeeNumber, string FullName, Payroll.Domain.Employees.EmploymentStatus EmploymentStatus, Payroll.Domain.Employees.EmployeeType EmployeeType);
public record UpdateEmployeeRequest(string EmployeeNumber, string FullName, Payroll.Domain.Employees.EmploymentStatus EmploymentStatus, Payroll.Domain.Employees.EmployeeType EmployeeType);
