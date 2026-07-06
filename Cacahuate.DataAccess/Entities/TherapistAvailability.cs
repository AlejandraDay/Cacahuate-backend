namespace Cacahuate.DataAccess.Entities;

public class TherapistAvailability
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TherapistId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SessionDurationMinutes { get; set; } = 60;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Therapist Therapist { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = [];
}
