using System.Data;
using System.Data.Common;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArchitecture.Application.FunctionalTests;

public class SqliteTestDatabase : ITestDatabase
{
    private readonly string _connectionString;
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connectionString = "DataSource=:memory:";
        _connection = new SqliteConnection(_connectionString);
    }

    public async Task InitialiseAsync()
    {
        if (_connection.State == ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }

        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            // IdentityDbContext resolves key-length conventions differently when it isn't built
            // through the app's DI container (ApplicationServiceProvider), so a context built
            // directly like this always reports a spurious pending model change. Known EF Core
            // behavior, not a real migration gap: https://github.com/dotnet/efcore/issues/36314
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        var context = new ApplicationDbContext(options);

        context.Database.Migrate();
    }

    public DbConnection GetConnection()
    {
        return _connection;
    }

    public async Task ResetAsync()
    {
        await InitialiseAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}