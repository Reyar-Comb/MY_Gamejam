using Godot;
using System;

public partial class Player_JumpLoopState : State
{
    private AnimationPlayer AnimPlayer => field ??= GetNode<AnimationPlayer>("%AnimationPlayer");
    private Player Player => field ??= Owner as Player;

    public void OnColorChanged(string newColor)
    {
        if (string.IsNullOrEmpty(AnimPlayer.CurrentAnimation))
        {
            AnimPlayer.Play($"{newColor}JumpLoop");
            return;
        }
        double currentPos = AnimPlayer.CurrentAnimationPosition;
        AnimPlayer.Stop();
        AnimPlayer.Play($"{newColor}JumpLoop");
        AnimPlayer.Seek(currentPos, true);
    }

    protected override void Enter()
    {
        AnimPlayer.Play($"{Player.PlayerColor}JumpLoop");
        Player.Connect(nameof(Player.ColorChanged), new Callable(this, nameof(OnColorChanged)));
    }
    protected override void PhysicsUpdate(double delta)
    {
        if (Player.IsOnFloor())
        {
            AskTransit("JumpEnd");
        }
    }
    protected override void Exit()
    {
        Player.Disconnect(nameof(Player.ColorChanged), new Callable(this, nameof(OnColorChanged)));
    }
}
