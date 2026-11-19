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
    public static readonly Error TooLong = Error.Validation("TooLong", "A booking can last at most 4 hours.");
    public static readonly Error TooLongForLargeRoom = Error.Validation("TooLongForLargeRoom", "Rooms with more than 10 seats can be booked for at most 2 hours.");
    public static readonly Error RoomOccupied = Error.Conflict("RoomOccupied", "The room is occupied in this interval.");
    public static readonly Error ConcurrentBooking = Error.Conflict("ConcurrentBooking", "The room was booked by another request at the same time. Try again.");
    public static readonly Error CancelTooLate = Error.Conflict("CancelTooLate", "A booking can be cancelled only if it starts at least 60 minutes from now.");
    public static readonly Error AlreadyCancelled = Error.Conflict("AlreadyCancelled", "The booking is already cancelled.");
}
