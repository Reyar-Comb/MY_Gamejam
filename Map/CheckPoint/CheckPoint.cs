using Godot;
using System;

public partial class CheckPoint : AnimatedSprite2D
{
	[Export] public int CheckPointID = 0;
	[Export] public Area2D CheckPointArea;

	private bool _isEntered = false;

	public override void _Ready()
	{
		CheckPointArea.BodyEntered += OnBodyEntered;
		CheckPointArea.BodyExited += OnBodyExited;
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			GD.Print($"Checkpoint {CheckPointID} reached by Player.");
			_isEntered = true;
		}
	}

	public void OnBodyExited(Node2D body)
	{
		if (body is Player player)
		{
			GD.Print($"Player exited Checkpoint {CheckPointID}.");
			_isEntered = false;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (_isEntered && @event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F)
		{
			GD.Print($"Checkpoint {CheckPointID} activated by Player input.");
			GameManager.Instance.SetCheckPoint(CheckPointID);
		}
	}
}
