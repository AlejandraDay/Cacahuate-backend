namespace Cacahuate.DataAccess.Entities;

public class FormAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FormTemplateId { get; set; }
    public Guid PatientId { get; set; }
    public Guid AssignedByAdminUserId { get; set; }
    public string? Notes { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public FormTemplate FormTemplate { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public List<FormSubmission> Submissions { get; set; } = [];
}