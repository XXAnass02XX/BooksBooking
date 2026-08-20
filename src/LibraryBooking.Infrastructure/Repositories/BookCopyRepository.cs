using LibraryBooking.Domain.Entities;
using LibraryBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryBooking.Infrastructure.Repositories;

public class BookCopyRepository
{
    private readonly LibraryBookingDbContext _context;

    public BookCopyRepository(LibraryBookingDbContext context)
    {
        _context = context;
    }

    public Task<BookCopy?> GetByIdAsync(Guid id)
    {
        return _context.BookCopies.FirstOrDefaultAsync(c => c.Id == id);
    }
}
