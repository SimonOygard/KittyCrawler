using Godot;
using System;
using System.Threading.Tasks;

public partial class FadeTransition : Node
{
    public static FadeTransition Instance { get; private set; }

    private ColorRect _fadeRect;

    public override void _Ready()
    {
        GD.Print("FadeTransition ready");
        Instance = this;
        _fadeRect = GetNodeOrNull<ColorRect>("CanvasLayer/FadeToBlack");

        GD.Print("Initial alpha: " + _fadeRect.Modulate.A);
        var color = _fadeRect.Modulate;
        color.A = 0.0f;
        _fadeRect.Modulate = color;
    }

    public async Task FadeToBlack(float duration = 1.0f)
    {
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "modulate:a", 1.0f, duration).SetTrans(Tween.TransitionType.Linear);

        await ToSignal(tween, Tween.SignalName.Finished);
    }

    public async Task FadeFromBlack(float duration = 1.0f)
    {
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "modulate:a", 0.0f, duration).SetTrans(Tween.TransitionType.Linear);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    public async Task FadeOutIn(float duration = 1.0f)
    {
        await FadeToBlack(duration);
        await FadeFromBlack(duration);
    }
}
