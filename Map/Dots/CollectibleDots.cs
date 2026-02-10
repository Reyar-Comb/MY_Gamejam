using Godot;
using System;

public partial class CollectibleDots : Area2D
{
	[Export] public int CollectibleDotsID = 0;
	[Export] public int DotValue = 1;
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		GameManager.Instance.Connect("LoadCollectedDot", new Callable(this, "OnLoadCollectedDot"));
	}
	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			DotlineManager.Instance.MaxHistoryDots += DotValue;
			AudioManager.Instance.PlaySFX("tools");
			GameManager.Instance.CollectDot(CollectibleDotsID);
			QueueFree();
		}
	}

	public void OnLoadCollectedDot(Godot.Collections.Array<int> collectedDotsIDs)
	{
		if (collectedDotsIDs.Contains(CollectibleDotsID))
		{
			QueueFree();
		}
	}

	
}
