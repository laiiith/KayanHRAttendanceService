using System.Linq.Expressions;

namespace KayanHRAttendanceService.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
    Task<T?> GetAsync(Expression<Func<T, bool>>? filter = null);
    Task AddAsync(T entity);
    Task AddAsync(T[] entity);
}
