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
    private readonly HostContext _context;

    public Repository(
        HostContext context,
        IDatabaseModelMapper<TDomain, TDatabase> mapper)
    {
        _dbSet = context.Set<TDatabase>();
        _context = context;
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
        var mapped = _mapper.ToDatabase(entity);
        var tracked = FindTracked(mapped);
        if (tracked == null) { _dbSet.Update(mapped); return; }
        var entry = _context.Entry(tracked);
        entry.CurrentValues.SetValues(mapped);
        // A newly added entity must remain Added until the first SaveChanges.
        if (entry.State != EntityState.Added) entry.State = EntityState.Modified;
    }

    public void Remove(TDomain entity)
    {
        var mapped = _mapper.ToDatabase(entity);
        _dbSet.Remove(FindTracked(mapped) ?? mapped);
    }

    private TDatabase? FindTracked(TDatabase mapped)
    {
        var key = _context.Model.FindEntityType(typeof(TDatabase))!.FindPrimaryKey()!;
        var incoming = _context.Entry(mapped);
        return _dbSet.Local.FirstOrDefault(local => key.Properties.All(property =>
            Equals(_context.Entry(local).Property(property.Name).CurrentValue,
                incoming.Property(property.Name).CurrentValue)));
    }
}
