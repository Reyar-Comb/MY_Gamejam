using Godot;
using System;

public partial class CheckPoint : AnimatedSprite2D
{
	[Export] public int CheckPointID = 0;
	[Export] public Area2D CheckPointArea;
	[Export] public AnimationPlayer AnimationPlayer;



	public override void _Ready()
	{
		AddToGroup("CheckPoints");
		CheckPointArea.BodyEntered += OnBodyEntered;
		
		GameManager.Instance.Connect("CheckPointChanged", new Callable(this, "OnCheckPointReached"));
		AnimationPlayer.Play("CheckPoint_OFF");
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			GD.Print($"Checkpoint {CheckPointID} reached by Player.");
			if (GameManager.Instance.CurrentCheckPointID != CheckPointID)
			{
				GameManager.Instance.SetCheckPoint(CheckPointID);
			}
		}
	}

	public void OnCheckPointReached(int checkPointID)
	{
		if (checkPointID == CheckPointID)
		{
			AnimationPlayer.Play("CheckPoint_ON");
			AudioManager.Instance.PlaySFX("checkpoint");
		}
		else
		{
			AnimationPlayer.Play("CheckPoint_OFF");
		}
	}




}
