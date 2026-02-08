using Godot;
using System;

public partial class testAudioScene : Node2D
{
	public override void _Ready()
	{
		AudioManager.Instance.test();
		AudioManager.Instance.LoadTracks();
		AudioManager.Instance.PlayBGM("Dots' Lullaby");
	}
}
