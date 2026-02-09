using Godot;
using System;
using System.Threading.Tasks;

public partial class StartButton : Button
{
	public override void _Pressed()
	{
		GetTree().ChangeSceneToFile("res://Map/Map.tscn");
		GameManager.Instance.StartNewGame();
	}
}
