using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Domain;

public sealed class Room
{
    private Room() { } // for EF Core later

    private Room(Guid id, string name, int capacity)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }

    // Bumped on every successful booking; will be the EF Core concurrency token,
    // so two parallel bookings of the same room cannot both pass the overlap check.
    public int Version { get; private set; }

    // R1
    public static Result<Room> Create(string? name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RoomErrors.InvalidName;

        if (capacity <= 0)
            return RoomErrors.InvalidCapacity;

        return new Room(Guid.NewGuid(), name.Trim(), capacity);
    }

    public static readonly TimeSpan MaxBookingDuration = TimeSpan.FromHours(4);

    // R2-R7. existingBookings = the bookings of this room; nowUtc comes from TimeProvider.
    public Result<Booking> Book(
        string? bookedBy,
        DateTime startUtc,
        DateTime endUtc,
        int attendees,
        DateTime nowUtc,
        IEnumerable<Booking> existingBookings)
    {
        if (string.IsNullOrWhiteSpace(bookedBy))
            return BookingErrors.InvalidBookedBy;               // R2

        var rangeResult = DateRange.Create(startUtc, endUtc);  // R3
        if (rangeResult.IsFailure)
            return rangeResult.Error!;
        var range = rangeResult.Value;

        if (attendees <= 0)
            return BookingErrors.InvalidAttendees;              // R4

        if (attendees > Capacity)
            return BookingErrors.AttendeesExceedCapacity;       // R4

        if (range.StartUtc < nowUtc)
            return BookingErrors.StartInPast;                   // R5 (start == now is allowed)

        if (range.Duration > MaxBookingDuration)
            return BookingErrors.TooLong;                       // R6 (exactly 4h is allowed)

        if (existingBookings.Any(b => b.IsActive && b.Period.Overlaps(range)))
            return BookingErrors.RoomOccupied;                  // R7 (only active bookings block)

        Version++;
        return Booking.Create(Id, bookedBy.Trim(), range, attendees);
    }
}
