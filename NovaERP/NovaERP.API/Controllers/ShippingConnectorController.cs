using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/shipping-connectors")]
[Authorize]
public class ShippingConnectorController : ApiControllerBase
{
    private readonly IShippingConnectorService _service;

    public ShippingConnectorController(IShippingConnectorService service) => _service = service;

    [Authorize(Policy = "carrier-connector.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "carrier-connector.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "carrier-connector.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveShippingConnectorDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "carrier-connector.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveShippingConnectorDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "carrier-connector.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    /// <summary>Postman-style "Send" for a brand-new, not-yet-saved connector draft.</summary>
    [Authorize(Policy = "carrier-connector.edit")]
    [HttpPost("test")]
    public async Task<IActionResult> Test([FromBody] TestShippingConnectorDto dto) =>
        Ok(await _service.TestAsync(OrgId, null, dto));

    /// <summary>Postman-style "Send" while editing an existing connector — inherits any secret
    /// fields left blank in the form from the saved row.</summary>
    [Authorize(Policy = "carrier-connector.edit")]
    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestExisting(int id, [FromBody] TestShippingConnectorDto dto) =>
        Ok(await _service.TestAsync(OrgId, id, dto));
}
