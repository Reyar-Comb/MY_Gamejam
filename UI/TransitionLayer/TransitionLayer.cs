using Godot;
using System;
using System.Threading.Tasks;
public partial class TransitionLayer : CanvasLayer
{
	private ColorRect DarkBG => field ??= GetNode<ColorRect>("%DarkBG");
	public static TransitionLayer Instance { get; private set; }
	public override void _Ready()
	{
		Instance = this;
	}
	public async Task Appear()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(DarkBG, "modulate:a", 1f, 0.5f).SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.In);
		await ToSignal(tween, Tween.SignalName.Finished);
	}
	public async Task Disappear()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(DarkBG, "modulate:a", 0f, 0.5f).SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.In);
		await ToSignal(tween, Tween.SignalName.Finished);
	}
}
