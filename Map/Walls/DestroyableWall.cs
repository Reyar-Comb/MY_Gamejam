using Godot;
using System;

public partial class DestroyableWall : StaticBody2D
{
	public Area2D LeftSide => field ??= GetNode<Area2D>("LeftSide");
	public Area2D RightSide => field ??= GetNode<Area2D>("RightSide");
	public Area2D TopSide => field ??= GetNode<Area2D>("TopSide");
	public Area2D BottomSide => field ??= GetNode<Area2D>("BottomSide");
	private Sprite2D Sprite => field ??= GetNode<Sprite2D>("Sprite2D");
	private ShaderMaterial Mat => field ??= Sprite.Material as ShaderMaterial;
	public void Destroy()
	{
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
}
