namespace Cacahuate.Shared.DTOs.Forms;

public class SubmitFormRequest
{
    public List<FormAnswerDto> Answers { get; set; } = [];
}

public class FormAnswerDto
{
    public Guid FieldId { get; set; }
    public string Value { get; set; } = string.Empty;
}