using RoomHub.Api.Domain;

namespace RoomHub.Api.Infrastructure.InMemory;

// Temporary: in-memory data, lost on restart. Replaced by EF Core + SQLite in the next step.
public sealed class InMemoryRoomRepository : IRoomRepository
{
    private readonly List<Room> _rooms = new();
    private readonly object _lock = new();

    public Task<Room?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        lock (_lock) return Task.FromResult(_rooms.FirstOrDefault(r => r.Id == id));
    }

    public void Add(Room room)
    {
        lock (_lock) _rooms.Add(room);
    }
}

public sealed class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = new();
    private readonly object _lock = new();

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        lock (_lock) return Task.FromResult(_bookings.FirstOrDefault(b => b.Id == id));
    }

    public Task<List<Booking>> ListForRoomAsync(Guid roomId, CancellationToken ct)
    {
        lock (_lock) return Task.FromResult(_bookings.Where(b => b.RoomId == roomId).ToList());
    }

    public void Add(Booking booking)
    {
        lock (_lock) _bookings.Add(booking);
    }
}

// Objects in memory are already "saved", so there is nothing to commit.
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct) => Task.FromResult(0);
}
