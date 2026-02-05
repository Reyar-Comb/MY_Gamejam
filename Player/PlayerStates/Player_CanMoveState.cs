using Godot;
using System;

public partial class Player_CanMoveState : State
{
	[Export] public float Speed = 300.0f;
	[Export] public float GroundAcceleration = 2000.0f;
	[Export] public float GroundFriction = 1500.0f;
	[Export] public float AirAcceleration = 2000.0f;
	[Export] public float AirFriction = 100.0f;

	private Player Player => field ??= Owner as Player;

	protected override void PhysicsUpdate(double delta)
	{
		float dt = (float)delta;
		int direction = GetDirection();
		Vector2 velocity = Player.Velocity;

		bool onFloor = Player.IsOnFloor();
		float acceleration = onFloor ? GroundAcceleration : AirAcceleration;
		float friction = onFloor ? GroundFriction : AirFriction;

		if (direction != 0)
		{
			bool isOverspeed = Mathf.Abs(velocity.X) > Speed && Mathf.Sign(velocity.X) == direction;
			
			if (onFloor || !isOverspeed)
			{
				velocity.X = Mathf.MoveToward(velocity.X, direction * Speed, acceleration * dt);
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, friction * dt);
		}
		
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
