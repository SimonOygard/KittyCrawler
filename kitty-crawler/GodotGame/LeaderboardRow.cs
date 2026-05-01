using Godot;
using System;

public partial class LeaderboardRow : PanelContainer
{
    [Export] public Label RankLabel;
    [Export] public Label NameLabel;
    [Export] public Label ScoreLabel;
    [Export] public Label TimeLabel;

    public void Setup(string rank, string name, string score, string time, bool isHeader = false)
    {
        RankLabel.Text = rank;
        NameLabel.Text = name;
        ScoreLabel.Text = score;
        TimeLabel.Text = time;

        if (isHeader)
        {
            RankLabel.AddThemeFontSizeOverride("font_size", 24);
            NameLabel.AddThemeFontSizeOverride("font_size", 24);
            ScoreLabel.AddThemeFontSizeOverride("font_size", 24);
            TimeLabel.AddThemeFontSizeOverride("font_size", 24);
        }
    }
}
