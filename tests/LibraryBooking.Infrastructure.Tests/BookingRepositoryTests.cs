using LibraryBooking.Domain.Entities;
using LibraryBooking.Infrastructure.Repositories;

namespace LibraryBooking.Infrastructure.Tests;

public class BookingRepositoryTests : RepositoryTestBase
{
    private readonly BookingRepository _sut;
    private readonly LibraryRepository _libraryRepository;
    private readonly PatronRepository _patronRepository;

    public BookingRepositoryTests()
    {
        _sut = new BookingRepository(Context);
        _libraryRepository = new LibraryRepository(Context);
        _patronRepository = new PatronRepository(Context);
    }

    // Helper that creates and persists one available BookCopy, distinct
    // from any other copy created by the same helper, so each test can get
    // as many independent copies as it needs to book.
    private async Task<BookCopy> CreateAvailableCopyAsync(string isbn)
    {
        var library = Library.Create("Central Library", "1 Main St");
        var book = Book.Create("Some Book " + isbn, "Some Author", isbn);
        var copy = library.AddCopy(book);

        await _libraryRepository.AddAsync(library);
        await Context.SaveChangesAsync();

        return copy;
    }

    private async Task<Patron> CreatePatronAsync(string email)
    {
        var patron = Patron.Create("Patron " + email, email);

        await _patronRepository.AddAsync(patron);
        await Context.SaveChangesAsync();

        return patron;
    }

    [Fact]
    public async Task AddAsync_Then_SaveChanges_PersistsTheBooking()
    {
        var patron = await CreatePatronAsync("reader1@example.com");
        var copy = await CreateAvailableCopyAsync("isbn-1");

        var booking = Booking.Create(patron, copy, DateTime.UtcNow);

        await _sut.AddAsync(booking);
        await Context.SaveChangesAsync();

        var stored = await _sut.GetByIdAsync(booking.Id);

        Assert.NotNull(stored);
        Assert.Equal(patron.Id, stored!.PatronId);
        Assert.Equal(copy.Id, stored.BookCopyId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNoBookingHasThatId()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveByPatronAsync_ReturnsOnlyReservedBookingsForThatPatron()
    {
        var targetPatron = await CreatePatronAsync("target@example.com");
        var otherPatron = await CreatePatronAsync("other@example.com");

        var reservedCopy = await CreateAvailableCopyAsync("isbn-active-1");
        var cancelledCopy = await CreateAvailableCopyAsync("isbn-active-2");
        var otherPatronCopy = await CreateAvailableCopyAsync("isbn-active-3");

        // Still Reserved -- this is the only booking the test expects back.
        var activeBooking = Booking.Create(targetPatron, reservedCopy, DateTime.UtcNow);

        // Belongs to the target patron but no longer active, so it must be
        // excluded from the result.
        var cancelledBooking = Booking.Create(targetPatron, cancelledCopy, DateTime.UtcNow);
        cancelledBooking.Cancel();

        // Still Reserved, but belongs to a different patron, so it must
        // also be excluded from the result.
        Booking.Create(otherPatron, otherPatronCopy, DateTime.UtcNow);

        await _sut.AddAsync(activeBooking);
        await _sut.AddAsync(cancelledBooking);
        await Context.SaveChangesAsync();

        var activeBookings = await _sut.GetActiveByPatronAsync(targetPatron.Id);

        var onlyBooking = Assert.Single(activeBookings);
        Assert.Equal(activeBooking.Id, onlyBooking.Id);
    }

    [Fact]
    public async Task GetActiveByPatronAsync_ReturnsEmpty_WhenPatronHasNoReservedBookings()
    {
        var patron = await CreatePatronAsync("nobookings@example.com");

        var activeBookings = await _sut.GetActiveByPatronAsync(patron.Id);

        Assert.Empty(activeBookings);
    }

    [Fact]
    public async Task GetExpiredReservationsAsync_ReturnsOnlyReservedBookingsPastExpiry()
    {
        var patron = await CreatePatronAsync("expiry@example.com");
        var expiredCopy = await CreateAvailableCopyAsync("isbn-expiry-1");
        var stillValidCopy = await CreateAvailableCopyAsync("isbn-expiry-2");

        // Reserved three days ago; the hold only lasts two days, so this
        // reservation has already expired.
        var expiredBooking = Booking.Create(patron, expiredCopy, DateTime.UtcNow.AddDays(-3));

        // Reserved just now, so its hold has not expired yet.
        var freshBooking = Booking.Create(patron, stillValidCopy, DateTime.UtcNow);

        await _sut.AddAsync(expiredBooking);
        await _sut.AddAsync(freshBooking);
        await Context.SaveChangesAsync();

        var expired = await _sut.GetExpiredReservationsAsync(DateTime.UtcNow);

        var onlyBooking = Assert.Single(expired);
        Assert.Equal(expiredBooking.Id, onlyBooking.Id);
    }

    [Fact]
    public async Task GetExpiredReservationsAsync_ExcludesReservationsThatAreNotReservedAnymore()
    {
        var patron = await CreatePatronAsync("notreserved@example.com");
        var copy = await CreateAvailableCopyAsync("isbn-expiry-3");

        // Reserved long enough ago to be past its expiry, but already
        // cancelled -- so it must not show up as an expired reservation.
        var booking = Booking.Create(patron, copy, DateTime.UtcNow.AddDays(-3));
        booking.Cancel();

        await _sut.AddAsync(booking);
        await Context.SaveChangesAsync();

        var expired = await _sut.GetExpiredReservationsAsync(DateTime.UtcNow);

        Assert.Empty(expired);
    }
}
