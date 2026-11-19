namespace RoomHub.Api.Domain;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(Room room);
}

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Booking>> ListActiveForRoomAsync(Guid roomId, CancellationToken ct);

    // R9: active bookings of one person (trimmed, case-insensitive) starting on the given UTC day, all rooms.
    Task<int> CountActiveForPersonOnDayAsync(string bookedBy, DateOnly dayUtc, CancellationToken ct);
    void Add(Booking booking);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
