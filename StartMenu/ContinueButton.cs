using Godot;
using System;

public partial class ContinueButton : Button
{
	public override void _Pressed()
	{
		GetTree().ChangeSceneToFile("res://Map/Map.tscn");
		GameManager.Instance.ContinueGame();
	}
}
