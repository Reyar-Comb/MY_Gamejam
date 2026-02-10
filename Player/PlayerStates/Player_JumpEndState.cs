using Godot;
using System;

public partial class Player_JumpEndState : State
{
    private AnimationPlayer AnimPlayer => field ??= GetNode<AnimationPlayer>("%AnimationPlayer");
    private Player Player => field ??= Owner as Player;

    public void OnColorChanged(string newColor)
    {
        if (string.IsNullOrEmpty(AnimPlayer.CurrentAnimation))
        {
            AnimPlayer.Play($"{newColor}JumpEnd");
            return;
        }
        double currentPos = AnimPlayer.CurrentAnimationPosition;
        AnimPlayer.Stop();
        AnimPlayer.Play($"{newColor}JumpEnd");
        AnimPlayer.Seek(currentPos, true);
    }
    protected override void Enter()
    {
        AnimPlayer.Play($"{Player.PlayerColor}JumpEnd");
        AnimPlayer.AnimationFinished += OnAnimationFinished;
        Player.Connect(nameof(Player.ColorChanged), new Callable(this, nameof(OnColorChanged)));
    }
    protected override void Exit()
    {
        AnimPlayer.AnimationFinished -= OnAnimationFinished;
        Player.Disconnect(nameof(Player.ColorChanged), new Callable(this, nameof(OnColorChanged)));
    }
    private void OnAnimationFinished(StringName _)
    {
        AudioManager.Instance.PlaySFX("land");
        AskTransit("Idle");
    }
}
