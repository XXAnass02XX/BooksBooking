using LibraryBooking.Domain.Common;

namespace LibraryBooking.Domain.Entities;

public class Patron : Entity
{
    public string Name { get; private set; }
    public string Email { get; private set; }

    private Patron(string name, string email) : base()
    {
        Name = name;
        Email = email;
    }

    public static Patron Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Patron name is required.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("Patron email is invalid.");

        return new Patron(name.Trim(), email.Trim());
    }
}
