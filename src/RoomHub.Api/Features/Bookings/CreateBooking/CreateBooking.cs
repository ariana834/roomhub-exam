using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain;
using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Features.Bookings.CreateBooking;

public sealed record CreateBookingRequest(string? BookedBy, DateTime StartUtc, DateTime EndUtc, int Attendees);

public sealed record CreateBookingCommand(Guid RoomId, string? BookedBy, DateTime StartUtc, DateTime EndUtc, int Attendees);

public sealed record BookingCreatedResponse(Guid Id, Guid RoomId, string Status);

public sealed class CreateBookingHandler(
    IRoomRepository rooms,
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateBookingCommand, BookingCreatedResponse>
{
    public async Task<Result<BookingCreatedResponse>> HandleAsync(CreateBookingCommand command, CancellationToken ct)
    {
        // 404 before any validation
        var room = await rooms.GetByIdAsync(command.RoomId, ct);
        if (room is null)
            return RoomErrors.NotFound;

        var existing = await bookings.ListActiveForRoomAsync(room.Id, ct);

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        var result = room.Book(
            command.BookedBy, command.StartUtc, command.EndUtc, command.Attendees, nowUtc, existing);
        if (result.IsFailure)
            return result.Error!;

        var booking = result.Value;
        bookings.Add(booking);

        try
        {
            await unitOfWork.SaveChangesAsync(ct); // one save: new booking + Room.Version bump
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request booked this room in the meantime (Version changed): treat it as a conflict.
            return BookingErrors.ConcurrentBooking;
        }

        return new BookingCreatedResponse(booking.Id, booking.RoomId, booking.Status.ToString());
    }
}

public static class CreateBookingEndpoint
{
    public static void MapCreateBooking(this RouteGroupBuilder group) =>
        group.MapPost("/{roomId:guid}/bookings", async Task<Results<Created<BookingCreatedResponse>, ProblemHttpResult>> (
            Guid roomId,
            CreateBookingRequest request,
            ICommandHandler<CreateBookingCommand, BookingCreatedResponse> handler,
            CancellationToken ct) =>
        {
            var command = new CreateBookingCommand(
                roomId, request.BookedBy, request.StartUtc, request.EndUtc, request.Attendees);

            var result = await handler.HandleAsync(command, ct);
            return result.IsSuccess
                ? TypedResults.Created($"/api/bookings/{result.Value.Id}", result.Value)
                : result.Error!.ToProblem();
        });
}
