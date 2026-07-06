using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.DataAccess.Repositories;

public class AvailabilityRepository(AppDbContext db) : IAvailabilityRepository
{
    public Task<TherapistAvailability?> GetByIdAsync(Guid id) =>
        db.TherapistAvailabilities
            .Include(a => a.Therapist).ThenInclude(t => t.User)
            .Include(a => a.Appointments)
            .FirstOrDefaultAsync(a => a.Id == id);

    public Task<List<TherapistAvailability>> GetByTherapistAndDateAsync(Guid therapistId, DateOnly date) =>
        db.TherapistAvailabilities
            .Include(a => a.Appointments)
            .Where(a => a.TherapistId == therapistId && a.Date == date)
            .ToListAsync();

    public Task<List<TherapistAvailability>> GetByTherapistAsync(Guid therapistId) =>
        db.TherapistAvailabilities
            .Include(a => a.Appointments)
            .Where(a => a.TherapistId == therapistId && a.Date >= DateOnly.FromDateTime(DateTime.UtcNow))
            .OrderBy(a => a.Date).ThenBy(a => a.StartTime)
            .ToListAsync();

    public async Task AddAsync(TherapistAvailability availability) =>
        await db.TherapistAvailabilities.AddAsync(availability);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
