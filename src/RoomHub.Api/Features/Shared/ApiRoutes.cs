using RoomHub.Api.Features.Bookings.CancelBooking;
using RoomHub.Api.Features.Bookings.CreateBooking;
using RoomHub.Api.Features.Bookings.GetBooking;
using RoomHub.Api.Features.Rooms.CreateRoom;
using RoomHub.Api.Features.Rooms.GetRoom;

namespace RoomHub.Api.Features.Shared;

public static class ApiRoutes
{
    public static void MapFeatures(this IEndpointRouteBuilder app)
    {
        var rooms = app.MapGroup("/api/rooms").WithTags("Rooms");
        rooms.MapCreateRoom();
        rooms.MapGetRoom();
        rooms.MapCreateBooking();

        var bookings = app.MapGroup("/api/bookings").WithTags("Bookings");
        bookings.MapGetBooking();
        bookings.MapCancelBooking();
    }
}
