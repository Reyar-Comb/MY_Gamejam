using Godot;
using System;

public partial class Player_GravityAffectedState : State
{
    [Export] public float GravityScale = 1.0f;
    private Player Player => field ??= Owner as Player;

    protected override void PhysicsUpdate(double delta)
    {
        Vector2 velocity = Player.Velocity;
        velocity += Player.GetGravity() * GravityScale * (float)delta;
        Player.Velocity = velocity;
    }
}
