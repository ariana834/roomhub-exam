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
    void Add(Booking booking);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
