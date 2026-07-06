using Cacahuate.DataAccess.Entities;

namespace Cacahuate.DataAccess.Repositories;

public interface IAvailabilityRepository
{
    Task<TherapistAvailability?> GetByIdAsync(Guid id);
    Task<List<TherapistAvailability>> GetByTherapistAndDateAsync(Guid therapistId, DateOnly date);
    Task<List<TherapistAvailability>> GetByTherapistAsync(Guid therapistId);
    Task AddAsync(TherapistAvailability availability);
    Task SaveChangesAsync();
}
