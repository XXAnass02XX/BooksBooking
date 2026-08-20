using LibraryBooking.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LibraryBooking.Infrastructure.Tests;

// Base class shared by every repository test class below.
// Each test class gets its own throwaway SQLite database that only ever
// lives in memory, so tests never touch a real file on disk and never
// affect each other.
//
// SQLite (rather than EF Core's InMemory provider) is used on purpose: it
// actually enforces the things we configured in the Configurations classes
// (unique indexes on Isbn/Email, required columns, foreign keys), so the
// tests exercise the same rules the real database would.
public abstract class RepositoryTestBase : IDisposable
{
    // The connection has to stay open for the whole test. SQLite deletes an
    // in-memory database as soon as its last open connection closes, so
    // closing it early would wipe the data mid-test.
    private readonly SqliteConnection _connection;

    // Exposed so each test class can build the repository it is testing,
    // and call SaveChangesAsync directly where a test needs to.
    protected readonly LibraryBookingDbContext Context;

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LibraryBookingDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new LibraryBookingDbContext(options);

        // Creates the schema straight from the EF model (our
        // IEntityTypeConfiguration classes), without needing generated
        // migration files.
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
