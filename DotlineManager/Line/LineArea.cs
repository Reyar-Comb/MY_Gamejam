using Godot;
using System;
using System.Linq;

public partial class LineArea : Area2D
{
	public DotlineColor LineColor => Line.Color;
	public Line Line => field ??= Owner as Line;
	public override void _Ready()
	{
		//GD.Print($"LineArea: Spawned {LineColor} line.");
		if (LineColor == DotlineColor.Red)
			Line.LineSpawned += OnLineSpawned;
	}
	private async void OnLineSpawned()
	{
		const int waitFrames = 2;
		foreach (var i in Enumerable.Range(0, waitFrames))
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		foreach (var body in GetOverlappingBodies())
		{
			if (body is not DestroyableWall wall) continue;

			if (IsOverlappingWithOppositeSide(wall))
				wall.Destroy();
		}
	}
	private bool IsOverlappingWithOppositeSide(DestroyableWall wall)
	{
		bool overlapsLeftAndRight = OverlapsArea(wall.LeftSide) && OverlapsArea(wall.RightSide);
		bool overlapsTopAndBottom = OverlapsArea(wall.TopSide) && OverlapsArea(wall.BottomSide);
		return overlapsLeftAndRight || overlapsTopAndBottom;
	}
}
