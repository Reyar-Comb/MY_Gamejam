using Godot;
using System;

public partial class Player_UniveralState : State
{
    private Player Player => field ??= Owner as Player;
    private AnimatedSprite2D Sprite => field ??= Player.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    protected override void PhysicsUpdate(double delta)
    {
        Player.MoveAndSlide();
        if (Player.Velocity.X != 0)
        {
            Sprite.FlipH = Player.Velocity.X < 0;
        }
    }
}
