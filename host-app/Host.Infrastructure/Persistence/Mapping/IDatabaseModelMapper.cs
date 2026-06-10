namespace Host.Infrastructure.Persistence.Mapping;

internal interface IDatabaseModelMapper<TDomain, TDatabase>
    where TDomain : class
    where TDatabase : class
{
    TDomain ToDomain(TDatabase databaseModel);

    TDatabase ToDatabase(TDomain domainEntity);
}
