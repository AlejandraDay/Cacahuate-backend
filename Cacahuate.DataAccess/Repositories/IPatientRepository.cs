using Cacahuate.DataAccess.Entities;

namespace Cacahuate.DataAccess.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id);
    Task<List<Patient>> GetByParentAsync(Guid parentId);
    Task<List<Patient>> GetAllAsync();
    Task<(List<Patient> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, string? search);
    Task AddAsync(Patient patient);
    Task SaveChangesAsync();
}
