using Godot;
using System;
using Godot.Collections;
using System.Text;
using Array = Godot.Collections.Array;


public partial class LeaderboardApi : Node
{
    private bool _name = false;
    private Button _button;
    private HttpRequest httpRequest;

    [Signal]
    public delegate void ScoresFetchedRequestEventHandler(Array scores);

    public override void _Ready()
	{
        // connect to API
        var httpRequest = GetNode<HttpRequest>("LeaderboardFetch");
        httpRequest.Request("https://kittycrawler-b8e0bkhpgegyhhbd.canadacentral-01.azurewebsites.net/api/leaderboard");

        GetNode<HttpRequest>("LeaderboardFetch").RequestCompleted += OnLeaderboardRequest;
        GetNode<HttpRequest>("LeaderboardPost").RequestCompleted += OnPostRequest;
    }

    // Fetch the top 10 from db
    private void OnLeaderboardRequest(long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode != 200)
        {
            GD.Print("Failed to fetch leaderboard. Response code: " + responseCode);
        }
        else
        {
            var json = Json.ParseString(Encoding.UTF8.GetString(body)).AsGodotArray();

            EmitSignal(SignalName.ScoresFetchedRequest, json);

            foreach (var entry in json)
            {
                var entryDict = entry.AsGodotDictionary();
                GD.Print("Name: " + entryDict["username"] + ", Score: " + entryDict["score"] + ", Time: " + entryDict["timeSeconds"]);
            }

            // Display the leaderboard in the game
        }
    }

    // Post name + score + time to db
    private void OnPostRequest(long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode != 200)
        {
            GD.Print("Failed to post score. Response code: " + responseCode);
        }
        else
        {
            GD.Print("Score posted successfully!");
            // Optionally, refresh the leaderboard after posting a new score
            GetNode<HttpRequest>("LeaderboardFetch").Request("https://kittycrawler-b8e0bkhpgegyhhbd.canadacentral-01.azurewebsites.net/api/leaderboard");
        }
    }

    public void OnSubmitPressed(string username)
    {
        var worldState = GetNode<WorldStateManager>("/root/WorldStateManager");
        var httpRequest = GetNode<HttpRequest>("LeaderboardPost");

        string[] customHeaders =
        [
            "Content-Type: application/json"
        ];
        var jsonData = new Dictionary
        {
            { "username", worldState.UserName },
            { "score", worldState.Score },
            { "timeSeconds", worldState.TimeSeconds }
        };

        httpRequest.Request("https://kittycrawler-b8e0bkhpgegyhhbd.canadacentral-01.azurewebsites.net/api/leaderboard", customHeaders, HttpClient.Method.Post, Json.Stringify(jsonData));

    }
}
