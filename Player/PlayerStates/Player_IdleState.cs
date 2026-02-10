using Godot;
using System;

public partial class Player_IdleState : State
{
    private Player Player => field ??= Owner as Player;
    private AnimationPlayer AnimPlayer => field ??= GetNode<AnimationPlayer>("%AnimationPlayer");

    private bool _isMoving = false;
    public void OnColorChanged(string newColor)
    {
        if (string.IsNullOrEmpty(AnimPlayer.CurrentAnimation))
        {
            AnimPlayer.Play($"{newColor}Idle");
            return;
        }
        double currentPos = AnimPlayer.CurrentAnimationPosition;
        AnimPlayer.Stop();
        AnimPlayer.Play($"{newColor}Idle");
        AnimPlayer.Seek(currentPos, true);
    }
    protected override void Enter()
    {
        AnimPlayer.Play($"{Player.PlayerColor}Idle");
        Player.Connect(nameof(Player.ColorChanged), new Callable(this, nameof(OnColorChanged)));
    }

    protected override void Exit()
    {
        Player.Disconnect(nameof(Player.ColorChanged), new Callable(this, nameof(OnColorChanged)));
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

        //GD.Print("Player velocity length: " + Player.Velocity.Length());
        if (_isMoving == false && Player.Velocity.Length() > 100f && Player.IsOnFloor())
        {
            
            _isMoving = true;
            AudioManager.Instance.PlayOnceSFX("walk");
        }
        else if (_isMoving == true && Player.Velocity.Length() <= 100f)
        {
            GD.Print("Stopping walk SFX");
            _isMoving = false;
            AudioManager.Instance.StopOnceSFX("walk");
        }
    }


}
