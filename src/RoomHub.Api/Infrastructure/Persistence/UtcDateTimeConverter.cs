using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RoomHub.Api.Infrastructure.Persistence;

public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    toDb => toDb.Kind == DateTimeKind.Local ? toDb.ToUniversalTime() : DateTime.SpecifyKind(toDb, DateTimeKind.Utc),
    fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));
