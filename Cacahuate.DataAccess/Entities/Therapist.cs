namespace Cacahuate.DataAccess.Entities;

public class Therapist
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string? Bio { get; set; }
    public int SessionDurationMinutes { get; set; } = 60;
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public ICollection<TherapistAvailability> Availabilities { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<TherapistRating> Ratings { get; set; } = [];
}
