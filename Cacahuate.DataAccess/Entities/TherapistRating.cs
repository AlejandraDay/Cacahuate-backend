namespace Cacahuate.DataAccess.Entities;

public class TherapistRating
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppointmentId { get; set; }
    public Guid TherapistId { get; set; }
    public Guid ParentId { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Appointment Appointment { get; set; } = null!;
    public Therapist Therapist { get; set; } = null!;
    public Parent Parent { get; set; } = null!;
}