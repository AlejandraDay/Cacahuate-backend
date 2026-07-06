namespace Cacahuate.Shared.DTOs.Forms;

public class AssignFormRequest
{
    public Guid FormTemplateId { get; set; }
    public Guid PatientId { get; set; }
    public string? Notes { get; set; }
}