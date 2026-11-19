using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain;

namespace RoomHub.Api.Infrastructure.Persistence;

// Repositories are used only by commands: they return tracked entities and never call SaveChanges
// (the handler does that once, through IUnitOfWork). Queries read the DbContext directly with AsNoTracking.
public sealed class EfRoomRepository(RoomHubDbContext db) : IRoomRepository
{
    public Task<Room?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);

    public void Add(Room room) => db.Rooms.Add(room);
}

public sealed class EfBookingRepository(RoomHubDbContext db) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<List<Booking>> ListActiveForRoomAsync(Guid roomId, CancellationToken ct) =>
        db.Bookings
            .Where(b => b.RoomId == roomId && b.Status == BookingStatus.Active)
            .ToListAsync(ct);

    public Task<int> CountActiveForPersonOnDayAsync(string bookedBy, DateOnly dayUtc, CancellationToken ct)
    {
        var person = bookedBy.Trim().ToLower();
        var dayStart = dayUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var nextDayStart = dayStart.AddDays(1);

        return db.Bookings.CountAsync(b =>
            b.Status == BookingStatus.Active &&
            b.BookedBy.ToLower() == person &&
            b.StartUtc >= dayStart && b.StartUtc < nextDayStart, ct);
    }

    public void Add(Booking booking) => db.Bookings.Add(booking);
}
