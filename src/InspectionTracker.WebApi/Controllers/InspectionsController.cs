using System.Security.Claims;
using InspectionTracker.Application.Dtos;
using InspectionTracker.Application.Services;
using InspectionTracker.Domain.Entities;
using InspectionTracker.WebApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InspectionTracker.WebApi.Controllers;

[ApiController]
[Route("api/inspections")]
public class InspectionsController(InspectionService inspectionService) : ControllerBase
{
    // PUBLIC — the "non-authorized endpoint" requirement
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<InspectionResponse>>> GetAll(CancellationToken ct)
    {
        var records = await inspectionService.GetAllAsync(ct);
        return Ok(records.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<InspectionResponse>> GetById(Guid id, CancellationToken ct)
        => Ok(ToResponse(await inspectionService.GetByIdAsync(id, ct)));

    // PROTECTED — require a valid JWT
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<InspectionResponse>> Create(InspectionRequest request, CancellationToken ct)
    {
        var record = await inspectionService.CreateAsync(ToDto(request), CurrentUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, ToResponse(record));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<InspectionResponse>> Update(Guid id, InspectionRequest request, CancellationToken ct)
        => Ok(ToResponse(await inspectionService.UpdateAsync(id, ToDto(request), CurrentUserId(), ct)));

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await inspectionService.DeleteAsync(id, CurrentUserId(), ct);
        return NoContent();
    }

    private Guid CurrentUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Token has no subject claim."));

    private static CreateInspectionDto ToDto(InspectionRequest r)
        => new(r.AssetName, r.Status, r.Notes, r.InspectionDate);

    private static InspectionResponse ToResponse(InspectionRecord i)
        => new(i.Id, i.AssetName, i.Status, i.Notes, i.InspectionDate,
               i.CreatedByUserId, i.CreatedBy?.DisplayName ?? "Unknown");
}
