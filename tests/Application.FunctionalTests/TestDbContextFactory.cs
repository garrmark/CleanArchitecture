using CleanArchitecture.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Application.FunctionalTests;

internal static class TestDbContextFactory
{
    // ApplicationDbContext inherits IdentityDbContext, whose OnModelCreating only applies the
    // historical 128-character key length (matching the committed migrations) when an
    // IOptions<IdentityOptions> is reachable via the application service provider, which the
    // real app gets from AddDefaultIdentity. Without it, the EF9 model falls back to SQL
    // Server's default 450-character key length, and Database.Migrate() throws
    // PendingModelChangesWarning because that no longer matches the last migration's snapshot.
    public static ApplicationDbContext Create(Action<DbContextOptionsBuilder<ApplicationDbContext>> configureProvider)
    {
        var services = new ServiceCollection();
        services.Configure<IdentityOptions>(options => options.Stores.MaxLengthForKeys = 128);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseApplicationServiceProvider(services.BuildServiceProvider());

        configureProvider(optionsBuilder);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
