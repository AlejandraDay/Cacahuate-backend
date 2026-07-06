namespace Cacahuate.Shared.DTOs.Scheduling;

public class PatientResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public string ParentName { get; set; } = string.Empty;
}
