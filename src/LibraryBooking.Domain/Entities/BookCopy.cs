using LibraryBooking.Domain.Common;
using LibraryBooking.Domain.Enums;

namespace LibraryBooking.Domain.Entities;

public class BookCopy : Entity
{
    public Guid BookId { get; private set; }
    public Book Book { get; private set; }
    public Guid LibraryId { get; private set; }
    public Library Library { get; private set; }
    public BookCopyStatus Status { get; private set; }

    // Required by EF Core: constructor binding can only bind scalar values,
    // never references to other entities, so the (Book, Library) constructor
    // below is unusable for materializing rows read back from the database.
    // EF calls this instead and fills every property itself via the private
    // setters above.
    private BookCopy()
    {
        Book = null!;
        Library = null!;
    }

    private BookCopy(Book book, Library library) : base()
    {
        Book = book;
        BookId = book.Id;
        Library = library;
        LibraryId = library.Id;
        Status = BookCopyStatus.Available;
    }

    internal static BookCopy Create(Book book, Library library) => new(book, library);

    public bool IsAvailable => Status == BookCopyStatus.Available;

    internal void Reserve()
    {
        if (Status != BookCopyStatus.Available)
            throw new DomainException("Only available copies can be booked.");

        Status = BookCopyStatus.Booked;
    }

    internal void Release()
    {
        if (Status != BookCopyStatus.Booked)
            throw new DomainException("Only booked copies can be released.");

        Status = BookCopyStatus.Available;
    }

    internal void CheckOut()
    {
        if (Status != BookCopyStatus.Booked)
            throw new DomainException("Only booked copies can be checked out.");

        Status = BookCopyStatus.CheckedOut;
    }

    public void Return()
    {
        if (Status != BookCopyStatus.CheckedOut)
            throw new DomainException("Only checked-out copies can be returned.");

        Status = BookCopyStatus.Available;
    }

    public void MarkAsLost()
    {
        if (Status == BookCopyStatus.Lost)
            throw new DomainException("Copy is already marked as lost.");

        Status = BookCopyStatus.Lost;
    }
}
