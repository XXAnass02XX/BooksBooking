using LibraryBooking.Domain.Common;

namespace LibraryBooking.Domain.Entities;

public class Library : Entity
{
    private readonly List<BookCopy> _copies = [];

    public string Name { get; private set; }
    public string Address { get; private set; }
    public IReadOnlyCollection<BookCopy> Copies => _copies.AsReadOnly();

    private Library(string name, string address) : base()
    {
        Name = name;
        Address = address;
    }

    public static Library Create(string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Library name is required.");

        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Library address is required.");

        return new Library(name.Trim(), address.Trim());
    }

    public BookCopy AddCopy(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        var copy = BookCopy.Create(book, this);
        _copies.Add(copy);
        book.AddCopy(copy);
        return copy;
    }

    public IEnumerable<BookCopy> GetAvailableCopies(Book book) =>
        _copies.Where(c => c.BookId == book.Id && c.Status == Enums.BookCopyStatus.Available);

    public bool HasAvailableCopy(Book book) => GetAvailableCopies(book).Any();
}
