using Godot;
using System;

public partial class CollectibleDots : Area2D
{
	[Export] public int DotValue = 1;
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}
	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			DotlineManager.Instance.MaxHistoryDots += DotValue;
		}
	}
}
