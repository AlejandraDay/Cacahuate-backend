namespace Cacahuate.DataAccess.Entities;

public class FormSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FormAssignmentId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid TherapistId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public FormAssignment FormAssignment { get; set; } = null!;
    public Appointment Appointment { get; set; } = null!;
    public Therapist Therapist { get; set; } = null!;
    public List<FormAnswer> Answers { get; set; } = [];
}