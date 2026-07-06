namespace Cacahuate.Shared.DTOs.Scheduling;

public class RatingResponse
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid TherapistId { get; set; }
    public string TherapistName { get; set; } = string.Empty;
    public Guid ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}