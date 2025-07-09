using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using user_panel.Data;

namespace user_panel.Context.EntityConfiguration
{

    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.StartTime).IsRequired();
            builder.Property(b => b.EndTime).IsRequired();
            builder.Property(b => b.ApplicationUserId).IsRequired();

            builder.HasOne(b => b.ApplicationUser)
                .WithMany()
                .HasForeignKey(b => b.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(b => b.CabinId).IsRequired();

            builder.HasOne(b => b.Cabin)
                .WithMany()
                .HasForeignKey(b => b.CabinId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.ApplicationUser)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Cabin)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CabinId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
