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
    Task<(List<FormAssignment> Items, int TotalCount)> GetAllAssignmentsPagedAsync(int page, int pageSize, Guid? patientId);
    Task<List<FormAssignment>> GetAssignmentsByPatientIdAsync(Guid patientId);
    Task<List<FormAssignment>> GetAssignmentsByPatientIdsAsync(IEnumerable<Guid> patientIds);
    Task<bool> AssignmentExistsAsync(Guid patientId, Guid formTemplateId);

    // Submissions
    Task AddSubmissionAsync(FormSubmission submission);
    Task<FormSubmission?> GetSubmissionByAppointmentAndAssignmentAsync(Guid appointmentId, Guid assignmentId);
    Task<FormSubmission?> GetSubmissionByIdAsync(Guid submissionId);

    Task SaveChangesAsync();
}