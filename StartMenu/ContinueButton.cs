using Godot;
using System;

public partial class ContinueButton : Button
{
	public override async void _Pressed()
	{
		await TransitionLayer.Instance.Appear();
		GetTree().ChangeSceneToFile("res://Map/Map.tscn");
		GameManager.Instance.ContinueGame();
		await TransitionLayer.Instance.Disappear();
	}
}
