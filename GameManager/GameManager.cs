using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public partial class GameManager : Node
{
	public static GameManager Instance;

	public int CurrentCheckPointID = 0;
	public int CurrentMaxPoints => DotlineManager.Instance.MaxHistoryDots;
	public Player Player;
	public List<CheckPoint> CheckPoints = new();

	public string SaveFilePath = "user://savedata.dat";
	[Signal] public delegate void CheckPointChangedEventHandler(int checkPointID);

	

	public override void _Ready()
	{
		Instance = this;
	}
	public void GetAllCheckPoints()
	{
		CheckPoints.Clear();
		var nodes = GetTree().GetNodesInGroup("CheckPoints");
		foreach (var node in nodes)
		{
			if (node is CheckPoint checkPoint)
			{
				GD.Print($"GameManager: Found CheckPoint ID {checkPoint.CheckPointID}");
				CheckPoints.Add(checkPoint);
			}
		}
	}

	public async void StartNewGame()
	{
		CurrentCheckPointID = -1;
		DotlineManager.Instance.MaxHistoryDots = 0;
		await LoadGame();
		RespawnPlayer(CurrentCheckPointID);
		SaveFile(CurrentCheckPointID);
	}
	public async void ContinueGame()
	{
		await LoadGame();
		await LoadSaveFile();
		RespawnPlayer(CurrentCheckPointID);
		SetCheckPoint(CurrentCheckPointID);
	}

	public async Task LoadGame()
	{
		await Task.Delay(5);
		GetAllCheckPoints();
	}

	public void RespawnPlayer(int checkPointID)
	{
		GD.Print($"GameManager: Respawning player at checkpoint {checkPointID}");
		foreach (var checkPoint in CheckPoints)
		{
			if (checkPoint.CheckPointID == checkPointID)
			{
				Player.GlobalPosition = checkPoint.GlobalPosition;
				return;
			}
		}
		GD.PrintErr($"GameManager: Checkpoint {checkPointID} not found!");
	}

	public void SetCheckPoint(int checkPointID)
	{
		CurrentCheckPointID = checkPointID;
		GD.Print($"GameManager: Checkpoint set to {CurrentCheckPointID}");
		SaveFile(checkPointID);
		EmitSignal("CheckPointChanged", checkPointID);
	}

	public void SaveFile(int checkPointID)
	{
		SaveData saveData = new SaveData
		{
			CheckPointID = checkPointID,
			MaxPoints = CurrentMaxPoints
		};

		var file = Godot.FileAccess.Open(SaveFilePath, Godot.FileAccess.ModeFlags.Write);
		if (file == null)
		{
			GD.PrintErr("GameManager: Failed to open save file for writing.");
			return;
		}

		var json = JsonSerializer.Serialize(saveData);
		file.StoreString(json);
		file.Close();
		GD.Print("GameManager: Game saved successfully.");
	}
	public async Task LoadSaveFile()
	{
		var file = Godot.FileAccess.Open(SaveFilePath, Godot.FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr("GameManager: Failed to open save file for reading.");
			return;
		}

		var json = file.GetAsText();
		file.Close();

		try
		{
			SaveData saveData = JsonSerializer.Deserialize<SaveData>(json);
			if (saveData != null)
			{
				CurrentCheckPointID = saveData.CheckPointID;
				DotlineManager.Instance.MaxHistoryDots = saveData.MaxPoints;
				GD.Print($"GameManager: Loaded checkpoint ID {CurrentCheckPointID} from save file.");
			}
			else
			{
				GD.PrintErr("GameManager: Save data is null after deserialization.");
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"GameManager: Failed to deserialize save data. Exception: {e.Message}");
		}
	}

	public async Task ReturnToLastCheckpoint()
	{
		EmitSignal("CheckPointChanged", CurrentCheckPointID);
		RespawnPlayer(CurrentCheckPointID);
	}

	public override async void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent)
		{
			if (keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
			{
				GetTree().ChangeSceneToFile("res://StartMenu/StartMenu.tscn");
			}
		}
	}

}
