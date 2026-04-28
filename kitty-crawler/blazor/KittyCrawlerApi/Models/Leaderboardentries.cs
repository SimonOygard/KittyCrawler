namespace KittyCrawlerApi.Models;

public class LeaderboardEntry
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int TimeSeconds { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
}