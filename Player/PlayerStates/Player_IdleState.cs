using Godot;
using System;

public partial class Player_IdleState : State
{
    private Player Player => field ??= Owner as Player;
    private AnimationPlayer AnimPlayer => field ??= GetNode<AnimationPlayer>("%AnimationPlayer");
    protected override void Enter()
    {
        AnimPlayer.Play($"{Player.PlayerColor}Idle");
    }

    protected override void PhysicsUpdate(double delta)
    {
        if (Input.IsActionJustPressed("Jump"))
        {
            AskTransit("JumpIntro");
            return;
        }

        if (Player.IsOnFloor() == false)
        {
            AskTransit("JumpLoop");
        }
    }
}
