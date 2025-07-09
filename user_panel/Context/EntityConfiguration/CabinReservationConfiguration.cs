using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using user_panel.Data;

namespace user_panel.Context.EntityConfiguration
{
    public class CabinReservationConfiguration : IEntityTypeConfiguration<CabinReservation>
    {
        public void Configure(EntityTypeBuilder<CabinReservation> builder)
        {
            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.Location)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(cr => cr.Description)
                .IsRequired();

            builder.Property(cr => cr.StartTime)
                .IsRequired();

            builder.Property(cr => cr.EndTime)
                .IsRequired();

            builder.Property(cr => cr.ApplicationUserId)
                .IsRequired();

            builder.HasOne(cr => cr.ApplicationUser)
                .WithMany(u => u.CabinReservations)
                .HasForeignKey(cr => cr.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
