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
            .Include(a => a.FormSubmissions)
            .FirstOrDefaultAsync(a => a.Id == id);

    public Task<List<Appointment>> GetByPatientAsync(Guid patientId) =>
        db.Appointments
            .Include(a => a.Therapist).ThenInclude(t => t.User)
            .Include(a => a.Patient)
            .Include(a => a.Rating)
            .Include(a => a.FormSubmissions)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime)
            .ToListAsync();

    public Task<List<Appointment>> GetByTherapistAsync(Guid therapistId) =>
        db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Parent).ThenInclude(p => p.User)
            .Include(a => a.Rating)
            .Include(a => a.FormSubmissions)
            .Where(a => a.TherapistId == therapistId)
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime)
            .ToListAsync();

    public Task<List<Appointment>> GetByParentAsync(Guid parentId) =>
        db.Appointments
            .Include(a => a.Therapist).ThenInclude(t => t.User)
            .Include(a => a.Patient)
            .Include(a => a.Rating)
            .Include(a => a.FormSubmissions)
            .Where(a => a.ParentId == parentId)
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime)
            .ToListAsync();

    public Task<List<Appointment>> GetAllAsync() =>
        db.Appointments
            .Include(a => a.Therapist).ThenInclude(t => t.User)
            .Include(a => a.Patient)
            .Include(a => a.Parent).ThenInclude(p => p.User)
            .Include(a => a.Rating)
            .Include(a => a.FormSubmissions)
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime)
            .ToListAsync();

    public Task<List<Guid>> GetPatientIdsByTherapistIdAsync(Guid therapistId) =>
        db.Appointments
            .Where(a => a.TherapistId == therapistId)
            .Select(a => a.PatientId)
            .Distinct()
            .ToListAsync();

    public async Task<(List<Appointment> Items, int TotalCount)> GetAllPagedAsync(
        int page, int pageSize, Guid? patientId, Guid? therapistId,
        AppointmentStatus? status, DateOnly? dateFrom, DateOnly? dateTo)
    {
        var query = db.Appointments
            .Include(a => a.Therapist).ThenInclude(t => t.User)
            .Include(a => a.Patient)
            .Include(a => a.Parent).ThenInclude(p => p.User)
            .Include(a => a.Rating)
            .Include(a => a.FormSubmissions)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(a => a.PatientId == patientId.Value);
        if (therapistId.HasValue) query = query.Where(a => a.TherapistId == therapistId.Value);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (dateFrom.HasValue) query = query.Where(a => a.Date >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(a => a.Date <= dateTo.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.Date).ThenBy(a => a.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<(Guid Id, string Name)>> GetDistinctTherapistsForPatientAsync(Guid patientId)
    {
        var results = await db.Appointments
            .Where(a => a.PatientId == patientId)
            .Include(a => a.Therapist).ThenInclude(t => t.User)
            .Select(a => new { a.TherapistId, a.Therapist.User.FirstName, a.Therapist.User.LastName })
            .Distinct()
            .ToListAsync();

        return results
            .GroupBy(r => r.TherapistId)
            .Select(g => (g.Key, $"{g.First().FirstName} {g.First().LastName}"))
            .ToList();
    }

    public async Task<List<(Guid Id, string Name)>> GetDistinctPatientsForTherapistAsync(Guid therapistId)
    {
        var results = await db.Appointments
            .Where(a => a.TherapistId == therapistId)
            .Include(a => a.Patient)
            .Select(a => new { a.PatientId, a.Patient.FirstName, a.Patient.LastName })
            .Distinct()
            .ToListAsync();

        return results
            .GroupBy(r => r.PatientId)
            .Select(g => (g.Key, $"{g.First().FirstName} {g.First().LastName}"))
            .ToList();
    }

    public Task<bool> SlotIsBookedAsync(Guid availabilityId, TimeOnly startTime) =>
        db.Appointments.AnyAsync(a =>
            a.AvailabilityId == availabilityId &&
            a.StartTime == startTime &&
            a.Status != AppointmentStatus.Cancelled);

    public async Task AddAsync(Appointment appointment) => await db.Appointments.AddAsync(appointment);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
