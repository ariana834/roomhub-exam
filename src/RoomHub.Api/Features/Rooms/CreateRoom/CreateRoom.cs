using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Domain;
using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Features.Rooms.CreateRoom;

public sealed record CreateRoomRequest(string? Name, int Capacity);

public sealed record CreateRoomCommand(string? Name, int Capacity);

public sealed record RoomResponse(Guid Id, string Name, int Capacity);

public sealed class CreateRoomHandler(IRoomRepository rooms, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoomCommand, RoomResponse>
{
    public async Task<Result<RoomResponse>> HandleAsync(CreateRoomCommand command, CancellationToken ct)
    {
        var result = Room.Create(command.Name, command.Capacity);
        if (result.IsFailure)
            return result.Error!;

        var room = result.Value;
        rooms.Add(room);
        await unitOfWork.SaveChangesAsync(ct);

        return new RoomResponse(room.Id, room.Name, room.Capacity);
    }
}

public static class CreateRoomEndpoint
{
    public static void MapCreateRoom(this RouteGroupBuilder group) =>
        group.MapPost("/", async Task<Results<Created<RoomResponse>, ProblemHttpResult>> (
            CreateRoomRequest request,
            ICommandHandler<CreateRoomCommand, RoomResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new CreateRoomCommand(request.Name, request.Capacity), ct);
            return result.IsSuccess
                ? TypedResults.Created($"/api/rooms/{result.Value.Id}", result.Value)
                : result.Error!.ToProblem();
        });
}
