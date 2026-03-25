using CustomerSupportPlatform.Domain.Entities;
using CustomerSupportPlatform.Application.Interfaces;
using Database;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupportPlatform.Infrastructure.Repositories;

public abstract class RepositoryBase<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context;

    protected RepositoryBase(AppDbContext context) => Context = context;

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Set<T>().ToListAsync(cancellationToken);

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<T>().AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
