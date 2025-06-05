using KayanHRAttendanceService.Application.Interfaces.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KayanHRAttendanceService.Infrastructure.Data.Persistence.Sqlite;

public class Repository<T>(ApplicationDbContext.ApplicationDbContext dbContext) : IRepository<T> where T : class
{
    internal DbSet<T> dbSet = dbContext.Set<T>();

    public async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public async Task AddAsync(IEnumerable<T> entities)
    {
        await dbSet.AddRangeAsync(entities);
    }

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, int? take = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        IQueryable<T> query = dbSet;
        if (filter is not null)
        {
            query = query.Where(filter);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (take.HasValue)
            query = query.Take(take.Value);

        return await query.ToListAsync();
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = dbSet;
        if (filter is not null)
        {
            query = query.Where(filter);
        }
        return await query.FirstOrDefaultAsync();
    }
}