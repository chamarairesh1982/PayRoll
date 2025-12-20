using Microsoft.AspNetCore.Mvc;
using Payroll.Application.PayrollConfig;
using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/tax/slabsets")]
public class TaxSlabSetsController : ControllerBase
{
    private readonly ITaxRuleSetService _service;

    public TaxSlabSetsController(ITaxRuleSetService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? yearOfAssessment = null)
    {
        var result = await _service.GetAsync(page, pageSize, yearOfAssessment);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaxRuleSetRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRuleSetRequest request)
    {
        await _service.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpGet("{id:guid}/slabs")]
    public async Task<IActionResult> GetSlabs(Guid id)
    {
        var slabs = await _service.GetSlabsAsync(id);
        return Ok(slabs);
    }

    [HttpPost("{id:guid}/slabs")]
    public async Task<IActionResult> AddSlab(Guid id, [FromBody] CreateTaxSlabRequest request)
    {
        var slab = await _service.AddSlabAsync(id, request);
        return Ok(slab);
    }

    [HttpPut("/api/tax/slabs/{slabId:guid}")]
    public async Task<IActionResult> UpdateSlab(Guid slabId, [FromBody] UpdateTaxSlabRequest request)
    {
        await _service.UpdateSlabAsync(slabId, request);
        return NoContent();
    }

    [HttpDelete("/api/tax/slabs/{slabId:guid}")]
    public async Task<IActionResult> DeleteSlab(Guid slabId)
    {
        await _service.DeleteSlabAsync(slabId);
        return NoContent();
    }

    [HttpGet("{id:guid}/reliefs")]
    public async Task<IActionResult> GetReliefs(Guid id)
    {
        var reliefs = await _service.GetReliefsAsync(id);
        return Ok(reliefs);
    }

    [HttpPost("{id:guid}/reliefs")]
    public async Task<IActionResult> AddRelief(Guid id, [FromBody] CreateTaxReliefRequest request)
    {
        var relief = await _service.AddReliefAsync(id, request);
        return Ok(relief);
    }

    [HttpPut("/api/tax/reliefs/{reliefId:guid}")]
    public async Task<IActionResult> UpdateRelief(Guid reliefId, [FromBody] UpdateTaxReliefRequest request)
    {
        await _service.UpdateReliefAsync(reliefId, request);
        return NoContent();
    }

    [HttpDelete("/api/tax/reliefs/{reliefId:guid}")]
    public async Task<IActionResult> DeleteRelief(Guid reliefId)
    {
        await _service.DeleteReliefAsync(reliefId);
        return NoContent();
    }
}
