using Godot;

public partial class GameTimerManager : Node
{
    public int TotalSeconds { get; private set; } = 0;

    private Godot.Timer _timer;
    private WorldStateManager _worldState;

    public override void _Ready()
    {
        _worldState = GetNode<WorldStateManager>("/root/WorldStateManager");

        _timer = new Godot.Timer();
        _timer.WaitTime = 1.0;
        _timer.OneShot = false;
        AddChild(_timer);

        _timer.Timeout += OnTimerTimeout;

        TotalSeconds = (int)_worldState.TimeSeconds;

        GD.Print($"[Timer] Initial time loaded: {TotalSeconds}");
    }

    private void OnTimerTimeout()
    {
        TotalSeconds++;
        _worldState.TimeSeconds = TotalSeconds;
    }

    public void StartTimer()
    {
        TotalSeconds = 0;
        _worldState.TimeSeconds = 0;
        _timer.Start();

        GD.Print("[Timer] Timer started at 00:00");
    }

    public void ContinueTimer()
    {
        TotalSeconds = (int)_worldState.TimeSeconds;
        _timer.Paused = false;
        _timer.Start();

        GD.Print("[Timer] Timer continued at " + GetFormattedTime());
    }

    public void PauseTimer()
    {
        _timer.Paused = true;
        _worldState.TimeSeconds = TotalSeconds;
        _worldState.SaveGame();

        GD.Print("[Timer] Timer paused at " + GetFormattedTime());
    }

    public void ResumeTimer()
    {
        _timer.Paused = false;
        GD.Print("Timer resumed at " + GetFormattedTime());
    }

    public void StopTimer()
    {
        _timer.Stop();
        _timer.Paused = false;

        TotalSeconds = 0;
        _worldState.TimeSeconds = 0;
        _worldState.SaveGame();
        GD.Print("[Timer] Timer stopped at" + GetFormattedTime());
    }

    public string GetFormattedTime()
    {
        int minutes = TotalSeconds / 60;
        int seconds = TotalSeconds % 60;

        return minutes.ToString("D2") + ":" + seconds.ToString("D2");
    }
}
