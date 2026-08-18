using LibraryBooking.Domain.Common;
using LibraryBooking.Domain.Enums;

namespace LibraryBooking.Domain.Entities;

public class Booking : Entity
{
    public Guid PatronId { get; private set; }
    public Patron Patron { get; private set; }
    public Guid BookCopyId { get; private set; }
    public BookCopy BookCopy { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime ReservedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    private static readonly TimeSpan HoldDuration = TimeSpan.FromDays(2);

    private Booking(Patron patron, BookCopy bookCopy, DateTime reservedAtUtc) : base()
    {
        Patron = patron;
        PatronId = patron.Id;
        BookCopy = bookCopy;
        BookCopyId = bookCopy.Id;
        Status = BookingStatus.Reserved;
        ReservedAtUtc = reservedAtUtc;
        ExpiresAtUtc = reservedAtUtc.Add(HoldDuration);
    }

    public static Booking Create(Patron patron, BookCopy bookCopy, DateTime reservedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(patron);
        ArgumentNullException.ThrowIfNull(bookCopy);

        bookCopy.Reserve();

        return new Booking(patron, bookCopy, reservedAtUtc);
    }

    public void Cancel()
    {
        if (Status != BookingStatus.Reserved)
            throw new DomainException("Only reserved bookings can be cancelled.");

        BookCopy.Release();
        Status = BookingStatus.Cancelled;
    }

    public void Fulfill()
    {
        if (Status != BookingStatus.Reserved)
            throw new DomainException("Only reserved bookings can be fulfilled.");

        BookCopy.CheckOut();
        Status = BookingStatus.Fulfilled;
    }

    public void Expire(DateTime asOfUtc)
    {
        if (Status != BookingStatus.Reserved)
            throw new DomainException("Only reserved bookings can expire.");

        if (asOfUtc < ExpiresAtUtc)
            throw new DomainException("Booking has not passed its expiry time yet.");

        BookCopy.Release();
        Status = BookingStatus.Expired;
    }
}
