using System.Security.Claims;
using Cacahuate.Services.Interfaces;
using Cacahuate.Shared.DTOs.Forms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cacahuate.API.Controllers;

[ApiController]
[Route("api/forms")]
[Authorize]
public class FormsController(IFormService formService) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    // ── Templates (Admin) ──────────────────────────────────────────────────────

    [HttpPost("templates")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FormTemplateResponse>> CreateTemplate([FromBody] CreateFormTemplateRequest request)
    {
        var result = await formService.CreateTemplateAsync(request);
        return CreatedAtAction(nameof(GetTemplates), new { }, result);
    }

    [HttpGet("templates")]
    [Authorize(Roles = "Admin,Therapist")]
    public async Task<ActionResult<List<FormTemplateResponse>>> GetTemplates()
    {
        var result = await formService.GetAllTemplatesAsync();
        return Ok(result);
    }

    [HttpDelete("templates/{templateId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTemplate(Guid templateId)
    {
        await formService.DeleteTemplateAsync(templateId);
        return NoContent();
    }

    // ── Assignments (Admin) ────────────────────────────────────────────────────

    [HttpPost("assignments")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FormAssignmentResponse>> AssignTemplate([FromBody] AssignFormRequest request)
    {
        var result = await formService.AssignTemplateAsync(request, CurrentUserId);
        return Ok(result);
    }

    [HttpGet("assignments")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<FormAssignmentResponse>>> GetAllAssignments()
    {
        var result = await formService.GetAllAssignmentsAsync();
        return Ok(result);
    }

    // ── Per-appointment form (Therapist) ───────────────────────────────────────

    [HttpGet("for-appointment/{appointmentId}")]
    [Authorize(Roles = "Therapist")]
    public async Task<ActionResult<AppointmentFormResponse>> GetFormForAppointment(Guid appointmentId)
    {
        var result = await formService.GetFormForAppointmentAsync(appointmentId);
        return Ok(result);
    }

    [HttpPost("for-appointment/{appointmentId}/submit/{assignmentId}")]
    [Authorize(Roles = "Therapist")]
    public async Task<ActionResult<AppointmentFormResponse>> SubmitForm(
        Guid appointmentId, Guid assignmentId, [FromBody] SubmitFormRequest request)
    {
        var result = await formService.SubmitFormAsync(appointmentId, assignmentId, request, CurrentUserId);
        return Ok(result);
    }
}