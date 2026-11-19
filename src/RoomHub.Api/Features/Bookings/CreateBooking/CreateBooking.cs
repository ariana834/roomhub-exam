using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Domain;
using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Features.Bookings.CreateBooking;

public sealed record CreateBookingRequest(string? BookedBy, DateTime StartUtc, DateTime EndUtc, int Attendees);

public sealed record CreateBookingCommand(Guid RoomId, string? BookedBy, DateTime StartUtc, DateTime EndUtc, int Attendees);

public sealed record BookingCreatedResponse(Guid Id, Guid RoomId, string Status);

public sealed class CreateBookingHandler(IRoomRepository rooms, IBookingRepository bookings, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBookingCommand, BookingCreatedResponse>
{
    public async Task<Result<BookingCreatedResponse>> HandleAsync(CreateBookingCommand command, CancellationToken ct)
    {
        // 404 before any validation
        var room = await rooms.GetByIdAsync(command.RoomId, ct);
        if (room is null)
            return RoomErrors.NotFound;

        var existing = await bookings.ListForRoomAsync(room.Id, ct);

        var result = room.Book(
            command.BookedBy, command.StartUtc, command.EndUtc, command.Attendees, existing);
        if (result.IsFailure)
            return result.Error!;

        var booking = result.Value;
        bookings.Add(booking);
        await unitOfWork.SaveChangesAsync(ct);

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
