namespace Cacahuate.Shared.DTOs.Common;

public class TherapistPatientSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? ParentName { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public DateOnly? LastAppointmentDate { get; set; }
}
