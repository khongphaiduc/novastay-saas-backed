using System.Linq.Expressions;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace NovaStay.Infrastructure.Persistence.Repositories;

internal class Repository<TDomain, TDatabase> : IRepository<TDomain>
    where TDomain : class
    where TDatabase : class
{
    private readonly DbSet<TDatabase> _dbSet;
    private readonly IDatabaseModelMapper<TDomain, TDatabase> _mapper;

    public Repository(
        HostContext context,
        IDatabaseModelMapper<TDomain, TDatabase> mapper)
    {
        _dbSet = context.Set<TDatabase>();
        _mapper = mapper;
    }

    public IQueryable<TDomain> Query()
    {
        return _dbSet
            .AsNoTracking()
            .AsEnumerable()
            .Select(_mapper.ToDomain)
            .AsQueryable();
    }

    public async Task<TDomain?> GetByIdAsync(
        object id,
        CancellationToken cancellationToken = default)
    {
        if (id is not Guid entityId)
        {
            return null;
        }

        var databaseModel = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => EF.Property<Guid>(entity, "Id") == entityId,
                cancellationToken);

        return databaseModel is null ? null : _mapper.ToDomain(databaseModel);
    }

    
    public async Task<IReadOnlyList<TDomain>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var databaseModels = await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return databaseModels.Select(_mapper.ToDomain).ToList();
    }

    public Task<IReadOnlyList<TDomain>> FindAsync(
        Expression<Func<TDomain, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TDomain> entities = Query()
            .Where(predicate)
            .ToList();

        return Task.FromResult(entities);
    }

    public async Task AddAsync(
        TDomain entity,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(_mapper.ToDatabase(entity), cancellationToken);
    }

    public void Update(TDomain entity)
    {
        _dbSet.Update(_mapper.ToDatabase(entity));
    }

    public void Remove(TDomain entity)
    {
        _dbSet.Remove(_mapper.ToDatabase(entity));
    }
}
