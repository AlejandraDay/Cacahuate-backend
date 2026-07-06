namespace Cacahuate.DataAccess.Entities;

public class FormTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<FormField> Fields { get; set; } = [];
    public List<FormAssignment> Assignments { get; set; } = [];
}