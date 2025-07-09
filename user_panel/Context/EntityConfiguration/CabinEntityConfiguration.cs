using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using user_panel.Data;

namespace user_panel.Context.EntityConfiguration
{
    public class CabinConfiguration : IEntityTypeConfiguration<Cabin>
    {
        public void Configure(EntityTypeBuilder<Cabin> builder)
        {
            // Table name mapping
            builder.ToTable("cab");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Location)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .IsRequired();

            builder.Property(c => c.PricePerHour)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.HasMany(c => c.Bookings)
                .WithOne(b => b.Cabin)
                .HasForeignKey(b => b.CabinId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
