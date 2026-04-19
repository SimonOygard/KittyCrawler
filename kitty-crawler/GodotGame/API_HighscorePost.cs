using Godot;
using System;

public partial class API_HighscorePost : Node
{

    private string url = "https://your-api-endpoint.com/highscores"; // Erstatt med din API-endepunkt

    public Godot.Collections.Dictionary dataToSend { get; set; }

	public override void _Ready()
	{
        string json = Json.Stringify(dataToSend);
        string[] headers = ["Content-Type: application/json"];
        HttpRequest httpRequest = GetNode<HttpRequest>("HTTPRequest");
        httpRequest.Request(url, headers, HttpClient.Method.Post, json);
    }

    private void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode == 200)
        {
            GD.Print("Highscore posted successfully!");
        }
        else
        {
            GD.Print("Failed to post highscore. Response code: " + responseCode);
        }
    }
}
