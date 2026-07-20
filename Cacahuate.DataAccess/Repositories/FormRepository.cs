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

    public async Task<(List<FormAssignment> Items, int TotalCount)> GetAllAssignmentsPagedAsync(int page, int pageSize, Guid? patientId)
    {
        var query = db.FormAssignments
            .Include(a => a.FormTemplate).ThenInclude(t => t.Fields.OrderBy(f => f.Order))
            .Include(a => a.Patient)
            .Include(a => a.Submissions).ThenInclude(s => s.Answers)
            .Include(a => a.Submissions).ThenInclude(s => s.Therapist).ThenInclude(t => t.User)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(a => a.PatientId == patientId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AssignedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<List<FormAssignment>> GetAssignmentsByPatientIdAsync(Guid patientId) =>
        db.FormAssignments
            .Include(a => a.FormTemplate).ThenInclude(t => t.Fields.OrderBy(f => f.Order))
            .Include(a => a.Patient)
            .Where(a => a.PatientId == patientId)
            .OrderBy(a => a.AssignedAt)
            .ToListAsync();

    public Task<List<FormAssignment>> GetAssignmentsByPatientIdsAsync(IEnumerable<Guid> patientIds) =>
        db.FormAssignments
            .Include(a => a.FormTemplate).ThenInclude(t => t.Fields.OrderBy(f => f.Order))
            .Include(a => a.Patient)
            .Include(a => a.Submissions).ThenInclude(s => s.Answers)
            .Include(a => a.Submissions).ThenInclude(s => s.Therapist).ThenInclude(t => t.User)
            .Where(a => patientIds.Contains(a.PatientId))
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

    public Task<bool> AssignmentExistsAsync(Guid patientId, Guid formTemplateId) =>
        db.FormAssignments.AnyAsync(a => a.PatientId == patientId && a.FormTemplateId == formTemplateId);

    public Task AddSubmissionAsync(FormSubmission submission)
    {
        db.FormSubmissions.Add(submission);
        return Task.CompletedTask;
    }

    public Task<FormSubmission?> GetSubmissionByAppointmentAndAssignmentAsync(Guid appointmentId, Guid assignmentId) =>
        db.FormSubmissions
            .Include(s => s.Answers)
            .Include(s => s.Therapist).ThenInclude(t => t.User)
            .Include(s => s.FormAssignment).ThenInclude(a => a.FormTemplate)
                .ThenInclude(t => t.Fields.OrderBy(f => f.Order))
            .FirstOrDefaultAsync(s => s.AppointmentId == appointmentId && s.FormAssignmentId == assignmentId);

    public Task<FormSubmission?> GetSubmissionByIdAsync(Guid submissionId) =>
        db.FormSubmissions
            .Include(s => s.Answers)
            .Include(s => s.Therapist).ThenInclude(t => t.User)
            .Include(s => s.FormAssignment).ThenInclude(a => a.FormTemplate)
                .ThenInclude(t => t.Fields.OrderBy(f => f.Order))
            .Include(s => s.Appointment).ThenInclude(a => a.Therapist).ThenInclude(t => t.User)
            .Include(s => s.Appointment).ThenInclude(a => a.Parent).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}