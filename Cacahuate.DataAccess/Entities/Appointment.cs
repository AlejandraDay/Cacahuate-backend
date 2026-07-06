using Cacahuate.Shared.Enums;

namespace Cacahuate.DataAccess.Entities;

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TherapistId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ParentId { get; set; }
    public Guid AvailabilityId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? Notes { get; set; }
    public string? ProgressNotes { get; set; }
    public string? ProgressObjectives { get; set; }
    public string? ProgressBehavior { get; set; }
    public string? ProgressCommunication { get; set; }
    public string? ProgressSocialInteraction { get; set; }
    public string? ProgressSensoryResponse { get; set; }
    public string? ProgressAchievements { get; set; }
    public string? ProgressAreasToReinforce { get; set; }
    public string? ProgressRecommendations { get; set; }
    public ParticipationLevel? ProgressParticipationLevel { get; set; }
    public DateTime? ProgressUpdatedAt { get; set; }
    public DateTime? EnRouteAt { get; set; }
    public DateTime? InProgressAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Therapist Therapist { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Parent Parent { get; set; } = null!;
    public TherapistAvailability Availability { get; set; } = null!;
    public TherapistRating? Rating { get; set; }
}
