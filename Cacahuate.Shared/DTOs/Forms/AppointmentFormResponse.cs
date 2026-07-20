namespace Cacahuate.Shared.DTOs.Forms;

public class AppointmentFormResponse
{
    public Guid AssignmentId { get; set; }
    public string FormTemplateName { get; set; } = string.Empty;
    public string? FormTemplateDescription { get; set; }
    public string? Notes { get; set; }
    public List<FormFieldResponse> Fields { get; set; } = [];
    public bool IsSubmitted { get; set; }
    public FormSubmissionResponse? Submission { get; set; }
}