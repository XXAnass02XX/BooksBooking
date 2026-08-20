using LibraryBooking.Infrastructure.Persistence;

namespace LibraryBooking.Infrastructure.Repositories;

public class UnitOfWork
{
    private readonly LibraryBookingDbContext _context;

    public BookRepository Books { get; }
    public LibraryRepository Libraries { get; }
    public PatronRepository Patrons { get; }
    public BookCopyRepository BookCopies { get; }
    public BookingRepository Bookings { get; }

    public UnitOfWork(LibraryBookingDbContext context)
    {
        _context = context;
        Books = new BookRepository(_context);
        Libraries = new LibraryRepository(_context);
        Patrons = new PatronRepository(_context);
        BookCopies = new BookCopyRepository(_context);
        Bookings = new BookingRepository(_context);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
