using Godot;
using System;

public partial class Player_CanJumpState : State
{
	[Export] public float JumpVelocity = 900.0f;
	private Player Player => field ??= Owner as Player;
	protected override void PhysicsUpdate(double delta)
	{
		if (Input.IsActionJustPressed("Jump"))
		{
			Vector2 velocity = Player.Velocity;
			velocity.Y = -JumpVelocity;
			Player.Velocity = velocity;
			AskTransit("JumpIntro");
			return;
		}

		if (Player.IsOnFloor() == false)
		{
			AskTransit("JumpLoop");
		}
	}
}
