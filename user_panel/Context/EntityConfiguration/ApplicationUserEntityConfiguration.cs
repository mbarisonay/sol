using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using user_panel.Data;

namespace user_panel.Context.EntityConfiguration
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.CreditBalance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.HasMany(u => u.Bookings)
                .WithOne(b => b.ApplicationUser)
                .HasForeignKey(b => b.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.CabinReservations)
                .WithOne(cr => cr.ApplicationUser)
                .HasForeignKey(cr => cr.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
