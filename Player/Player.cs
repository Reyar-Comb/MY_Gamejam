using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Signal] public delegate void ColorChangedEventHandler(string newColor);
	[Export] public StateTree StateTree;
	public Line LastTouchedLine = null;
	public string PlayerColor { 
		get => field; 
		set
		{
			field = value;
			EmitSignal(nameof(ColorChanged), value);
		} } = "White";

	public override void _Ready()
	{
		DotlineManager.Instance.Player = this;
	}
}
