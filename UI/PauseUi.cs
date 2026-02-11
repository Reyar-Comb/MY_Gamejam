using Godot;
using System;

public partial class PauseUi : Node2D
{
	[Export] public Button ContinueButton;
	[Export] public Button OptionsButton;
	[Export] public Button QuitButton;
	[Export] public Node2D OptionsWindow;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetProcessInput(true);
		GameManager.Instance.PauseUI = this;
		ContinueButton.Pressed += () =>
		{
			Visible = false;
			GameManager.Instance.TogglePauseGame();
		};
		OptionsButton.Pressed += () =>
		{
			Visible = false;
			OptionsWindow.Visible = true;
		};
		QuitButton.Pressed += () =>
		{
			GameManager.Instance.TogglePauseGame();
			GetTree().ChangeSceneToFile("res://StartMenu/StartUI.tscn");
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	

}
