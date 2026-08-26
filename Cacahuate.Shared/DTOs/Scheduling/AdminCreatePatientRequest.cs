namespace Cacahuate.Shared.DTOs.Scheduling;

public class AdminCreatePatientRequest
{
    public Guid ParentUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? Notes { get; set; }
}
