using Cacahuate.DataAccess.Entities;

namespace Cacahuate.DataAccess.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<List<Appointment>> GetByPatientAsync(Guid patientId);
    Task<List<Appointment>> GetByTherapistAsync(Guid therapistId);
    Task<List<Appointment>> GetByParentAsync(Guid parentId);
    Task<List<Appointment>> GetAllAsync();
    Task<bool> SlotIsBookedAsync(Guid availabilityId, TimeOnly startTime);
    Task AddAsync(Appointment appointment);
    Task SaveChangesAsync();
}
