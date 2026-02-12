using Godot;
using System;

public partial class StarryRoom : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			if (AudioManager.Instance != null && AudioManager.Instance.FindBGM("guitar"))
			{
				await AudioManager.Instance.ChangeBGM("guitar", 0.5f);
			}
			else
			{
				return;
			}
		}
	}

	public async void OnBodyExited(Node2D body)
	{
		if (body is Player)
		{
			if (AudioManager.Instance.GetPlayingBGM() == "guitar")
			{
				await AudioManager.Instance.ChangeBGM("Dots' Lullaby", 1f);
			}
			else
			{
				return;
			}
		}
	}
}
