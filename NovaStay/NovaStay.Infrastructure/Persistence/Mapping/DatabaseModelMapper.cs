using AutoMapper;

namespace NovaStay.Infrastructure.Persistence.Mapping;

internal sealed class DatabaseModelMapper<TDomain, TDatabase> : IDatabaseModelMapper<TDomain, TDatabase>
    where TDomain : class
    where TDatabase : class
{
    private readonly IMapper _mapper;

    public DatabaseModelMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    public TDomain ToDomain(TDatabase databaseModel)
    {
        return _mapper.Map<TDomain>(databaseModel);
    }

    public TDatabase ToDatabase(TDomain domainEntity)
    {
        return _mapper.Map<TDatabase>(domainEntity);
    }
}
