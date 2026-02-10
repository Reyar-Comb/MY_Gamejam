using Godot;
using System;

public partial class Player_JumpIntroState : State
{
    private AnimationPlayer AnimPlayer => field ??= GetNode<AnimationPlayer>("%AnimationPlayer");
    private Player Player => field ??= Owner as Player;

    public void OnColorChanged(string newColor)
    {
        if (string.IsNullOrEmpty(AnimPlayer.CurrentAnimation))
        {
            AnimPlayer.Play($"{newColor}JumpIntro");
            return;
        }
        double currentPos = AnimPlayer.CurrentAnimationPosition;
        AnimPlayer.Stop();
        AnimPlayer.Play($"{newColor}JumpIntro");
        AnimPlayer.Seek(currentPos, true);
    }

    protected override void Enter()
    {
        AnimPlayer.Play($"{Player.PlayerColor}JumpIntro");
        AnimPlayer.AnimationFinished += OnAnimationFinished;
        AudioManager.Instance.PlaySFX("jump");
        Player.Connect(nameof(Player.ColorChanged), new Callable(this, nameof(OnColorChanged)));
    }
    protected override void Exit()
    {
        AnimPlayer.AnimationFinished -= OnAnimationFinished;
        Player.Disconnect(nameof(Player.ColorChanged), new Callable(this, nameof(OnColorChanged)));
    }
    private void OnAnimationFinished(StringName _)
    {
        AskTransit("JumpLoop");
    }
}
