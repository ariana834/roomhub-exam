using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Domain;
using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Features.Bookings.GetBooking;

public sealed record GetBookingQuery(Guid Id);

public sealed record BookingResponse(
    Guid Id,
    Guid RoomId,
    string BookedBy,
    DateTime StartUtc,
    DateTime EndUtc,
    int Attendees,
    string Status);

// Temporary: reads through the repository. With EF Core it will read the DbContext directly (AsNoTracking).
public sealed class GetBookingHandler(IBookingRepository bookings) : IQueryHandler<GetBookingQuery, BookingResponse>
{
    public async Task<Result<BookingResponse>> HandleAsync(GetBookingQuery query, CancellationToken ct)
    {
        var b = await bookings.GetByIdAsync(query.Id, ct);
        if (b is null)
            return BookingErrors.NotFound;

        return new BookingResponse(b.Id, b.RoomId, b.BookedBy, b.StartUtc, b.EndUtc, b.Attendees, b.Status.ToString());
    }
}

public static class GetBookingEndpoint
{
    public static void MapGetBooking(this RouteGroupBuilder group) =>
        group.MapGet("/{id:guid}", async Task<Results<Ok<BookingResponse>, ProblemHttpResult>> (
            Guid id,
            IQueryHandler<GetBookingQuery, BookingResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetBookingQuery(id), ct);
            return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error!.ToProblem();
        });
}
