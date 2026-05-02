using Godot;
using KittyCrawler.TELT;
using System;
using System.Threading.Tasks;

public partial class LevelTransition : Node
{
    [Export] public string ScenePath { get; set; } = string.Empty;
    [Export] public TransitionType Type { get; set; } = TransitionType.Battle;
    [Export] public string ObjectName { get; set; } = "Skeleton";

    public async void TriggerTransition()
    {
        await SceneManager.Instance.ChangeSceneAsync(ScenePath, Type, ObjectName);
    }
}

// Enum to define different types of transitions, allowing for specific logic based on the transition type
public enum TransitionType
{
    Battle,
    Puzzle,
    Trap,
    Dialogue,
    Teleport,
    Generic
}
