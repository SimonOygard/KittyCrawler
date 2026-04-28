using KittyCrawlerApi.Data;
using KittyCrawlerApi.Dtos;
using KittyCrawlerApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/leaderboard", async (AppDbContext db) =>
{
    var entries = await db.LeaderboardEntries
        .OrderByDescending(e => e.Score)
        .ThenBy(e => e.TimeSeconds)
        .Take(20)
        .ToListAsync();

    return Results.Ok(entries);
});

app.MapPost("/api/leaderboard", async (CreateLeaderboardEntryDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Username))
        return Results.BadRequest("Username is required.");

    if (dto.Username.Length > 50)
        return Results.BadRequest("Username must be 50 characters or fewer.");

    if (dto.TimeSeconds < 0)
        return Results.BadRequest("TimeSeconds must be 0 or greater.");

    if (dto.Score < 0)
        return Results.BadRequest("Score must be 0 or greater.");

    var entry = new LeaderboardEntry
    {
        Username = dto.Username.Trim(),
        TimeSeconds = dto.TimeSeconds,
        Score = dto.Score,
        CreatedAt = DateTime.UtcNow
    };

    db.LeaderboardEntries.Add(entry);
    await db.SaveChangesAsync();

    return Results.Ok(entry);
});

app.Run();