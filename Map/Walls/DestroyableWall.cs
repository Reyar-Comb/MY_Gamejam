using Godot;
using System;

public partial class DestroyableWall : StaticBody2D
{
	[Export] public int WallID = 0;
	public Area2D LeftSide => field ??= GetNode<Area2D>("LeftSide");
	public Area2D RightSide => field ??= GetNode<Area2D>("RightSide");
	public Area2D TopSide => field ??= GetNode<Area2D>("TopSide");
	public Area2D BottomSide => field ??= GetNode<Area2D>("BottomSide");
	private Sprite2D Sprite => field ??= GetNode<Sprite2D>("Sprite2D");
	private ShaderMaterial Mat => field ??= Sprite.Material as ShaderMaterial;
	private bool _isDestroyed = false;
	public void Destroy()
	{
		if (_isDestroyed) return;
		_isDestroyed = true;
		SendID();
		
		Tween tween = CreateTween()
			.SetTrans(Tween.TransitionType.Quart)
			.SetEase(Tween.EaseType.InOut);
		tween.TweenMethod(
			Callable.From<float>(v => Mat.SetShaderParameter("white_amount", v)),
			0f, 1f, 0.2f
		);
		tween.TweenMethod(
			Callable.From<float>(v => {
				Mat.SetShaderParameter("white_amount", v);
				Mat.SetShaderParameter("alpha_amount", v);
			}),
			1f, 0f, 0.2f
		);
		
		tween.TweenCallback(Callable.From(QueueFree));
	}

	public void SendID()
	{
		GameManager.Instance.DestroyWall(WallID);
	}
}
