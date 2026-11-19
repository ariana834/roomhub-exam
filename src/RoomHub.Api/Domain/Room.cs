using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Domain;

public sealed class Room
{
    private Room() { } // EF Core materializes through this; code must use Create

    private Room(Guid id, string name, int capacity)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }

    // Concurrency token, bumped by Book: of two parallel bookings of the same room only one
    // can be saved, so both cannot pass the overlap check on stale data.
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

    public const int LargeRoomCapacity = 10;
    public static readonly TimeSpan MaxBookingDuration = TimeSpan.FromHours(4);
    public static readonly TimeSpan LargeRoomMaxBookingDuration = TimeSpan.FromHours(2);

    public bool IsLarge => Capacity > LargeRoomCapacity;

    // R6 + R10
    public TimeSpan MaxDuration => IsLarge ? LargeRoomMaxBookingDuration : MaxBookingDuration;

    public const int MaxActiveBookingsPerPersonPerDay = 2;

    // R2-R10. The domain has no database access, so the handler loads what the rules need:
    // the active bookings of this room, and the person's active bookings that UTC day in all rooms.
    public Result<Booking> Book(
        string? bookedBy,
        DateTime startUtc,
        DateTime endUtc,
        int attendees,
        DateTime nowUtc,
        IEnumerable<Booking> existingBookings,
        int personBookingsThatDay)
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

        if (range.Duration > MaxDuration)
            return IsLarge
                ? BookingErrors.TooLongForLargeRoom             // R10 (exactly 2h is allowed)
                : BookingErrors.TooLong;                        // R6 (exactly 4h is allowed)

        if (existingBookings.Any(b => b.IsActive && b.Period.Overlaps(range)))
            return BookingErrors.RoomOccupied;                  // R7

        if (personBookingsThatDay >= MaxActiveBookingsPerPersonPerDay)
            return BookingErrors.TooManyBookingsPerDay;         // R9

        Version++;
        return Booking.Create(Id, bookedBy.Trim(), range, attendees);
    }
}
