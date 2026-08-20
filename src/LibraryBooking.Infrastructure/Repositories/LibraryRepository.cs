using LibraryBooking.Domain.Entities;
using LibraryBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryBooking.Infrastructure.Repositories;

public class LibraryRepository
{
    private readonly LibraryBookingDbContext _context;

    public LibraryRepository(LibraryBookingDbContext context)
    {
        _context = context;
    }

    public Task<Library?> GetByIdAsync(Guid id)
    {
        return _context.Library
            .Include(l => l.Copies)
            .ThenInclude(c => c.Book)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Library>> GetAllAsync()
    {
        return await _context.Library.ToListAsync();
    }

    public async Task AddAsync(Library library)
    {
        await _context.Library.AddAsync(library);
    }
}
