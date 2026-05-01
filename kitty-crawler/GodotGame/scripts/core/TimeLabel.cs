using Godot;
using System;

public partial class TimeLabel : Label
{
    public partial class TimerLabel : Label
    {
        private GameTimerManager _timer;

        public override void _Ready()
        {
            _timer = GetNode<GameTimerManager>("/root/GameTimerManager");
        }

        public override void _Process(double delta)
        {
            Text = _timer.GetFormattedTime();
        }
    }
}
