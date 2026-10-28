using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovaStay.Infrastructure.ContextDB;

// Metadata-only configuration. Migration generation must never start the API or read production .env.
public sealed class DesignTimeHostContextFactory : IDesignTimeDbContextFactory<HostContext>
{
    public HostContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<HostContext>()
        .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=NovaStayMigrationDesignOnly;Integrated Security=true;TrustServerCertificate=true").Options);
}
