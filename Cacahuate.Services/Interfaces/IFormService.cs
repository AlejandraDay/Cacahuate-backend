using Cacahuate.Shared.DTOs.Forms;

namespace Cacahuate.Services.Interfaces;

public interface IFormService
{
    // Admin
    Task<FormTemplateResponse> CreateTemplateAsync(CreateFormTemplateRequest request);
    Task<List<FormTemplateResponse>> GetAllTemplatesAsync();
    Task DeleteTemplateAsync(Guid templateId);
    Task<FormAssignmentResponse> AssignTemplateAsync(AssignFormRequest request, Guid adminUserId);
    Task<List<FormAssignmentResponse>> GetAllAssignmentsAsync();

    // Therapist
    Task<AppointmentFormResponse> GetFormForAppointmentAsync(Guid appointmentId);
    Task<AppointmentFormResponse> SubmitFormAsync(Guid appointmentId, Guid assignmentId, SubmitFormRequest request, Guid therapistUserId);
}