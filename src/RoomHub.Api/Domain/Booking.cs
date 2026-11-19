using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Domain;

public enum BookingStatus
{
    Active,
    Cancelled
}

public sealed class Booking
{
    private Booking() { } // EF Core materializes through this; code must use Room.Book

    private Booking(Guid id, Guid roomId, string bookedBy, DateRange period, int attendees)
    {
        Id = id;
        RoomId = roomId;
        BookedBy = bookedBy;
        StartUtc = period.StartUtc;
        EndUtc = period.EndUtc;
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

    public DateRange Period => DateRange.FromValidated(StartUtc, EndUtc);
    public bool IsActive => Status == BookingStatus.Active;

    // Only Room.Book creates bookings, after it has checked the rules.
    internal static Booking Create(Guid roomId, string bookedBy, DateRange period, int attendees) =>
        new(Guid.NewGuid(), roomId, bookedBy, period, attendees);

    public static readonly TimeSpan MinCancelNotice = TimeSpan.FromMinutes(60);

    // R8, R11 (exactly 60 minutes before the start is still allowed).
    public Result Cancel(DateTime nowUtc)
    {
        if (!IsActive)
            return BookingErrors.AlreadyCancelled;

        if (StartUtc - nowUtc < MinCancelNotice)
            return BookingErrors.CancelTooLate;

        Status = BookingStatus.Cancelled;
        return Result.Success();
    }
}
