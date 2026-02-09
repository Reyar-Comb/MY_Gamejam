using Godot;
using System;
using System.Collections.Frozen;

[Tool]
public partial class ColorBlock : StaticBody2D
{
	private enum BlockColor
	{
		Blue,
		Purple,
		Red
	}
	[Export] private BlockColor Color 
	{
		get => field;
		set
		{
			field = value;
			string colorName = value.ToString();
			BlockSprite.Texture = ResourceLoader.Load<Texture2D>($"res://Map/ColorBlock/{colorName}ColorBlock.png");
		}
	} = BlockColor.Blue;
	private Player Player => field ??= GetTree().GetFirstNodeInGroup("Player") as Player;
	private Sprite2D BlockSprite => field ??= GetNode<Sprite2D>("BlockSprite");
    public override void _Ready()
    {
        Color = Color;
    }
	public DotlineColor GetColor()
	{
		return Color switch
		{
			BlockColor.Blue => DotlineColor.Blue,
			BlockColor.Purple => DotlineColor.Purple,
			BlockColor.Red => DotlineColor.Red,
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}
