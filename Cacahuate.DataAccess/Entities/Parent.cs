namespace Cacahuate.DataAccess.Entities;

public class Parent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Patient> Patients { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<TherapistRating> Ratings { get; set; } = [];
}
