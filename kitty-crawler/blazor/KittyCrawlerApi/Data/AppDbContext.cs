using KittyCrawlerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KittyCrawlerApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaderboardEntry>(entity =>
        {
            entity.ToTable("leaderboardentries");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.TimeSeconds).HasColumnName("timeseconds");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
        });
    }
}