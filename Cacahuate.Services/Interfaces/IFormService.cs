using Cacahuate.Shared.DTOs.Common;
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
    Task<PagedResult<FormAssignmentResponse>> GetAllAssignmentsPagedAsync(int page, int pageSize, Guid? patientId);

    // Therapist
    Task<List<FormAssignmentResponse>> GetAssignmentsForTherapistAsync(Guid therapistUserId);
    Task<List<AppointmentFormResponse>> GetFormsForAppointmentAsync(Guid appointmentId);
    Task<AppointmentFormResponse> SubmitFormAsync(Guid appointmentId, Guid assignmentId, SubmitFormRequest request, Guid therapistUserId);

    // Therapist + Parents/Admin
    Task<FormSubmissionResponse> GetSubmissionByIdAsync(Guid submissionId);
}