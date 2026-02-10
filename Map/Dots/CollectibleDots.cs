using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class CollectibleDots : Area2D
{
	[Export] public int CollectibleDotsID
	{
		get
		{
			return UseSuffixAsID
			? int.Parse(Regex.Match(Name, @"\d+$").Value)
			: field;
		}
		set => field = value;
	} = 0;
	[Export] public int DotValue = 1;
	[Export] public bool UseSuffixAsID = false;
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		GameManager.Instance.Connect("LoadCollectedDot", new Callable(this, "OnLoadCollectedDot"));
	}
	private void OnBodyEntered(Node2D body)
	{
		if (body is Player)
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
