using Cacahuate.Shared.Enums;

namespace Cacahuate.Shared.DTOs.Forms;

public class FormTemplateResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FormFieldResponse> Fields { get; set; } = [];
}

public class FormFieldResponse
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public FieldType Type { get; set; }
    public List<string>? Options { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
}