using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain;
using RoomHub.Api.Features.Shared;
using RoomHub.Api.Infrastructure.Persistence;

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

// Query: reads the DbContext directly, read-only (AsNoTracking), no repository / unit of work.
public sealed class GetBookingHandler(RoomHubDbContext db) : IQueryHandler<GetBookingQuery, BookingResponse>
{
    public async Task<Result<BookingResponse>> HandleAsync(GetBookingQuery query, CancellationToken ct)
    {
        var b = await db.Bookings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.Id, ct);
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
