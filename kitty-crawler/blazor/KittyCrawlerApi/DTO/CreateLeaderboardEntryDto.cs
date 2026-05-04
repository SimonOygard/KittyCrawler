namespace KittyCrawlerApi.Dtos;

public class CreateLeaderboardEntryDto
{
    public string Username { get; set; } = string.Empty;
    public int TimeSeconds { get; set; }
    public int Score { get; set; }
}