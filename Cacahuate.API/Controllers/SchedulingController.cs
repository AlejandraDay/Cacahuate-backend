using System.Security.Claims;
using Cacahuate.Services.Interfaces;
using Cacahuate.Shared.DTOs.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cacahuate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchedulingController(ISchedulingService schedulingService) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    // Therapist: set availability
    [HttpPost("availability")]
    [Authorize(Roles = "Therapist")]
    public async Task<ActionResult<AvailabilityResponse>> AddAvailability([FromBody] AvailabilityRequest request)
    {
        var result = await schedulingService.AddAvailabilityAsync(CurrentUserId, request);
        return Ok(result);
    }

    // Therapist: view own availability (resolves therapist by JWT user id)
    [HttpGet("availability")]
    [Authorize(Roles = "Therapist")]
    public async Task<ActionResult<List<AvailabilityResponse>>> GetMyAvailability()
    {
        var result = await schedulingService.GetMyAvailabilityAsync(CurrentUserId);
        return Ok(result);
    }

    // Anyone: view therapist availability with slots
    [HttpGet("therapists/{therapistId}/availability")]
    public async Task<ActionResult<List<AvailabilityResponse>>> GetTherapistAvailability(Guid therapistId)
    {
        var result = await schedulingService.GetTherapistAvailabilityAsync(therapistId);
        return Ok(result);
    }

    // Anyone: view specific availability block with open slots
    [HttpGet("availability/{availabilityId}")]
    public async Task<ActionResult<AvailabilityResponse>> GetAvailabilitySlots(Guid availabilityId)
    {
        var result = await schedulingService.GetAvailabilityWithSlotsAsync(availabilityId);
        return Ok(result);
    }

    // Parent: book an appointment
    [HttpPost("appointments")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<AppointmentResponse>> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        var result = await schedulingService.BookAppointmentAsync(CurrentUserId, request);
        return Ok(result);
    }

    // Parent: view their appointments
    [HttpGet("appointments/my")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetMyAppointments()
    {
        var result = await schedulingService.GetAppointmentsByParentAsync(CurrentUserId);
        return Ok(result);
    }

    // Therapist: view their appointments
    [HttpGet("appointments/therapist")]
    [Authorize(Roles = "Therapist")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetTherapistAppointments()
    {
        var result = await schedulingService.GetAppointmentsByTherapistAsync(CurrentUserId);
        return Ok(result);
    }

    // Parent or Therapist: cancel appointment
    [HttpPatch("appointments/{appointmentId}/cancel")]
    [Authorize(Roles = "Parent,Therapist")]
    public async Task<ActionResult<AppointmentResponse>> CancelAppointment(Guid appointmentId)
    {
        var result = await schedulingService.CancelAppointmentAsync(appointmentId, CurrentUserId);
        return Ok(result);
    }

    // Therapist: mark appointment as en route
    [HttpPatch("appointments/{appointmentId}/enroute")]
    [Authorize(Roles = "Therapist")]
    public async Task<ActionResult<AppointmentResponse>> MarkEnRoute(Guid appointmentId)
    {
        var result = await schedulingService.MarkEnRouteAsync(appointmentId, CurrentUserId);
        return Ok(result);
    }

    // Therapist: mark appointment as in progress (session started)
    [HttpPatch("appointments/{appointmentId}/inprogress")]
    [Authorize(Roles = "Therapist")]
    public async Task<ActionResult<AppointmentResponse>> MarkInProgress(Guid appointmentId)
    {
        var result = await schedulingService.MarkInProgressAsync(appointmentId, CurrentUserId);
        return Ok(result);
    }

    // Therapist: mark appointment as completed
    [HttpPatch("appointments/{appointmentId}/complete")]
    [Authorize(Roles = "Therapist")]
    public async Task<ActionResult<AppointmentResponse>> CompleteAppointment(Guid appointmentId)
    {
        var result = await schedulingService.CompleteAppointmentAsync(appointmentId, CurrentUserId);
        return Ok(result);
    }

    // Parent: add a patient
    [HttpPost("patients")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<PatientResponse>> AddPatient([FromBody] PatientRequest request)
    {
        var result = await schedulingService.AddPatientAsync(CurrentUserId, request);
        return Ok(result);
    }

    // Parent: list their patients
    [HttpGet("patients/my")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<List<PatientResponse>>> GetMyPatients()
    {
        var result = await schedulingService.GetPatientsByParentAsync(CurrentUserId);
        return Ok(result);
    }

    // Admin: list all patients
    [HttpGet("patients/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<PatientResponse>>> GetAllPatients()
    {
        var result = await schedulingService.GetAllPatientsAsync();
        return Ok(result);
    }

    // Admin: approve appointment (pending → confirmed)
    [HttpPatch("appointments/{appointmentId}/confirm")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AppointmentResponse>> ConfirmAppointment(Guid appointmentId)
    {
        var result = await schedulingService.ConfirmAppointmentAsync(appointmentId);
        return Ok(result);
    }

    // Admin: reject appointment (pending → cancelled)
    [HttpPatch("appointments/{appointmentId}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AppointmentResponse>> RejectAppointment(Guid appointmentId)
    {
        var result = await schedulingService.RejectAppointmentAsync(appointmentId);
        return Ok(result);
    }

    // Admin: get all pending appointments
    [HttpGet("appointments/admin/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetPendingAppointments()
    {
        var result = await schedulingService.GetPendingAppointmentsAsync();
        return Ok(result);
    }

    // Admin: get all appointments
    [HttpGet("appointments/admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetAllAppointments()
    {
        var result = await schedulingService.GetAllAppointmentsAsync();
        return Ok(result);
    }

    // Therapist: add progress notes to a completed appointment
    [HttpPatch("appointments/{appointmentId}/progress")]
    [Authorize(Roles = "Therapist")]
    public async Task<ActionResult<AppointmentResponse>> AddProgressNotes(Guid appointmentId, [FromBody] ProgressRequest request)
    {
        var result = await schedulingService.AddProgressNotesAsync(appointmentId, CurrentUserId, request);
        return Ok(result);
    }

    // Parent: rate the therapist after a completed appointment
    [HttpPost("appointments/{appointmentId}/rate")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<RatingResponse>> RateTherapist(Guid appointmentId, [FromBody] RatingRequest request)
    {
        var result = await schedulingService.RateTherapistAsync(appointmentId, CurrentUserId, request);
        return Ok(result);
    }

    // Anyone: view a therapist's ratings
    [HttpGet("therapists/{therapistId}/ratings")]
    public async Task<ActionResult<List<RatingResponse>>> GetTherapistRatings(Guid therapistId)
    {
        var result = await schedulingService.GetTherapistRatingsAsync(therapistId);
        return Ok(result);
    }

    // Therapist: view own ratings
    [HttpGet("ratings/my")]
    [Authorize(Roles = "Therapist")]
    public async Task<ActionResult<List<RatingResponse>>> GetMyRatings()
    {
        var result = await schedulingService.GetMyRatingsAsync(CurrentUserId);
        return Ok(result);
    }
}
