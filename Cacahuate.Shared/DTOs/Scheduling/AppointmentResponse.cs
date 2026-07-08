using Cacahuate.Shared.Enums;

namespace Cacahuate.Shared.DTOs.Scheduling;

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public Guid TherapistId { get; set; }
    public string TherapistName { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
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
    public bool IsRated { get; set; }
    public int? RatingStars { get; set; }
    public Guid? FormSubmissionId { get; set; }
    public DateTime CreatedAt { get; set; }
}
