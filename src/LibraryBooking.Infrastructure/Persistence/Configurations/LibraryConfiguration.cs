using LibraryBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryBooking.Infrastructure.Persistence.Configurations;

public class LibraryConfiguration : IEntityTypeConfiguration<Library>
{
    public void Configure(EntityTypeBuilder<Library> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(b => b.Address)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.HasMany(b => b.Copies)
            .WithOne(c => c.Library)
            .HasForeignKey(c => c.LibraryId);

        builder.Navigation(b => b.Copies)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}