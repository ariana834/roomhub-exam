using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain;
using RoomHub.Api.Features.Shared;
using RoomHub.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// SQLite file next to the app's content root, so it does not depend on the current directory.
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "roomhub.db");
builder.Services.AddDbContext<RoomHubDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

// The DbContext is the Unit of Work (same scoped instance the repositories use).
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RoomHubDbContext>());
builder.Services.AddScoped<IRoomRepository, EfRoomRepository>();
builder.Services.AddScoped<IBookingRepository, EfBookingRepository>();

// Every error response (also invalid JSON / bad binding) is ProblemDetails (application/problem+json).
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHandlers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<RoomHubDbContext>().Database.EnsureCreated();
}

// Development throws BadHttpRequestException for invalid JSON: keep its 400 instead of a 500 / stack trace.
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = ex => ex is BadHttpRequestException bad ? bad.StatusCode : StatusCodes.Status500InternalServerError
});
// Production returns an empty 400 for invalid JSON: give it a ProblemDetails body.
app.UseStatusCodePages();

app.MapGet("/", () => "RoomHub API is running.");
app.MapFeatures();

app.Run();

public partial class Program;
