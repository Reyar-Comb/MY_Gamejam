using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class AudioManager : Node
{
	public static AudioManager Instance;

	[Export] public Godot.Collections.Array<AudioKeyValuePair> BGMTracks = new();
	[Export] public Godot.Collections.Array<AudioKeyValuePair> SFXTracks = new();

	[Export] public int MaxSFXStreams = 10;
	[Export] public float DefaultBGMVolumeDb = -10f;
	[Export] public float DefaultSFXVolumeDb = -5f;
	[Export] public float DefaultMinDb = -80f;

	public AudioStreamPlayer BGMPlayer;
	public List<AudioStreamPlayer> SFXPlayers = new();


	public override void _Ready()
	{
		Instance = this;
		if (Instance == null)
		{
			GD.PrintErr("AudioManager instance is null!");
		}
	}

	public void test()
	{
		GD.Print("AudioManager is working!");
		SetBGMVolumePercent(1f);
	}

	public void LoadTracks()
	{
		AddChild(BGMPlayer = new AudioStreamPlayer());
		BGMPlayer.Bus = "BGM";
		foreach (var pair in BGMTracks)
		{
			GD.Print("Loaded BGM Track: " + pair.Key);

		}
	}
	
	public void PlayBGM(string key)
	{
		bool found = false;
		foreach (var pair in BGMTracks)
		{
			if (pair.Key == key)
			{
				BGMPlayer.Stream = pair.Value;
				BGMPlayer.Play();
				GD.Print("Playing BGM Track: " + pair.Key);
				found = true;
				return;
			}
		}
		if (!found)
		{
			GD.PrintErr("BGM Track not found: " + key);
		}
	}
	public void SetSFXVolumePercent(float percent)
	{
		float db = PercentToDb(percent, DefaultMinDb, DefaultSFXVolumeDb);
		GD.Print("Setting SFX volume to " + db + " dB");
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), db);
	}

	public void SetBGMVolumePercent(float percent)
	{
		float db = PercentToDb(percent, DefaultMinDb, DefaultBGMVolumeDb);
		GD.Print("Setting BGM volume to " + db + " dB");
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("BGM"), db);
	}


	private float PercentToDb(float percent, float minDb = -80f, float maxDb = 0f)
	{
		float maxLinear = Mathf.DbToLinear(maxDb);
		float minLinear = Mathf.DbToLinear(minDb);
		GD.Print("maxLinear: " + maxLinear + " minLinear: " + minLinear);
		return Mathf.LinearToDb(minLinear + (maxLinear - minLinear) * percent);
	}
}
