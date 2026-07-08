using System.Security.Claims;
using Cacahuate.DataAccess.Repositories;
using Cacahuate.Services.Interfaces;
using Cacahuate.Shared.DTOs.Forms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cacahuate.API.Controllers;

[ApiController]
[Route("api/forms")]
[Authorize]
public class FormsController(IFormService formService, IFormRepository formRepository, IAppointmentRepository appointmentRepository) : ControllerBase
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
    [Authorize(Roles = "Admin,Therapist,Parent")]
    public async Task<ActionResult<AppointmentFormResponse>> GetFormForAppointment(Guid appointmentId)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (User.IsInRole("Parent") && appointment.Parent.UserId != CurrentUserId)
            throw new UnauthorizedAccessException("No autorizado para ver este informe.");

        if (User.IsInRole("Therapist") && appointment.Therapist.UserId != CurrentUserId)
            throw new UnauthorizedAccessException("No autorizado para ver este informe.");

        var result = await formService.GetFormForAppointmentAsync(appointmentId);
        return Ok(result);
    }

    [HttpGet("submissions/{submissionId}")]
    [Authorize(Roles = "Admin,Therapist,Parent")]
    public async Task<ActionResult<FormSubmissionResponse>> GetSubmission(Guid submissionId)
    {
        var submission = await formRepository.GetSubmissionByIdAsync(submissionId)
            ?? throw new KeyNotFoundException("Submission not found.");

        if (User.IsInRole("Parent") && submission.Appointment.Parent.UserId != CurrentUserId)
            throw new UnauthorizedAccessException("No autorizado para ver este informe.");

        if (User.IsInRole("Therapist") && submission.Appointment.Therapist.UserId != CurrentUserId)
            throw new UnauthorizedAccessException("No autorizado para ver este informe.");

        return Ok(new FormSubmissionResponse
        {
            Id = submission.Id,
            AppointmentId = submission.AppointmentId,
            TherapistName = submission.Therapist.User != null ? $"{submission.Therapist.User.FirstName} {submission.Therapist.User.LastName}" : string.Empty,
            SubmittedAt = submission.SubmittedAt,
            Answers = submission.Answers.Select(ans => new FormAnswerResponse
            {
                FieldId = ans.FormFieldId,
                FieldLabel = submission.FormAssignment.FormTemplate.Fields.FirstOrDefault(f => f.Id == ans.FormFieldId)?.Label ?? string.Empty,
                FieldType = submission.FormAssignment.FormTemplate.Fields.FirstOrDefault(f => f.Id == ans.FormFieldId)?.Type ?? default,
                Value = ans.Value
            }).ToList()
        });
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