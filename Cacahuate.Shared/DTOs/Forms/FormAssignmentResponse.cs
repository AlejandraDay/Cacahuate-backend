using Cacahuate.Shared.Enums;

namespace Cacahuate.Shared.DTOs.Forms;

public class FormAssignmentResponse
{
    public Guid Id { get; set; }
    public Guid FormTemplateId { get; set; }
    public string FormTemplateName { get; set; } = string.Empty;
    public string? FormTemplateDescription { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime AssignedAt { get; set; }
    public List<FormFieldResponse> Fields { get; set; } = [];
    public List<FormSubmissionResponse> Submissions { get; set; } = [];
}

public class FormSubmissionResponse
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string TherapistName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public List<FormAnswerResponse> Answers { get; set; } = [];
}

public class FormAnswerResponse
{
    public Guid FieldId { get; set; }
    public string FieldLabel { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public string Value { get; set; } = string.Empty;
}