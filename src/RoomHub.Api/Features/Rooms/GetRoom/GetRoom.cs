using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain;
using RoomHub.Api.Features.Rooms.CreateRoom;
using RoomHub.Api.Features.Shared;
using RoomHub.Api.Infrastructure.Persistence;

namespace RoomHub.Api.Features.Rooms.GetRoom;

public sealed record GetRoomQuery(Guid Id);

// Query: reads the DbContext directly, read-only (AsNoTracking), no repository / unit of work.
public sealed class GetRoomHandler(RoomHubDbContext db) : IQueryHandler<GetRoomQuery, RoomResponse>
{
    public async Task<Result<RoomResponse>> HandleAsync(GetRoomQuery query, CancellationToken ct)
    {
        var room = await db.Rooms
            .AsNoTracking()
            .Where(r => r.Id == query.Id)
            .Select(r => new RoomResponse(r.Id, r.Name, r.Capacity))
            .FirstOrDefaultAsync(ct);

        return room is null ? RoomErrors.NotFound : room;
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
