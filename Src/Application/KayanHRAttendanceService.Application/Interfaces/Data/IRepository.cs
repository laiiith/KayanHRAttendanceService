using System.Linq.Expressions;

namespace KayanHRAttendanceService.Application.Interfaces.Data;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, int? take = null);

    Task<T?> GetAsync(Expression<Func<T, bool>>? filter = null);

    Task AddAsync(T entity);

    Task AddAsync(IEnumerable<T> entities);
}