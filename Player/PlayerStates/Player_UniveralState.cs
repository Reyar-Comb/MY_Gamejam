using Godot;
using System;

public partial class Player_UniveralState : State
{
    private Player Player => field ??= Owner as Player;
    private AnimatedSprite2D Sprite => field ??= Player.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    private Area2D LineDetectArea => field ??= Player.GetNode<Area2D>("%LineDetectArea");
    protected override void Enter()
    {
        LineDetectArea.AreaEntered += OnLineDetected;
    }
    protected override void Exit()
    {
        LineDetectArea.AreaEntered -= OnLineDetected;
    }
    protected override void PhysicsUpdate(double delta)
    {
        Player.MoveAndSlide();
        if (Player.Velocity.X != 0)
        {
            Sprite.FlipH = Player.Velocity.X < 0;
        }
    }
    private void OnLineDetected(Area2D area)
    {
        if (area is LineArea lineArea)
        {
            GD.Print("Line Detected: " + lineArea.Name);
        }
    }
}
