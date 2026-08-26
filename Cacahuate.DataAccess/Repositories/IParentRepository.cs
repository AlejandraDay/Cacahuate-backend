using Cacahuate.DataAccess.Entities;

namespace Cacahuate.DataAccess.Repositories;

public interface IParentRepository
{
    Task<Parent?> GetByUserIdAsync(Guid userId);
    Task<Parent?> GetByIdAsync(Guid id);
    Task<(List<Parent> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, string? search);
    Task AddAsync(Parent parent);
    Task SaveChangesAsync();
}
