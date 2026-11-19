using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Domain;

public enum BookingStatus
{
    Active,
    Cancelled
}

public sealed class Booking
{
    private Booking() { } // for EF Core later

    private Booking(Guid id, Guid roomId, string bookedBy, DateTime startUtc, DateTime endUtc, int attendees)
    {
        Id = id;
        RoomId = roomId;
        BookedBy = bookedBy;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Attendees = attendees;
        Status = BookingStatus.Active;
    }

    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public string BookedBy { get; private set; } = string.Empty;
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public int Attendees { get; private set; }
    public BookingStatus Status { get; private set; }

    // R2, R3, R4, R7. existingBookings = the bookings of the same room.
    // BUG kept (baseline): R5 (start in the past) and R6 (max 4h) are not checked.
    public static Result<Booking> Create(
        Room room,
        string? bookedBy,
        DateTime startUtc,
        DateTime endUtc,
        int attendees,
        IEnumerable<Booking> existingBookings)
    {
        if (string.IsNullOrWhiteSpace(bookedBy))
            return BookingErrors.InvalidBookedBy;               // R2

        if (endUtc <= startUtc)
            return BookingErrors.InvalidRange;                  // R3

        if (attendees <= 0)
            return BookingErrors.InvalidAttendees;              // R4

        if (attendees > room.Capacity)
            return BookingErrors.AttendeesExceedCapacity;       // R4

        if (existingBookings.Any(b => b.Overlaps(startUtc, endUtc)))
            return BookingErrors.RoomOccupied;                  // R7

        return new Booking(Guid.NewGuid(), room.Id, bookedBy.Trim(), startUtc, endUtc, attendees);
    }

    // BUG kept (baseline): uses <= so touching intervals overlap, and cancelled bookings still block.
    public bool Overlaps(DateTime startUtc, DateTime endUtc) =>
        StartUtc <= endUtc && startUtc <= EndUtc;

    // BUG kept (baseline): cancelling an already cancelled booking succeeds instead of 409.
    public Result Cancel()
    {
        Status = BookingStatus.Cancelled;
        return Result.Success();
    }
}
