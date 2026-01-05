using Microsoft.AspNetCore.Mvc;
using Payroll.Application.DTOs.RulePackages;
using Payroll.Application.Interfaces;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/rule-packages")]
public class RulePackagesController : ControllerBase
{
    private readonly IRulePackageService _service;

    public RulePackagesController(IRulePackageService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] RulePackageQuery query, CancellationToken cancellationToken = default)
    {
        var packages = await _service.GetPackagesAsync(query, cancellationToken);
        return Ok(packages);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRulePackageRequest request, CancellationToken cancellationToken = default)
    {
        var created = await _service.CreatePackageAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetVersions), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}/versions")]
    public async Task<IActionResult> GetVersions(Guid id, CancellationToken cancellationToken = default)
    {
        var versions = await _service.GetVersionsAsync(id, cancellationToken);
        return Ok(versions);
    }

    [HttpPost("{id:guid}/versions")]
    public async Task<IActionResult> CreateVersion(Guid id, [FromBody] CreateRulePackageVersionRequest request, CancellationToken cancellationToken = default)
    {
        var created = await _service.CreateVersionAsync(id, request, cancellationToken);
        return Ok(created);
    }

    [HttpPost("{id:guid}/versions/{versionId:guid}/activate")]
    public async Task<IActionResult> ActivateVersion(Guid id, Guid versionId, CancellationToken cancellationToken = default)
    {
        await _service.ActivateVersionAsync(id, versionId, cancellationToken);
        return NoContent();
    }
}
