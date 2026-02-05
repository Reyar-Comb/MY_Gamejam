using Godot;
using System;

public partial class LineArea : Area2D
{
	public DotlineColor LineColor => (Owner as Line).Color;
}
