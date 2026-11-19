using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Domain;
using RoomHub.Api.Features.Rooms.CreateRoom;
using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Features.Rooms.GetRoom;

public sealed record GetRoomQuery(Guid Id);

// Temporary: reads through the repository. With EF Core it will read the DbContext directly (AsNoTracking).
public sealed class GetRoomHandler(IRoomRepository rooms) : IQueryHandler<GetRoomQuery, RoomResponse>
{
    public async Task<Result<RoomResponse>> HandleAsync(GetRoomQuery query, CancellationToken ct)
    {
        var room = await rooms.GetByIdAsync(query.Id, ct);
        if (room is null)
            return RoomErrors.NotFound;

        return new RoomResponse(room.Id, room.Name, room.Capacity);
    }
}

public static class GetRoomEndpoint
{
    public static void MapGetRoom(this RouteGroupBuilder group) =>
        group.MapGet("/{id:guid}", async Task<Results<Ok<RoomResponse>, ProblemHttpResult>> (
            Guid id,
            IQueryHandler<GetRoomQuery, RoomResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetRoomQuery(id), ct);
            return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error!.ToProblem();
        });
}
