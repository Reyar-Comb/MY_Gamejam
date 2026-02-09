using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Signal] public delegate void ColorChangedEventHandler(string newColor);
	[Export] public StateTree StateTree;
	[Export] public AnimatedSprite2D DotPreviewSprite;
	//[Export] public float MaxDotSpeed = 500f;
	[Export] public float MaxDragDistance = 200f;
	[Export] public float MinDragDistance = 20f;
	[Export] public float k = 0.9f;

	public float damping = 1.0f;
	private bool _isDragging = false;
	private bool _isCancelled = false;
	private Vector2 _dragStartPos;
	
	public Line LastTouchedLine = null;
	public string PlayerColor { 
		get => field; 
		set
		{
			field = value;
			EmitSignal(nameof(ColorChanged), value);
		} } = "White";

	public override void _Ready()
	{
		DotlineManager.Instance.Player = this;
		GameManager.Instance.Player = this;
		damping = DotlineManager.Instance.DotDamping;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDragging)
		{
			if (PlayerColor == "White") return;
			DotPreviewSprite.Play(PlayerColor);
			DotPreviewSprite.Visible = true;
			Vector2 mousePos = GetViewport().GetMousePosition();
			Vector2 dragVector = mousePos - _dragStartPos;
			float dragDistance = dragVector.Length();
			//GD.Print("Dragging... Current mouse pos: " + mousePos + ", drag vector: " + dragVector + ", distance: " + dragDistance);
			if (dragDistance > MaxDragDistance)
			{
				dragVector = dragVector.Normalized() * MaxDragDistance;
			}
			if (dragDistance < MinDragDistance)
			{
				DotPreviewSprite.Visible = false;
				return;
			}

			Vector2 velocity = dragVector * (DotlineManager.Instance.MaxDotSpeed / MaxDragDistance);
			Vector2 previewPos = (velocity - DotlineManager.Instance.DotStaticVelocity * velocity.Normalized()) / damping;
			previewPos *= k;
			DotPreviewSprite.Position = -previewPos;
		}
		else
		{
			DotPreviewSprite.Visible = false;
		}

		//GD.Print("animation playing: " + DotPreviewSprite.Animation);
	}

	public override void _Input(InputEvent @event)
	{
		if (PlayerColor == "White" || StateTree._currentState.Name == "PurpleAffected")
			return;
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseEvent.Pressed && _isCancelled == false)
				{
					GD.Print("Started dragging at: " + mouseEvent.Position);
					_isDragging = true;
					_dragStartPos = mouseEvent.Position;
				}
				else
				{
					_isDragging = false;
					if (_isCancelled == false)
					{
						Vector2 dragVector = mouseEvent.Position - _dragStartPos;
						float dragDistance = dragVector.Length();

						if (dragDistance > MaxDragDistance)
						{
							dragVector = dragVector.Normalized() * MaxDragDistance;
						}
						if (dragDistance < MinDragDistance)
						{
							_isCancelled = false;
							return;
						}

						Vector2 velocity = dragVector * (DotlineManager.Instance.MaxDotSpeed / MaxDragDistance);
						DotlineManager.Instance.SpawnDot(-velocity);
					}
					_isCancelled = false;
				}
			}
			
			if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				_isDragging = false;
				_isCancelled = true;
			}
		}


	}



}
