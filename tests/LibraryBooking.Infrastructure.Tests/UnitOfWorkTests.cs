using LibraryBooking.Domain.Entities;
using LibraryBooking.Infrastructure.Repositories;

namespace LibraryBooking.Infrastructure.Tests;

public class UnitOfWorkTests : RepositoryTestBase
{
    private readonly UnitOfWork _sut;

    public UnitOfWorkTests()
    {
        _sut = new UnitOfWork(Context);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsChangesStagedThroughItsRepositories()
    {
        var book = Book.Create("The Hobbit", "J.R.R. Tolkien", "978-0345339683");

        await _sut.Books.AddAsync(book);
        var rowsWritten = await _sut.SaveChangesAsync();

        Assert.Equal(1, rowsWritten);

        var stored = await _sut.Books.GetByIdAsync(book.Id);
        Assert.NotNull(stored);
    }

    // The reason UnitOfWork exists at all: several repositories can stage
    // changes to different tables, and one SaveChangesAsync call commits
    // all of them together because they all share the same DbContext.
    [Fact]
    public async Task SaveChangesAsync_CommitsChangesFromMultipleRepositoriesInOneCall()
    {
        var book = Book.Create("Fahrenheit 451", "Ray Bradbury", "978-1451673319");
        var library = Library.Create("Central Library", "1 Main St");
        var patron = Patron.Create("Dana", "dana@example.com");

        await _sut.Books.AddAsync(book);
        await _sut.Libraries.AddAsync(library);
        await _sut.Patrons.AddAsync(patron);

        var rowsWritten = await _sut.SaveChangesAsync();

        // One row per entity that was staged for insertion.
        Assert.Equal(3, rowsWritten);

        Assert.NotNull(await _sut.Books.GetByIdAsync(book.Id));
        Assert.NotNull(await _sut.Libraries.GetByIdAsync(library.Id));
        Assert.NotNull(await _sut.Patrons.GetByIdAsync(patron.Id));
    }

    // Booking.Create both reserves an existing BookCopy (an update) and
    // creates a new Booking (an insert). This test checks that a single
    // SaveChangesAsync call through UnitOfWork commits both halves of that
    // one logical operation together.
    [Fact]
    public async Task SaveChangesAsync_CommitsBothTheNewBookingAndTheReservedCopyTogether()
    {
        var library = Library.Create("Central Library", "1 Main St");
        var book = Book.Create("Brave New World", "Aldous Huxley", "978-0060850524");
        var copy = library.AddCopy(book);
        var patron = Patron.Create("Eli", "eli@example.com");

        await _sut.Libraries.AddAsync(library);
        await _sut.Patrons.AddAsync(patron);
        await _sut.SaveChangesAsync();

        var booking = Booking.Create(patron, copy, DateTime.UtcNow);
        await _sut.Bookings.AddAsync(booking);
        await _sut.SaveChangesAsync();

        var storedBooking = await _sut.Bookings.GetByIdAsync(booking.Id);
        var storedCopy = await _sut.BookCopies.GetByIdAsync(copy.Id);

        Assert.NotNull(storedBooking);
        Assert.NotNull(storedCopy);
        Assert.False(storedCopy!.IsAvailable);
    }
}
