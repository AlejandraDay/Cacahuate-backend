using Cacahuate.Shared.Enums;

namespace Cacahuate.DataAccess.Entities;

public class FormField
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FormTemplateId { get; set; }
    public string Label { get; set; } = string.Empty;
    public FieldType Type { get; set; } = FieldType.Text;
    public string? Options { get; set; } // JSON array for Select type
    public bool IsRequired { get; set; } = true;
    public int Order { get; set; }

    public FormTemplate FormTemplate { get; set; } = null!;
    public List<FormAnswer> Answers { get; set; } = [];
}