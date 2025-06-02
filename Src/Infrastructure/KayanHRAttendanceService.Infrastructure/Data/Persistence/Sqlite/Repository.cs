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

    public async Task AddAsync(T[] entity)
    {
        await dbSet.AddRangeAsync(entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = dbSet;
        if (filter is not null)
        {
            query = query.Where(filter);
        }
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