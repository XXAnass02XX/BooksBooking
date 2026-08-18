using LibraryBooking.Domain.Common;

namespace LibraryBooking.Domain.Entities;

public class Book : Entity
{
    private readonly List<BookCopy> _copies = [];

    public string Title { get; private set; }
    public string Author { get; private set; }
    public string Isbn { get; private set; }
    public IReadOnlyCollection<BookCopy> Copies => _copies.AsReadOnly();

    private Book(string title, string author, string isbn) : base()
    {
        Title = title;
        Author = author;
        Isbn = isbn;
    }

    public static Book Create(string title, string author, string isbn)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Book title is required.");

        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("Book author is required.");

        if (string.IsNullOrWhiteSpace(isbn))
            throw new DomainException("Book ISBN is required.");

        return new Book(title.Trim(), author.Trim(), isbn.Trim());
    }

    internal void AddCopy(BookCopy copy) => _copies.Add(copy);
}
