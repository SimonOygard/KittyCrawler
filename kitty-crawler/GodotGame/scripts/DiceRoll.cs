using Godot;
using System;

public partial class DiceRoll : Node
{
    public static DiceRoll Instance { get; private set; }

    private RandomNumberGenerator rng = new RandomNumberGenerator();
    public override void _Ready()
	{
        Instance = this;
        rng.Randomize();
    }

    public int RollDice(int sides)
    {
        return rng.RandiRange(1, sides);
    }

    // når klasser/items har modifiers kan denne brukes for å rulle en dice og legge til modifikatoren 
    public int DiceModifier(int sides, int modifier)
    {
        return RollDice(sides) + modifier;
    }
}
