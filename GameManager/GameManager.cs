using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	public static GameManager Instance;

	public int CurrentCheckPointID = 0;
	public List<int> DestroyedWalls = new();
	public override void _Ready()
	{
		Instance = this;
	}

	public void SetCheckPoint(int checkPointID)
	{
		CurrentCheckPointID = checkPointID;
		GD.Print($"GameManager: Checkpoint set to {CurrentCheckPointID}");
		// TODO: Write checkpoint data to file
	}

	public void DestroyWall(int wallID)
	{
		if (!DestroyedWalls.Contains(wallID))
		{
			DestroyedWalls.Add(wallID);
			GD.Print($"GameManager: Wall {wallID} destroyed.");
		}
		// TODO: Write destroyed walls data to file
	}
}
