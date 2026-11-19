using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Domain;
using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Features.Bookings.CancelBooking;

public sealed record CancelBookingCommand(Guid Id);

public sealed class CancelBookingHandler(
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CancelBookingCommand>
{
    public async Task<Result> HandleAsync(CancelBookingCommand command, CancellationToken ct)
    {
        var booking = await bookings.GetByIdAsync(command.Id, ct);
        if (booking is null)
            return BookingErrors.NotFound;

        var result = booking.Cancel(timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public static class CancelBookingEndpoint
{
    public static void MapCancelBooking(this RouteGroupBuilder group) =>
        group.MapPost("/{id:guid}/cancel", async Task<Results<NoContent, ProblemHttpResult>> (
            Guid id,
            ICommandHandler<CancelBookingCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new CancelBookingCommand(id), ct);
            return result.IsSuccess ? TypedResults.NoContent() : result.Error!.ToProblem();
        });
}
