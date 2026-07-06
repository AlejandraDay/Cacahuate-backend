using Cacahuate.Shared.DTOs.Scheduling;

namespace Cacahuate.Services.Interfaces;

public interface ISchedulingService
{
    Task<AvailabilityResponse> AddAvailabilityAsync(Guid therapistUserId, AvailabilityRequest request);
    Task<List<AvailabilityResponse>> GetTherapistAvailabilityAsync(Guid therapistId);
    Task<List<AvailabilityResponse>> GetMyAvailabilityAsync(Guid therapistUserId);
    Task<AvailabilityResponse> GetAvailabilityWithSlotsAsync(Guid availabilityId);
    Task<AppointmentResponse> BookAppointmentAsync(Guid parentUserId, BookAppointmentRequest request);
    Task<List<AppointmentResponse>> GetAppointmentsByParentAsync(Guid parentUserId);
    Task<List<AppointmentResponse>> GetAppointmentsByTherapistAsync(Guid therapistUserId);
    Task<AppointmentResponse> CancelAppointmentAsync(Guid appointmentId, Guid requestingUserId);
    Task<AppointmentResponse> ConfirmAppointmentAsync(Guid appointmentId);
    Task<AppointmentResponse> RejectAppointmentAsync(Guid appointmentId);
    Task<AppointmentResponse> CompleteAppointmentAsync(Guid appointmentId, Guid requestingUserId);
    Task<AppointmentResponse> MarkEnRouteAsync(Guid appointmentId, Guid therapistUserId);
    Task<AppointmentResponse> MarkInProgressAsync(Guid appointmentId, Guid therapistUserId);
    Task<List<AppointmentResponse>> GetPendingAppointmentsAsync();
    Task<List<AppointmentResponse>> GetAllAppointmentsAsync();
    Task<PatientResponse> AddPatientAsync(Guid parentUserId, PatientRequest request);
    Task<List<PatientResponse>> GetPatientsByParentAsync(Guid parentUserId);
    Task<List<PatientResponse>> GetAllPatientsAsync();
    Task<AppointmentResponse> AddProgressNotesAsync(Guid appointmentId, Guid therapistUserId, ProgressRequest request);
    Task<RatingResponse> RateTherapistAsync(Guid appointmentId, Guid parentUserId, RatingRequest request);
    Task<List<RatingResponse>> GetTherapistRatingsAsync(Guid therapistId);
    Task<List<RatingResponse>> GetMyRatingsAsync(Guid therapistUserId);
}
