using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomHub.Api.Domain;

namespace RoomHub.Api.Infrastructure.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Capacity).IsRequired();

        // Optimistic concurrency: UPDATE ... WHERE Id = @id AND Version = @originalVersion.
        // Room.Book bumps Version, so two parallel bookings of the same room cannot both be saved.
        builder.Property(r => r.Version).IsConcurrencyToken();

        builder.Ignore(r => r.IsLarge);
        builder.Ignore(r => r.MaxDuration);
    }
}
