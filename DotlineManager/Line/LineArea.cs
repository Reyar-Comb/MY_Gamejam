using Godot;
using System;

public partial class LineArea : Area2D
{
	public DotlineColor LineColor => Line.Color;
	public Line Line => field ??= Owner as Line;
}
