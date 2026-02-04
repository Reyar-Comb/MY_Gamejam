using Godot;
using System;

public partial class Player_JumpEndState : State
{
    private AnimationPlayer AnimPlayer => field ??= GetNode<AnimationPlayer>("%AnimationPlayer");
    private Player Player => field ??= Owner as Player;
    protected override void Enter()
    {
        AnimPlayer.Play($"{Player.PlayerColor}JumpEnd");
        AnimPlayer.AnimationFinished += OnAnimationFinished;
    }
    protected override void Exit()
    {
        AnimPlayer.AnimationFinished -= OnAnimationFinished;
    }
    private void OnAnimationFinished(StringName _)
    {
        AskTransit("Idle");
    }
}
