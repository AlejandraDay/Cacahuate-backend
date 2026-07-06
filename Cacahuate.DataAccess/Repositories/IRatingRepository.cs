using Cacahuate.DataAccess.Entities;

namespace Cacahuate.DataAccess.Repositories;

public interface IRatingRepository
{
    Task<TherapistRating?> GetByAppointmentAsync(Guid appointmentId);
    Task<List<TherapistRating>> GetByTherapistAsync(Guid therapistId);
    Task AddAsync(TherapistRating rating);
    Task SaveChangesAsync();
}