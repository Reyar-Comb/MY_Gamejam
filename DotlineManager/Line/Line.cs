using Godot;
using System;
using System.Collections;
using System.Threading.Tasks;
public partial class Line : Line2D
{
	[Export] public CollisionShape2D LineCollisionShape;
	public DotlineColor Color;

	public Dot StartDot;
	public Dot EndDot;

	public Tween Tween;
	public LineState State = LineState.Idle;
	public void SetColor(DotlineColor color)
	{
		Color = color;
		UpdateLineColor();
	}

	public void UpdateLineColor()
	{
		DefaultColor = Color switch
		{
			DotlineColor.Blue => Colors.Blue,
			DotlineColor.Red => Colors.Red,
			DotlineColor.Purple => Colors.Purple,
			DotlineColor.White => Colors.White,
			_ => Colors.White
		};
	}

	public async void Spawn()
	{
		State = LineState.Lining;
		Vector2 initStartPos = StartDot.GlobalPosition;
		SetPointPosition(0, initStartPos);
		SetPointPosition(1, initStartPos);
		Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0f);
		Tween = CreateTween()
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
		Tween.TweenMethod(
			Callable.From<float>((progress) => {
				Vector2 currentStartPos = StartDot.GlobalPosition;
				Vector2 currentEndPos = EndDot.GlobalPosition;
				Vector2 animatedEndPos = initStartPos.Lerp(currentEndPos, progress);
				SetPointPosition(0, currentStartPos);
				SetPointPosition(1, animatedEndPos);
			}),
				
			0f,
			1f,
			0.2f
		);
		Tween.Parallel().TweenProperty(this, "modulate:a", 0.8f, 0.2f);
		await ToSignal(Tween, "finished");
		State = LineState.Idle;
	}

	public async Task Clear()
	{
		State = LineState.Lining;
		Vector2 initStartPos = StartDot.GlobalPosition;
		SetPointPosition(0, initStartPos);
		SetPointPosition(1, initStartPos);
		Tween = CreateTween()
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
		Tween.TweenMethod(
			Callable.From<float>((progress) => {
				Vector2 currentStartPos = StartDot.GlobalPosition;
				Vector2 currentEndPos = EndDot.GlobalPosition;
				Vector2 animatedEndPos = initStartPos.Lerp(currentEndPos, progress);
				SetPointPosition(0, currentStartPos);
				SetPointPosition(1, animatedEndPos);
			}),
				
			1f,
			0f,
			0.2f
		);
		Tween.Parallel().TweenProperty(this, "modulate:a", 0f, 0.2f);
		await ToSignal(Tween, "finished");
		State = LineState.Idle;
		QueueFree();
	}

	public Vector2 GetDirection()
	{
		if (!IsInstanceValid(StartDot) || !IsInstanceValid(EndDot)) return Vector2.Right;
		return (EndDot.GlobalPosition - StartDot.GlobalPosition).Normalized();
	}

	public Vector2 GetNormal()
	{
		Vector2 dir = GetDirection();
		Vector2 normal = dir.Orthogonal();
		if (normal.Y > 0)
		{
			normal = -normal;
		}
		return normal;
	}

	public void SetLinePosition()
	{
		SetPointPosition(0, StartDot.GlobalPosition);
		SetPointPosition(1, EndDot.GlobalPosition);

		LineCollisionShape.Shape = new SegmentShape2D()
		{
			A = ToLocal(StartDot.GlobalPosition),
			B = ToLocal(EndDot.GlobalPosition)
		};
		// SegmentShape2D segment = LineCollisionShape.Shape as SegmentShape2D;
		// GD.Print(segment.A + " to " + segment.B);
	}
	public override void _PhysicsProcess(double delta)
	{
		if (State == LineState.Idle && IsInstanceValid(StartDot) && IsInstanceValid(EndDot))
			SetLinePosition();
	}
}
