using Godot;
using System;
using System.Threading.Tasks;

public partial class StartButton : Button
{
	public override async void _Pressed()
	{
		await TransitionLayer.Instance.Appear();
		GetTree().ChangeSceneToFile("res://Map/Map.tscn");
		GameManager.Instance.StartNewGame();
		await TransitionLayer.Instance.Disappear();
	}
}
