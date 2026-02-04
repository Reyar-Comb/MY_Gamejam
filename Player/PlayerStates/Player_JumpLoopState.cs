using Godot;
using System;

public partial class Player_JumpLoopState : State
{
    private AnimationPlayer AnimPlayer => field ??= GetNode<AnimationPlayer>("%AnimationPlayer");
    private Player Player => field ??= Owner as Player;
    protected override void Enter()
    {
        AnimPlayer.Play($"{Player.PlayerColor}JumpLoop");
    }
    protected override void PhysicsUpdate(double delta)
    {
        if (Player.IsOnFloor())
        {
            AskTransit("JumpEnd");
        }
    }
}
