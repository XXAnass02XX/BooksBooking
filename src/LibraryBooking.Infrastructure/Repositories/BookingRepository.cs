using LibraryBooking.Domain.Entities;
using LibraryBooking.Domain.Enums;
using LibraryBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryBooking.Infrastructure.Repositories;

public class BookingRepository
{
    private readonly LibraryBookingDbContext _context;

    public BookingRepository(LibraryBookingDbContext context)
    {
        _context = context;
    }

    public Task<Booking?> GetByIdAsync(Guid id)
    {
        return _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Booking>> GetActiveByPatronAsync(Guid patronId)
    {
        return await _context.Bookings
            .Where(b => b.PatronId == patronId && b.Status == BookingStatus.Reserved)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetExpiredReservationsAsync(DateTime asOfUtc)
    {
        return await _context.Bookings
            .Where(b => b.Status == BookingStatus.Reserved && b.ExpiresAtUtc < asOfUtc)
            .ToListAsync();
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
    }
}
