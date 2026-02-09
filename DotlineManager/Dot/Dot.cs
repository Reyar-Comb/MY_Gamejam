using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
public partial class Dot : RigidBody2D
{
	public DotlineColor Color { get; set; } = DotlineColor.White;
	public DotState State { get ; set; } = DotState.Static;
	[Export] public AnimationPlayer DotAnimPlayer;
	[Export] public float VelocityGate = 50f;
	[Export] public float StaticVelocity = 40f;
 	[Signal] public delegate void DotCollideEventHandler(Dot collidedDot, Dot selfDot);

	public bool hasEmittedLineUp = false;
	public bool hasEmittedUnline = true;
	public bool VelUnderGate => LinearVelocity.Length() < VelocityGate;
	public Tween Tween;
	public Line CurrentLine = null;
	public List<Line> CurrentLines = new List<Line>();
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

	public void Spawn()
	{
		Tween = CreateTween()
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
		Scale = new Vector2(0f, 0f);
		Tween.TweenProperty(this, "scale", new Vector2(1f, 1f), 0.2f);
	}

	public async void Clear()
	{
		Tween = CreateTween()
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
		Tween.TweenProperty(this, "scale", new Vector2(0f, 0f), 0.2f);
		await ToSignal(Tween, "finished");
		QueueFree();
	}
	public override void _Ready()
	{
		AddToGroup("Dots");
		LinearDamp = DotlineManager.Instance.DotDamping;
		StaticVelocity = DotlineManager.Instance.DotStaticVelocity;
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
		DotAnimPlayer.CurrentAnimation = Color switch
		{
			DotlineColor.Blue => "BlueStatic",
			DotlineColor.Red => "RedStatic",
			DotlineColor.Purple => "PurpleStatic",
			DotlineColor.White => "BlueStatic",
			_ => "BlueStatic"
		};
	}

	public override void _PhysicsProcess(double delta)
	{
		if (LinearVelocity.Length() < StaticVelocity)
		{
			LinearVelocity = Vector2.Zero;
		}

		if (State == DotState.Static && LinearVelocity.Length() < VelocityGate && !hasEmittedLineUp)
		{
			
			DotlineManager.Instance.LineUpDot(this);
		}

		if (State == DotState.Lined && LinearVelocity.Length() >= VelocityGate + 10f && !hasEmittedUnline)
		{
			GD.Print("Unlining dot"+ Name);
			GD.Print(State);
			GD.Print(LinearVelocity.Length());
			
			DotlineManager.Instance.UnlineDot(this);
		}
		
	}
	
}
