using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.DataAccess.Repositories;

public class FormRepository(AppDbContext db) : IFormRepository
{
    public Task AddTemplateAsync(FormTemplate template)
    {
        db.FormTemplates.Add(template);
        return Task.CompletedTask;
    }

    public Task<FormTemplate?> GetTemplateByIdAsync(Guid id) =>
        db.FormTemplates
            .Include(t => t.Fields.OrderBy(f => f.Order))
            .FirstOrDefaultAsync(t => t.Id == id);

    public Task<List<FormTemplate>> GetAllTemplatesAsync() =>
        db.FormTemplates
            .Include(t => t.Fields.OrderBy(f => f.Order))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public Task RemoveTemplateAsync(FormTemplate template)
    {
        db.FormTemplates.Remove(template);
        return Task.CompletedTask;
    }

    public Task AddAssignmentAsync(FormAssignment assignment)
    {
        db.FormAssignments.Add(assignment);
        return Task.CompletedTask;
    }

    public Task<FormAssignment?> GetAssignmentByIdAsync(Guid id) =>
        db.FormAssignments
            .Include(a => a.FormTemplate).ThenInclude(t => t.Fields.OrderBy(f => f.Order))
            .Include(a => a.Patient)
            .Include(a => a.Submissions).ThenInclude(s => s.Answers)
            .Include(a => a.Submissions).ThenInclude(s => s.Therapist).ThenInclude(t => t.User)
            .FirstOrDefaultAsync(a => a.Id == id);

    public Task<List<FormAssignment>> GetAllAssignmentsAsync() =>
        db.FormAssignments
            .Include(a => a.FormTemplate).ThenInclude(t => t.Fields.OrderBy(f => f.Order))
            .Include(a => a.Patient)
            .Include(a => a.Submissions).ThenInclude(s => s.Answers)
            .Include(a => a.Submissions).ThenInclude(s => s.Therapist).ThenInclude(t => t.User)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

    public Task<FormAssignment?> GetActiveAssignmentByPatientIdAsync(Guid patientId) =>
        db.FormAssignments
            .Include(a => a.FormTemplate).ThenInclude(t => t.Fields.OrderBy(f => f.Order))
            .Include(a => a.Patient)
            .OrderByDescending(a => a.AssignedAt)
            .FirstOrDefaultAsync(a => a.PatientId == patientId);

    public Task AddSubmissionAsync(FormSubmission submission)
    {
        db.FormSubmissions.Add(submission);
        return Task.CompletedTask;
    }

    public Task<FormSubmission?> GetSubmissionByAppointmentIdAsync(Guid appointmentId) =>
        db.FormSubmissions
            .Include(s => s.Answers)
            .Include(s => s.Therapist).ThenInclude(t => t.User)
            .Include(s => s.FormAssignment).ThenInclude(a => a.FormTemplate)
                .ThenInclude(t => t.Fields.OrderBy(f => f.Order))
            .FirstOrDefaultAsync(s => s.AppointmentId == appointmentId);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}