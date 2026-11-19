using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Domain;

// Value object: a UTC time interval [StartUtc, EndUtc).
public sealed record DateRange
{
    private DateRange(DateTime startUtc, DateTime endUtc)
    {
        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    public DateTime StartUtc { get; }
    public DateTime EndUtc { get; }

    public TimeSpan Duration => EndUtc - StartUtc;

    // R3
    public static Result<DateRange> Create(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc)
            return BookingErrors.InvalidRange;

        return new DateRange(startUtc, endUtc);
    }

    // For values that were already validated (e.g. loaded from storage).
    internal static DateRange FromValidated(DateTime startUtc, DateTime endUtc) => new(startUtc, endUtc);

    // Strict <: intervals that only touch (one ends exactly when the other starts) do not overlap.
    public bool Overlaps(DateRange other) =>
        StartUtc < other.EndUtc && other.StartUtc < EndUtc;
}
