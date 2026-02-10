using Godot;
using System;
using System.Collections.Generic;


public partial class AudioManager : Node2D
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
		LoadTracks();

		PlayBGM("Dots' Lullaby");
	}

	public void test()
	{
		GD.Print("AudioManager is working!");
		SetBGMVolumePercent(1f);
	}
	private AudioStreamPlayer AddSFXPlayerToPool()
	{
		AudioStreamPlayer sfxPlayer = new AudioStreamPlayer()
		{
			Bus = "SFX"
		};
		AddChild(sfxPlayer);
		SFXPlayers.Add(sfxPlayer);
		return sfxPlayer;
	}
	public void LoadTracks()
	{
		AddChild(BGMPlayer = new AudioStreamPlayer());
		BGMPlayer.Bus = "BGM";
		foreach (var pair in BGMTracks)
		{
			GD.Print("Loaded BGM Track: " + pair.Key);
		}

		for (int i = 0; i < MaxSFXStreams; i++)
		{
			AddSFXPlayerToPool();
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
	private AudioStreamPlayer GetAvailableSFXPlayer(string streamNameToPlay)
	{
		foreach (var player in SFXPlayers)
		{
			if (!player.Playing)
			{
				return player;
			}
		}
		GD.PushWarning("SFX Track not found or no available SFX player for: " + streamNameToPlay);
		return AddSFXPlayerToPool();
	}
	private AudioStream GetStreamByKey(string key)
	{
		foreach (var pair in SFXTracks)
		{
			if (pair.Key == key)
			{
				return pair.Value;
			}
		}
		GD.PushError("SFX Track not found: " + key);
		return null;
	}
	public void PlaySFX(string key)
	{
		AudioStream streamToPlay = GetStreamByKey(key);
		if (streamToPlay == null) return;

		AudioStreamPlayer sfxPlayer = GetAvailableSFXPlayer(key);
		if (sfxPlayer == null) return;

		sfxPlayer.Stream = streamToPlay;
		sfxPlayer.Play();
		GD.Print("Playing SFX Track: " + key);
	}

	public void PlayOnceSFX(string key)
	{
		foreach (var pair in SFXTracks)
		{
			if (pair.Key == key)
			{
				foreach (var player in SFXPlayers)
				{
					if (player.Stream == pair.Value && player.Playing)
					{
						// Already playing
						return;
					}
				}
				PlaySFX(key);
				return;
			}
		}
	}

	public void StopOnceSFX(string key)
	{
		foreach (var pair in SFXTracks)
		{
			if (pair.Key == key)
			{
				foreach (var player in SFXPlayers)
				{
					if (player.Stream == pair.Value && player.Playing)
					{
						player.Stop();
						GD.Print("Stopped SFX Track: " + pair.Key);
						return;
					}
				}
			}
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
