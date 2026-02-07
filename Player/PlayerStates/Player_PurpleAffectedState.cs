using Godot;
using System;

public partial class Player_PurpleAffectedState : State
{
	[Export] public float SlideSpeed = 400f;
	[Export] public float JumpVelocity = 900f;

	private Player Player => field ??= Owner as Player;
	private AnimationPlayer AnimPlayer => field ??= GetNode<AnimationPlayer>("%AnimationPlayer");
    private CollisionShape2D CollisionShape => field ??= GetNode<CollisionShape2D>("%CollisionShape2D");

	private Line _attachedLine;
	private float _t; // 0~1 参数，表示在线上的位置
	private uint _savedCollisionMask;
	private uint _savedCollisionLayer;

	protected override void Enter()
	{
		_attachedLine = Player.LastTouchedLine;

		// 计算玩家在线上的初始投影位置
		_t = GetClosestT(Player.GlobalPosition);

		// 保存碰撞设置，然后禁用碰撞以穿墙
		_savedCollisionMask = Player.CollisionMask;
		_savedCollisionLayer = Player.CollisionLayer;
		Player.CollisionMask = 0;
		Player.CollisionLayer = 0;

		Player.Velocity = Vector2.Zero;

		// 播放当前颜色的 Idle 动画
		AnimPlayer.Play($"{Player.PlayerColor}Idle");
	}

	protected override void Exit()
	{
		// 恢复碰撞
		Player.CollisionMask = _savedCollisionMask;
		Player.CollisionLayer = _savedCollisionLayer;
	}
    private bool IsAttachedLineValid()
    {
        return IsInstanceValid(_attachedLine)
            && IsInstanceValid(_attachedLine.StartDot)
            && IsInstanceValid(_attachedLine.EndDot);
    }
	protected override void PhysicsUpdate(double delta)
	{
		// 线被销毁时强制脱离
		if (!IsAttachedLineValid())
		{
			Player.Velocity = Vector2.Zero;
			AskTransit("JumpLoop");
			return;
		}

		Vector2 lineStart = _attachedLine.StartDot.GlobalPosition;
		Vector2 lineEnd = _attachedLine.EndDot.GlobalPosition;
		float lineLength = lineStart.DistanceTo(lineEnd);

		if (lineLength < 0.001f) return;

		// 根据输入沿线移动
		int direction = GetDirection();
		Vector2 lineDir = (lineEnd - lineStart).Normalized();

		// 按 Right 向线的"右方"移动，按 Left 向"左方"移动
		// 用线方向的 X 分量来决定 t 的增减方向
		// 对于近乎垂直的线，退化使用 Y 分量
		float signFactor;
		if (Mathf.Abs(lineDir.X) > 0.01f)
			signFactor = Mathf.Sign(lineDir.X);
		else
			signFactor = -Mathf.Sign(lineDir.Y); // 垂直线：Right = 向上

		_t += direction * signFactor * SlideSpeed * (float)delta / lineLength;
		_t = Mathf.Clamp(_t, 0f, 1f);

		// 设置玩家位置到线上
		Player.GlobalPosition = lineStart.Lerp(lineEnd, _t);
		Player.Velocity = Vector2.Zero;

		// 跳跃脱离
		if (Input.IsActionJustPressed("Jump"))
		{
			if (!IsInsideWall())
			{
				Player.Velocity = new Vector2(0, -JumpVelocity);
				AskTransit("JumpIntro");
			}
		}
	}

	/// <summary>
	/// 计算点在线段上的最近投影参数 t (0~1)
	/// </summary>
	private float GetClosestT(Vector2 point)
	{
		Vector2 lineStart = _attachedLine.StartDot.GlobalPosition;
		Vector2 lineEnd = _attachedLine.EndDot.GlobalPosition;
		Vector2 lineVec = lineEnd - lineStart;
		float lengthSq = lineVec.LengthSquared();
		if (lengthSq < 0.001f) return 0f;
		float t = (point - lineStart).Dot(lineVec) / lengthSq;
		return Mathf.Clamp(t, 0f, 1f);
	}

	/// <summary>
	/// 使用原始碰撞掩码检测玩家当前位置是否在墙内
	/// </summary>
	private bool IsInsideWall()
	{
		var spaceState = Player.GetWorld2D().DirectSpaceState;

        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = CollisionShape.Shape,
            Transform = new Transform2D(Player.GlobalRotation, CollisionShape.GlobalPosition),
            CollisionMask = _savedCollisionMask,
            Exclude = new Godot.Collections.Array<Rid> { Player.GetRid() }
        };

        var results = spaceState.IntersectShape(query);
		return results.Count > 0;
	}

	private int GetDirection()
	{
		int direction = 0;
		if (Input.IsActionPressed("Right")) direction++;
		if (Input.IsActionPressed("Left")) direction--;
		return direction;
	}
}
