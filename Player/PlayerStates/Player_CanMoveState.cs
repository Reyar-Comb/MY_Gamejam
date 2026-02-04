using Godot;
using System;

public partial class Player_CanMoveState : State
{
	[Export] public float Speed = 200.0f;
	private Player Player => field ??= Owner as Player;
	protected override void PhysicsUpdate(double delta)
	{
		int direction = GetDirection();
		Vector2 velocity = Player.Velocity;
		velocity.X = direction * Speed;
		Player.Velocity = velocity;
	}
	private int GetDirection()
	{
		int direction = 0;
		if (Input.IsActionPressed("Right"))
		{
			direction++;
		}
		if (Input.IsActionPressed("Left"))
		{
			direction--;
		}
		return direction;
	}
}
