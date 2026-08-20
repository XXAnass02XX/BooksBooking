using LibraryBooking.Domain.Entities;
using LibraryBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryBooking.Infrastructure.Repositories;

public class BookRepository
{
    private readonly LibraryBookingDbContext _context;

    public BookRepository(LibraryBookingDbContext context)
    {
        _context = context;
    }

    public Task<Book?> GetByIdAsync(Guid id)
    {
        return _context.Books.FirstOrDefaultAsync(b => b.Id == id);
    }

    public Task<Book?> GetByIsbnAsync(string isbn)
    {
        return _context.Books.FirstOrDefaultAsync(b => b.Isbn == isbn);
    }

    public Task<bool> ExistsWithIsbnAsync(string isbn)
    {
        return _context.Books.AnyAsync(b => b.Isbn == isbn);
    }

    public async Task AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);
    }   
}
