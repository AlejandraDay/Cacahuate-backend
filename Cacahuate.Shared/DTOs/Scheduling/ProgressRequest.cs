using Cacahuate.Shared.Enums;

namespace Cacahuate.Shared.DTOs.Scheduling;

public class ProgressRequest
{
    public string? ProgressNotes { get; set; }
    public string? Objectives { get; set; }
    public string? Behavior { get; set; }
    public string? Communication { get; set; }
    public string? SocialInteraction { get; set; }
    public string? SensoryResponse { get; set; }
    public string? Achievements { get; set; }
    public string? AreasToReinforce { get; set; }
    public string? Recommendations { get; set; }
    public ParticipationLevel? ParticipationLevel { get; set; }
}