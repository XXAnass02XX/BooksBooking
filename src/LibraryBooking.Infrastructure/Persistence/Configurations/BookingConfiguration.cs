using LibraryBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryBooking.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(bk => bk.Id);

        builder.Property(bk => bk.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(bk => bk.ReservedAtUtc)
            .IsRequired();

        builder.Property(bk => bk.ExpiresAtUtc)
            .IsRequired();

        builder.HasOne(bk => bk.Patron)
            .WithMany()
            .HasForeignKey(bk => bk.PatronId);

        builder.HasOne(bk => bk.BookCopy)
            .WithMany()
            .HasForeignKey(bk => bk.BookCopyId);
    }
}
