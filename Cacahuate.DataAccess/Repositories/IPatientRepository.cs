using Cacahuate.DataAccess.Entities;

namespace Cacahuate.DataAccess.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id);
    Task<List<Patient>> GetByParentAsync(Guid parentId);
    Task<List<Patient>> GetAllAsync();
    Task AddAsync(Patient patient);
    Task SaveChangesAsync();
}
