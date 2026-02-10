using Godot;
using System;

public partial class Player_BlueAffectedState : State
{
	[Export] public float LaunchSpeed = 2000f;
	private Player Player => field ??= Owner as Player;
	protected override void Enter()
	{
		Player.Velocity = Player.LastTouchedLine.GetNormal() * LaunchSpeed;
		AskTransit("JumpLoop");
		AudioManager.Instance.PlaySFX("blueline");
	}
}
