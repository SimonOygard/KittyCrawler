using Godot;
using System.Text;

public partial class HighscoreFetch : Node
{
    public override void _Ready()
    {
        HttpRequest httpRequest = GetNode<HttpRequest>("HTTPRequest");
        httpRequest.RequestCompleted += OnRequestCompleted;
        // link til API
        httpRequest.Request("https://jsonplaceholder.typicode.com/posts/1");
    }

    private void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body) //fiks parametere
    {
        if (responseCode != 200)
        {
            GD.Print("Failed to fetch highscore. Response code: " + responseCode);
        }
        else
        {
            Godot.Collections.Dictionary json = Json.ParseString(Encoding.UTF8.GetString(body)).AsGodotDictionary();
            GD.Print(json["name"]);
        }
    }
}
