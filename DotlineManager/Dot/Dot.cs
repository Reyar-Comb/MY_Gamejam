using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public partial class Dot : RigidBody2D
{
	public DotlineColor Color { get; set; } = DotlineColor.White;
	public DotState State { get; set; } = DotState.Idle;
	[Export] public Sprite2D DotSprite;

	[Export] public Texture2D BlueSprite;
	[Export] public Texture2D RedSprite;
	[Export] public Texture2D PurpleSprite;
	[Export] public PackedScene LineScene;

	[Signal] public delegate void DotCollideEventHandler(Dot collidedDot, Dot selfDot);

	public Tween Tween;
	public List<Line> CurrentLines = new List<Line>();
	public Line CurrentLine;
	
	public void SetColor(DotlineColor color)
	{
		Color = color;
		UpdateSprite();
	}

	public void SetVelocity(Vector2 velocity)
	{
		GD.Print("Setting velocity: " + velocity);
		LinearVelocity = velocity;
	}

	public Line LineUp(Dot targetDot)
	{
		Line line = (Line)LineScene.Instantiate();
		line.SetColor(Color);
		line.StartDot = this;
		line.EndDot = targetDot;
		GetTree().CurrentScene.AddChild(line); 
		CurrentLines.Add(line);
		targetDot.CurrentLine = line;
		
		line.Spawn(); 
		return line;
	}
	public void Spawn()
	{
		Tween = CreateTween()
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
		Scale = new Vector2(0f, 0f);
		Tween.TweenProperty(this, "scale", new Vector2(1f, 1f), 0.2f);
	}

	public async Task Clear()
	{
		Tween = CreateTween()
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
		Tween.TweenProperty(this, "scale", new Vector2(0f, 0f), 0.2f);
		await ToSignal(Tween, "finished");
		CurrentLines.Clear();
		QueueFree();
	}
	public override void _Ready()
	{
		BodyEntered += (body) =>
		{
			if (body is Dot dot && dot != this)
			{
				// TODO: Handle dot collision logic here
				// EmitSignal(SignalName.DotCollide, dot, this);
			}
			
		};
		UpdateSprite();
	}
	public void UpdateSprite()
	{
		DotSprite.Texture = Color switch
		{
			DotlineColor.Blue => BlueSprite,
			DotlineColor.Red => RedSprite,
			DotlineColor.Purple => PurpleSprite,
			DotlineColor.White => BlueSprite,
			_ => BlueSprite
		};
	}
	
}
