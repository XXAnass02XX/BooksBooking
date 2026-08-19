using LibraryBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryBooking.Infrastructure.Persistence;

public class LibraryBookingDbContext : DbContext
{
    public DbSet<Book>  Books { get; set; }
    public DbSet<BookCopy> BookCopies { get; set; }
    public DbSet<Library> Library {get; set;}
    public DbSet<Patron> Patrons { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    public LibraryBookingDbContext(DbContextOptions<LibraryBookingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryBookingDbContext).Assembly);
    }
}