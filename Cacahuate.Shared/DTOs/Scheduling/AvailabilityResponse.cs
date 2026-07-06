namespace Cacahuate.Shared.DTOs.Scheduling;

public class AvailabilityResponse
{
    public Guid Id { get; set; }
    public Guid TherapistId { get; set; }
    public string TherapistName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SessionDurationMinutes { get; set; }
    public List<TimeSlotResponse> AvailableSlots { get; set; } = [];
}
