namespace Cacahuate.Shared.DTOs.Scheduling;

public class AvailabilityRequest
{
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SessionDurationMinutes { get; set; } = 60;
}
