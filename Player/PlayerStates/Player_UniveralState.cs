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

        int direction = GetDirection();
        if (direction != 0)
        {
            Sprite.FlipH = direction < 0;
        }
    }
    private void OnLineDetected(Area2D area)
    {
        if (area is not LineArea lineArea) return;

        DotlineColor color = lineArea.LineColor;
        Player.LastTouchedLine = lineArea.Line;
        GD.Print($"{color} Line Detected");
        if (color == DotlineColor.Red || color == DotlineColor.Purple) return;

        AskTransit($"{color}Affected");
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
