using Cacahuate.DataAccess.Entities;

namespace Cacahuate.DataAccess.Repositories;

public interface IFormRepository
{
    // Templates
    Task AddTemplateAsync(FormTemplate template);
    Task<FormTemplate?> GetTemplateByIdAsync(Guid id);
    Task<List<FormTemplate>> GetAllTemplatesAsync();
    Task RemoveTemplateAsync(FormTemplate template);

    // Assignments
    Task AddAssignmentAsync(FormAssignment assignment);
    Task<FormAssignment?> GetAssignmentByIdAsync(Guid id);
    Task<List<FormAssignment>> GetAllAssignmentsAsync();
    Task<FormAssignment?> GetActiveAssignmentByPatientIdAsync(Guid patientId);

    // Submissions
    Task AddSubmissionAsync(FormSubmission submission);
    Task<FormSubmission?> GetSubmissionByAppointmentIdAsync(Guid appointmentId);
    Task<FormSubmission?> GetSubmissionByIdAsync(Guid submissionId);

    Task SaveChangesAsync();
}