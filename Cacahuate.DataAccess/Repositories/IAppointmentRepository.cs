using Cacahuate.DataAccess.Entities;
using Cacahuate.Shared.Enums;

namespace Cacahuate.DataAccess.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<List<Appointment>> GetByPatientAsync(Guid patientId);
    Task<List<Appointment>> GetByTherapistAsync(Guid therapistId);
    Task<List<Guid>> GetPatientIdsByTherapistIdAsync(Guid therapistId);
    Task<List<Appointment>> GetByParentAsync(Guid parentId);
    Task<List<Appointment>> GetAllAsync();
    Task<(List<Appointment> Items, int TotalCount)> GetAllPagedAsync(
        int page, int pageSize, Guid? patientId, Guid? therapistId,
        AppointmentStatus? status, DateOnly? dateFrom, DateOnly? dateTo);
    Task<List<(Guid Id, string Name)>> GetDistinctTherapistsForPatientAsync(Guid patientId);
    Task<List<(Guid Id, string Name)>> GetDistinctPatientsForTherapistAsync(Guid therapistId);
    Task<bool> SlotIsBookedAsync(Guid availabilityId, TimeOnly startTime);
    Task AddAsync(Appointment appointment);
    Task SaveChangesAsync();
}
