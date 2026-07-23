using JiujitsuGymApp.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Tests.Infrastructure;

/// <summary>
/// Spins up a SQLite in-memory database that lives for the duration of one test class.
/// The database exists only while the connection stays open, and every test class gets
/// its own database, so tests are isolated from each other and can run in parallel.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    /// <summary>
    /// Creates a fresh DbContext on the shared connection. Arrange, act, and assert with
    /// separate contexts so the change tracker of one phase cannot mask what was (or was
    /// not) actually saved to the database.
    /// </summary>
    public ApplicationDbContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
