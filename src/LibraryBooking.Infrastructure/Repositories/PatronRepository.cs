using LibraryBooking.Domain.Entities;
using LibraryBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryBooking.Infrastructure.Repositories;

public class PatronRepository
{
    private readonly LibraryBookingDbContext _context;

    public PatronRepository(LibraryBookingDbContext context)
    {
        _context = context;
    }

    public Task<Patron?> GetByIdAsync(Guid id)
    {
        return _context.Patrons.FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<Patron?> GetByEmailAsync(string email)
    {
        return _context.Patrons.FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task AddAsync(Patron patron)
    {
        await _context.Patrons.AddAsync(patron);
    }
}
