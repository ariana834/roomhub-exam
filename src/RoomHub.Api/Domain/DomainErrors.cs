using RoomHub.Api.Features.Shared;

namespace RoomHub.Api.Domain;

public static class RoomErrors
{
    public static readonly Error NotFound = Error.NotFound("RoomNotFound", "The room does not exist.");
    public static readonly Error InvalidName = Error.Validation("InvalidName", "Room name is required.");
    public static readonly Error InvalidCapacity = Error.Validation("InvalidCapacity", "Capacity must be positive.");
}

public static class BookingErrors
{
    public static readonly Error NotFound = Error.NotFound("BookingNotFound", "The booking does not exist.");
    public static readonly Error InvalidBookedBy = Error.Validation("InvalidBookedBy", "BookedBy is required.");
    public static readonly Error InvalidRange = Error.Validation("InvalidRange", "The end must be after the start.");
    public static readonly Error InvalidAttendees = Error.Validation("InvalidAttendees", "Attendees must be positive.");
    public static readonly Error AttendeesExceedCapacity = Error.Validation("AttendeesExceedCapacity", "The room does not have enough seats.");
    public static readonly Error StartInPast = Error.Validation("StartInPast", "A booking cannot start in the past.");
    public static readonly Error RoomOccupied = Error.Conflict("RoomOccupied", "The room is occupied in this interval.");
}
