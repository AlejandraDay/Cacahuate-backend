namespace Cacahuate.Shared.DTOs.Scheduling;

public class BookAppointmentRequest
{
    public Guid TherapistId { get; set; }
    public Guid PatientId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public string? Notes { get; set; }
}
