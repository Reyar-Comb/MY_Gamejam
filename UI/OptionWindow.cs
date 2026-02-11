using Godot;
using System;

public partial class OptionWindow : Node2D
{
	[Export] public HSlider BGMSlider;
	[Export] public HSlider SFXSlider;
	[Export] public Button DoneButton;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BGMSlider.Value = AudioManager.Instance.GetBGMVolumePercent();
		SFXSlider.Value = AudioManager.Instance.GetSFXVolumePercent();
		BGMSlider.ValueChanged += (value) =>
		{
			AudioManager.Instance.SetBGMVolumePercent((float)value);
		};
		SFXSlider.ValueChanged += (value) =>
		{
			AudioManager.Instance.SetSFXVolumePercent((float)value);
		};
		DoneButton.Pressed += () =>
		{
			Visible = false;
			GameManager.Instance.PauseUI.Visible = true;
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent)
		{
			if (GetTree().Paused && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
			{
				Visible = false;
				GameManager.Instance.PauseUI.Visible = true;
			}
		}
	}

}
