using RoomHub.Api.Domain;
using RoomHub.Api.Features.Shared;
using RoomHub.Api.Infrastructure.InMemory;

var builder = WebApplication.CreateBuilder(args);

// Temporary in-memory persistence (singletons so data lives as long as the process).
builder.Services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHandlers();

var app = builder.Build();

app.MapGet("/", () => "RoomHub API is running.");
app.MapFeatures();

app.Run();

public partial class Program;
