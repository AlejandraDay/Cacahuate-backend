namespace Cacahuate.DataAccess.Entities;

public class FormAnswer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FormSubmissionId { get; set; }
    public Guid FormFieldId { get; set; }
    public string Value { get; set; } = string.Empty;

    public FormSubmission FormSubmission { get; set; } = null!;
    public FormField FormField { get; set; } = null!;
}