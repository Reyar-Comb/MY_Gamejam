using Godot;
using System;
using System.Linq;

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
    private void HandleSpriteFlip()
    {
        int direction = GetDirection();
        if (direction != 0)
        {
            Sprite.FlipH = direction < 0;
        }
    }
    private void HandleColorBlockCollision()
    {
        for (int i = 0; i < Player.GetSlideCollisionCount(); i++)
        {
            var collision = Player.GetSlideCollision(i);
            if (collision.GetCollider() is ColorBlock colorBlock)
            {
                DotlineManager.Instance.CurrentColor = colorBlock.GetColor();
                return;
            }
        }
    }
    private async void HandleSpikeCollision()
    {
        for (int i = 0; i < Player.GetSlideCollisionCount(); i++)
        {
            var collision = Player.GetSlideCollision(i);
            GD.Print($"Collision with {collision.GetCollider()}");
            if (collision.GetCollider() is SpikeLayer spikeLayer)
            {
                await DotlineManager.Instance.RewindProgress("cancel");
                return;
            }
        }
    }
    protected override void PhysicsUpdate(double delta)
    {
        Player.MoveAndSlide();

        HandleSpriteFlip();
        HandleColorBlockCollision();
        HandleSpikeCollision();
    }
    private void OnLineDetected(Area2D area)
    {
        if (area is not LineArea lineArea) return;

        DotlineColor color = lineArea.LineColor;
        Player.LastTouchedLine = lineArea.Line;
        GD.Print($"{color} Line Detected");
        if (color == DotlineColor.Red) return;

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
