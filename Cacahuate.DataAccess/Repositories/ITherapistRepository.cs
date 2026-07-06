using Cacahuate.DataAccess.Entities;

namespace Cacahuate.DataAccess.Repositories;

public interface ITherapistRepository
{
    Task<Therapist?> GetByUserIdAsync(Guid userId);
    Task<Therapist?> GetByIdAsync(Guid id);
    Task<List<Therapist>> GetAllActiveAsync();
    Task AddAsync(Therapist therapist);
    Task SaveChangesAsync();
}
