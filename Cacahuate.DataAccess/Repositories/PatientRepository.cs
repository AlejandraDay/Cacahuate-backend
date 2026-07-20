using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.DataAccess.Repositories;

public class PatientRepository(AppDbContext db) : IPatientRepository
{
    public Task<Patient?> GetByIdAsync(Guid id) =>
        db.Patients.Include(p => p.Parent).ThenInclude(par => par.User)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public Task<List<Patient>> GetByParentAsync(Guid parentId) =>
        db.Patients.Include(p => p.Parent).ThenInclude(par => par.User)
            .Where(p => p.ParentId == parentId && p.IsActive)
            .ToListAsync();

    public Task<List<Patient>> GetAllAsync() =>
        db.Patients.Include(p => p.Parent).ThenInclude(par => par.User)
            .Where(p => p.IsActive)
            .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
            .ToListAsync();

    public async Task<(List<Patient> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, string? search)
    {
        var query = db.Patients.Include(p => p.Parent).ThenInclude(par => par.User)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.FirstName.ToLower().Contains(term) || p.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(Patient patient) => await db.Patients.AddAsync(patient);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
