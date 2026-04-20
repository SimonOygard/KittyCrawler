using Godot;
using System;
using Godot.Collections;


public partial class Leaderboard : Node
{
    private bool _name = false;
    private Button _button;
    private HttpRequest httpRequest;

    public override void _Ready()
	{
        // connect to API
        // _button = GetNode<Button>("SubmitButton").Pressed += _OnSubmitPressed;


        GetNode<HttpRequest>("LeaderboardFetch").RequestCompleted += _OnLeaderboardRequest;
        GetNode<HttpRequest>("LeaderboardPost").RequestCompleted += _OnPostRequest;
    }

    // Fetch the top 10 from db
    private void _OnLeaderboardRequest(long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode != 200)
        {
            GD.Print("Failed to fetch leaderboard. Response code: " + responseCode);
        }
        else
        {
            Godot.Collections.Dictionary json = Json.ParseString(System.Text.Encoding.UTF8.GetString(body)).AsGodotDictionary();
            GD.Print(json["name"]);
            // Display the leaderboard in the game
        }
    }

    // Post name + score + time to db
    private void _OnPostRequest(long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode != 200)
        {
            GD.Print("Failed to post score. Response code: " + responseCode);
        }
        else
        {
            GD.Print("Score posted successfully!");
            // Optionally, refresh the leaderboard after posting a new score
            GetNode<HttpRequest>("LeaderboardFetch").Request("https://jsonplaceholder.typicode.com/posts/1");
        }
    }

    private void _OnSubmitPressed()
    {
        return;
    }
}
