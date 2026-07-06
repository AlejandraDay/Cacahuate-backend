using System.Text.Json;
using Cacahuate.DataAccess.Entities;
using Cacahuate.DataAccess.Repositories;
using Cacahuate.Services.Interfaces;
using Cacahuate.Shared.DTOs.Forms;
using Cacahuate.Shared.Enums;

namespace Cacahuate.Services.Implementations;

public class FormService(
    IFormRepository formRepository,
    IAppointmentRepository appointmentRepository,
    ITherapistRepository therapistRepository) : IFormService
{
    public async Task<FormTemplateResponse> CreateTemplateAsync(CreateFormTemplateRequest request)
    {
        var template = new FormTemplate
        {
            Name = request.Name,
            Description = request.Description,
            Fields = request.Fields.Select((f, i) => new FormField
            {
                Label = f.Label,
                Type = f.Type,
                Options = f.Options != null ? JsonSerializer.Serialize(f.Options) : null,
                IsRequired = f.IsRequired,
                Order = f.Order > 0 ? f.Order : i
            }).ToList()
        };

        await formRepository.AddTemplateAsync(template);
        await formRepository.SaveChangesAsync();

        return MapTemplate(template);
    }

    public async Task<List<FormTemplateResponse>> GetAllTemplatesAsync()
    {
        var templates = await formRepository.GetAllTemplatesAsync();
        return templates.Select(MapTemplate).ToList();
    }

    public async Task DeleteTemplateAsync(Guid templateId)
    {
        var template = await formRepository.GetTemplateByIdAsync(templateId)
            ?? throw new KeyNotFoundException("Template not found.");
        await formRepository.RemoveTemplateAsync(template);
        await formRepository.SaveChangesAsync();
    }

    public async Task<FormAssignmentResponse> AssignTemplateAsync(AssignFormRequest request, Guid adminUserId)
    {
        var template = await formRepository.GetTemplateByIdAsync(request.FormTemplateId)
            ?? throw new KeyNotFoundException("Form template not found.");

        var assignment = new FormAssignment
        {
            FormTemplateId = template.Id,
            PatientId = request.PatientId,
            AssignedByAdminUserId = adminUserId,
            Notes = request.Notes
        };

        await formRepository.AddAssignmentAsync(assignment);
        await formRepository.SaveChangesAsync();

        var saved = await formRepository.GetAssignmentByIdAsync(assignment.Id)
            ?? throw new InvalidOperationException("Failed to retrieve assignment.");

        return MapAssignment(saved);
    }

    public async Task<List<FormAssignmentResponse>> GetAllAssignmentsAsync()
    {
        var assignments = await formRepository.GetAllAssignmentsAsync();
        return assignments.Select(MapAssignment).ToList();
    }

    public async Task<AppointmentFormResponse> GetFormForAppointmentAsync(Guid appointmentId)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        var assignment = await formRepository.GetActiveAssignmentByPatientIdAsync(appointment.PatientId);

        if (assignment == null)
            return new AppointmentFormResponse { HasForm = false };

        var submission = await formRepository.GetSubmissionByAppointmentIdAsync(appointmentId);

        return new AppointmentFormResponse
        {
            HasForm = true,
            AssignmentId = assignment.Id,
            FormTemplateName = assignment.FormTemplate.Name,
            FormTemplateDescription = assignment.FormTemplate.Description,
            Notes = assignment.Notes,
            Fields = assignment.FormTemplate.Fields.OrderBy(f => f.Order).Select(MapField).ToList(),
            IsSubmitted = submission != null,
            Submission = submission == null ? null : MapSubmission(submission, assignment.FormTemplate.Fields)
        };
    }

    public async Task<AppointmentFormResponse> SubmitFormAsync(
        Guid appointmentId, Guid assignmentId, SubmitFormRequest request, Guid therapistUserId)
    {
        var existing = await formRepository.GetSubmissionByAppointmentIdAsync(appointmentId);
        if (existing != null)
            throw new InvalidOperationException("This appointment already has a submitted form.");

        var assignment = await formRepository.GetAssignmentByIdAsync(assignmentId)
            ?? throw new KeyNotFoundException("Assignment not found.");

        var therapist = await therapistRepository.GetByUserIdAsync(therapistUserId)
            ?? throw new KeyNotFoundException("Therapist not found.");

        var submission = new FormSubmission
        {
            FormAssignmentId = assignmentId,
            AppointmentId = appointmentId,
            TherapistId = therapist.Id,
            Answers = request.Answers.Select(a => new FormAnswer
            {
                FormFieldId = a.FieldId,
                Value = a.Value
            }).ToList()
        };

        await formRepository.AddSubmissionAsync(submission);
        await formRepository.SaveChangesAsync();

        var saved = await formRepository.GetSubmissionByAppointmentIdAsync(appointmentId)!
            ?? throw new InvalidOperationException("Failed to retrieve submission.");

        return new AppointmentFormResponse
        {
            HasForm = true,
            AssignmentId = assignmentId,
            FormTemplateName = assignment.FormTemplate.Name,
            FormTemplateDescription = assignment.FormTemplate.Description,
            Notes = assignment.Notes,
            Fields = assignment.FormTemplate.Fields.OrderBy(f => f.Order).Select(MapField).ToList(),
            IsSubmitted = true,
            Submission = MapSubmission(saved, assignment.FormTemplate.Fields)
        };
    }

    // ── Mappers ────────────────────────────────────────────────────────────────

    private static FormTemplateResponse MapTemplate(FormTemplate t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        IsActive = t.IsActive,
        CreatedAt = t.CreatedAt,
        Fields = t.Fields.OrderBy(f => f.Order).Select(MapField).ToList()
    };

    private static FormFieldResponse MapField(FormField f) => new()
    {
        Id = f.Id,
        Label = f.Label,
        Type = f.Type,
        Options = f.Options != null ? JsonSerializer.Deserialize<List<string>>(f.Options) : null,
        IsRequired = f.IsRequired,
        Order = f.Order
    };

    private static FormAssignmentResponse MapAssignment(FormAssignment a) => new()
    {
        Id = a.Id,
        FormTemplateId = a.FormTemplateId,
        FormTemplateName = a.FormTemplate.Name,
        FormTemplateDescription = a.FormTemplate.Description,
        PatientId = a.PatientId,
        PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
        Notes = a.Notes,
        AssignedAt = a.AssignedAt,
        Fields = a.FormTemplate.Fields.OrderBy(f => f.Order).Select(MapField).ToList(),
        Submissions = a.Submissions.OrderByDescending(s => s.SubmittedAt)
            .Select(s => MapSubmission(s, a.FormTemplate.Fields))
            .ToList()
    };

    private static FormSubmissionResponse MapSubmission(FormSubmission s, IEnumerable<FormField> fields) => new()
    {
        Id = s.Id,
        AppointmentId = s.AppointmentId,
        TherapistName = $"{s.Therapist.User.FirstName} {s.Therapist.User.LastName}",
        SubmittedAt = s.SubmittedAt,
        Answers = s.Answers.Select(ans => new FormAnswerResponse
        {
            FieldId = ans.FormFieldId,
            FieldLabel = fields.FirstOrDefault(f => f.Id == ans.FormFieldId)?.Label ?? "",
            FieldType = fields.FirstOrDefault(f => f.Id == ans.FormFieldId)?.Type ?? FieldType.Text,
            Value = ans.Value
        }).ToList()
    };
}
