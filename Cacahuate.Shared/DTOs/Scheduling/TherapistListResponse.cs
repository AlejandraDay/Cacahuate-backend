namespace Cacahuate.Shared.DTOs.Scheduling;

public class TherapistListResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public int SessionDurationMinutes { get; set; }
}
