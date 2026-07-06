using Cacahuate.Shared.Enums;

namespace Cacahuate.Shared.DTOs.Forms;

public class CreateFormTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateFormFieldDto> Fields { get; set; } = [];
}

public class CreateFormFieldDto
{
    public string Label { get; set; } = string.Empty;
    public FieldType Type { get; set; } = FieldType.Text;
    public List<string>? Options { get; set; }
    public bool IsRequired { get; set; } = true;
    public int Order { get; set; }
}