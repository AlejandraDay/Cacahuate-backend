using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Cacahuate.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.DataAccess.Repositories;

public class AppointmentRepository(AppDbContext db) : IAppointmentRepository
{
    public Task<Appointment?> GetByIdAsync(Guid id) =>
        db.Appointments
            .Include(a => a.Therapist).ThenInclude(t => t.User)
            .Include(a => a.Patient)
            .Include(a => a.Parent).ThenInclude(p => p.User)
            .Include(a => a.Rating)
            .Include(a => a.FormSubmission)
            .FirstOrDefaultAsync(a => a.Id == id);

    public Task<List<Appointment>> GetByPatientAsync(Guid patientId) =>
        db.Appointments
            .Include(a => a.Therapist).ThenInclude(t => t.User)
            .Include(a => a.Patient)
            .Include(a => a.Rating)
            .Include(a => a.FormSubmission)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime)
            .ToListAsync();

    public Task<List<Appointment>> GetByTherapistAsync(Guid therapistId) =>
        db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Parent).ThenInclude(p => p.User)
            .Include(a => a.Rating)
            .Include(a => a.FormSubmission)
            .Where(a => a.TherapistId == therapistId)
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime)
            .ToListAsync();

    public Task<List<Appointment>> GetByParentAsync(Guid parentId) =>
        db.Appointments
            .Include(a => a.Therapist).ThenInclude(t => t.User)
            .Include(a => a.Patient)
            .Include(a => a.Rating)
            .Include(a => a.FormSubmission)
            .Where(a => a.ParentId == parentId)
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime)
            .ToListAsync();

    public Task<List<Appointment>> GetAllAsync() =>
        db.Appointments
            .Include(a => a.Therapist).ThenInclude(t => t.User)
            .Include(a => a.Patient)
            .Include(a => a.Parent).ThenInclude(p => p.User)
            .Include(a => a.Rating)
            .Include(a => a.FormSubmission)
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime)
            .ToListAsync();

    public Task<bool> SlotIsBookedAsync(Guid availabilityId, TimeOnly startTime) =>
        db.Appointments.AnyAsync(a =>
            a.AvailabilityId == availabilityId &&
            a.StartTime == startTime &&
            a.Status != AppointmentStatus.Cancelled);

    public async Task AddAsync(Appointment appointment) => await db.Appointments.AddAsync(appointment);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
