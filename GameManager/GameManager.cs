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

	public List<int> CollectedDotsIDs = new();

	public string SaveFilePath = "user://savedata.dat";
	public bool IsLoading = false;
	[Signal] public delegate void CheckPointChangedEventHandler(int checkPointID);
	[Signal] public delegate void LoadCollectedDotEventHandler(Godot.Collections.Array<int> collectedDotsIDs);


	private async Task WaitUntilCurrentSceneReady()
	{
        var oldScene = GetTree().CurrentScene;

        while (GetTree().CurrentScene == oldScene || GetTree().CurrentScene == null)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        var newScene = GetTree().CurrentScene;

        if (!newScene.IsNodeReady())
        {
            GD.Print("GameManager: Waiting for new scene to be ready...");
            await ToSignal(newScene, Node.SignalName.Ready);
        }
        
        GD.Print("GameManager: Current scene is ready!");
	}
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
		IsLoading = true;
		CurrentCheckPointID = -1;
		DotlineManager.Instance.MaxHistoryDots = 0;
		CollectedDotsIDs.Clear();
		await LoadGame();
		RespawnPlayer(CurrentCheckPointID);
		SaveFile(CurrentCheckPointID);
		IsLoading = false;
	}
	public async void ContinueGame()
	{
		IsLoading = true;
		await LoadGame();
		await LoadSaveFile();
		RespawnPlayer(CurrentCheckPointID);
		SetCheckPoint(CurrentCheckPointID);
		IsLoading = false;
	}

	public async Task LoadGame()
	{
		await WaitUntilCurrentSceneReady();
		GetAllCheckPoints();
	}

	public void RespawnPlayer(int checkPointID)
	{
		Player.PlayerColor = "White";
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
		Player.PlayerColor = "White";
		SaveFile(checkPointID);
		EmitSignal("CheckPointChanged", checkPointID);
	}

	public void CollectDot(int dotID)
	{
		if (!CollectedDotsIDs.Contains(dotID))
		{
			CollectedDotsIDs.Add(dotID);
			GD.Print($"GameManager: Collected dot ID {dotID}");
		}
		else
		{
			GD.Print($"GameManager: Dot ID {dotID} already collected");
		}
	}

	public void SaveFile(int checkPointID)
	{
		int maxPoints = 0;
		if (DotlineManager.Instance != null && IsInstanceValid(DotlineManager.Instance))
		{
			maxPoints = DotlineManager.Instance.MaxHistoryDots;
		}

		SaveData saveData = new SaveData
		{
			CheckPointID = checkPointID,
			MaxPoints = maxPoints,
			CollectedDotsID = CollectedDotsIDs
		};

		var file = Godot.FileAccess.Open(SaveFilePath, Godot.FileAccess.ModeFlags.Write);
		if (file == null)
		{
			GD.PrintErr($"GameManager: Failed to open save file for writing at {SaveFilePath}. Error: {Godot.FileAccess.GetOpenError()}");
			return;
		}

		var json = JsonSerializer.Serialize(saveData);
		file.StoreString(json);
		file.Close();
		GD.Print($"GameManager: Game saved successfully. CheckPoint: {checkPointID}, MaxPoints: {maxPoints}");
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
		GD.Print(json);
		try
		{
			SaveData saveData = JsonSerializer.Deserialize<SaveData>(json);
			if (saveData != null)
			{
				CurrentCheckPointID = saveData.CheckPointID;
				DotlineManager.Instance.MaxHistoryDots = saveData.MaxPoints;
				CollectedDotsIDs = saveData.CollectedDotsID ?? new List<int>();
				EmitSignal("LoadCollectedDot", new Godot.Collections.Array<int>(CollectedDotsIDs));
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
				GetTree().ChangeSceneToFile("res://StartMenu/StartUI.tscn");
			}
		}
	}

}
