using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Infrastructure.Persistence;

namespace Payroll.Infrastructure.Repositories;

public class GenericRepository<TEntity> where TEntity : class
{
    private readonly PayrollDbContext _context;

    public GenericRepository(PayrollDbContext context)
    {
        _context = context;
    }

    public Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Set<TEntity>().FindAsync(new object?[] { id }, cancellationToken).AsTask();
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _context.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();
    }

    public void Remove(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }
}
