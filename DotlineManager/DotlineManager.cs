using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public partial class DotlineManager : Node2D
{
	public static DotlineManager Instance;

	private SemaphoreSlim _clearLock = new SemaphoreSlim(1, 1);

	[Export] public PackedScene DotScene;
	[Export] public PackedScene LineScene;
	[Signal] public delegate void DotsLabelRefreshEventHandler(int currentDots, int maxDots);
	public DotlineColor CurrentColor
	{
		get => field;
		set
		{
			field = value;
			Player.PlayerColor = value.ToString();
		}
	}
	public Player Player;
	[Export] public float DotDamping = 1.0f;
	[Export] public float DotStaticVelocity = 40f;
	[Export] public int MaxHistoryDots
	{
		get => field;
		set
		{
			field = value;
			EmitSignal(SignalName.DotsLabelRefresh, HistoryDots.Count, value);
		}
	} = 5;
	[Export] public float RedDotRange = 500f;
	[Export] public float BlueDotRange = 500f;
	[Export] public float PurpleDotRange = 500f;

	public float MaxDotSpeed
	{
		get
		{
			switch (CurrentColor)
			{
				case DotlineColor.Blue:
					return BlueDotRange;
				case DotlineColor.Red:
					return RedDotRange;
				case DotlineColor.Purple:
					return PurpleDotRange;
				default:
					return 500f;
			}
		}
		set { }
	}

	public Queue<Dot> BlueDotQueue { get; private set; } = new Queue<Dot>();
	public Queue<Dot> RedDotQueue { get; private set; } = new Queue<Dot>();
	public Queue<Dot> PurpleDotQueue { get; private set; } = new Queue<Dot>();
	public Queue<DotlineColor> HistoryDots = new Queue<DotlineColor>();
	public List<Line> BlueLines = new List<Line>();
	public List<Line> RedLines = new List<Line>();
	public List<Line> PurpleLines = new List<Line>();
	public Dot FirstBlueDot => BlueDotQueue.Count > 0 ? BlueDotQueue.Peek() : null;
	public Dot FirstRedDot => RedDotQueue.Count > 0 ? RedDotQueue.Peek() : null;
	public Dot FirstPurpleDot => PurpleDotQueue.Count > 0 ? PurpleDotQueue.Peek() : null;
	public override void _Process(double delta)
	{
		GD.Print(HistoryDots.Count);
	}

	public override void _Ready()
	{

		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			GD.PrintErr("Multiple instances of DotlineManager detected!");
			QueueFree();
		}
		GameManager.Instance.Connect("CheckPointChanged", new Callable(this, "OnCheckPointChanged"));
		GD.Print("Connected");
		MaxHistoryDots = MaxHistoryDots;
	}

	public Vector2 GetDirection()
	{
		return (GetGlobalMousePosition() - Player.GlobalPosition).Normalized();
	}
	private DotlineColor IncrementHistoryDots(DotlineColor color)
	{
		HistoryDots.Enqueue(color);
		if (HistoryDots.Count <= MaxHistoryDots)
			EmitSignal(SignalName.DotsLabelRefresh, HistoryDots.Count, MaxHistoryDots);
		return color;
	}
	private DotlineColor? DecrementHistoryDots()
	{
		if (HistoryDots.Count <= 0) return null;
		DotlineColor color = HistoryDots.Dequeue();
		EmitSignal(SignalName.DotsLabelRefresh, HistoryDots.Count, MaxHistoryDots);
		
		return color;
	}
	private void ClearHistoryDots()
	{
		HistoryDots.Clear();
		EmitSignal(SignalName.DotsLabelRefresh, HistoryDots.Count, MaxHistoryDots);
	}
	public Queue<Dot> GetDotQueue(DotlineColor color)
	{
		return color switch
		{
			DotlineColor.Blue => BlueDotQueue,
			DotlineColor.Red => RedDotQueue,
			DotlineColor.Purple => PurpleDotQueue,
			_ => null
		};
	}
	public void LineUpDot(Dot dot)
	{
		if (dot == null)
			return;
		//GD.Print(RedDotQueue.Peek().Name);
		if (dot.State != DotState.Static)
			return;
		if (!dot.VelUnderGate)
			return;
		DotlineColor color = dot.Color;
		Queue<Dot> DotQueue = GetDotQueue(color);
		if (DotQueue == null || DotQueue.Count == 0)
			return;
		if (dot.hasEmittedLineUp)
			return;

		dot.hasEmittedLineUp = true;
		dot.hasEmittedUnline = false;
		// ...existing code...
		if (dot == DotQueue.Peek())
		{
			// 只连已经停下的点
			var readyDots = new List<Dot>();
			foreach (Dot d in DotQueue.ToArray())
			{
				if (d == dot) continue;
				if (!d.VelUnderGate)
				{
					d.hasEmittedLineUp = false; // 还在动，允许之后再连
					continue;
				}
				readyDots.Add(d);
			}

			if (readyDots.Count == 0)
			{
				dot.hasEmittedLineUp = false; // 没人可连，中心允许下次再触发
				return;
			}

			string colorName = dot.Color switch
			{
				DotlineColor.Blue => "Blue",
				DotlineColor.Red => "Red",
				DotlineColor.Purple => "Purple",
				_ => "Blue"
			};
			dot.DotAnimPlayer.Play(colorName + "DotTrans");
			dot.State = DotState.Lined;

			foreach (Dot d in readyDots)
			{
				d.State = DotState.Lined;
				d.hasEmittedLineUp = true;
				d.hasEmittedUnline = false;

				Line newLine = MakeLine(dot, d);
				Animate(dot, d, newLine, "2l");
			}
			return;
		}
		// ...existing code...

		else
		{
			if (!DotQueue.Peek().VelUnderGate)
			{
				dot.hasEmittedLineUp = false;
				return;
			}

			dot.State = DotState.Lined;
			if (DotQueue.Peek().State == DotState.Static && DotQueue.Count == 2)
			{
				DotQueue.Peek().State = DotState.Lined;
				GD.Print("Lining last dot in queue");
				DotQueue.Peek().hasEmittedLineUp = true;
				DotQueue.Peek().hasEmittedUnline = false;
				Line newLine = MakeLine(DotQueue.Peek(), dot);
				Animate(DotQueue.Peek(), dot, newLine, "3l");
				DotQueue.Peek().State = DotState.Lined;
			}
			else
			{
				DotQueue.Peek().State = DotState.Lined;
				if (DotQueue.Peek().State != DotState.Lined)
					GD.Print("wtf");

				Line newLine = MakeLine(DotQueue.Peek(), dot);
				Animate(DotQueue.Peek(), dot, newLine, "2l");
			}
		}
	}



	public void UnlineDot(Dot dot)
	{
		if (dot.hasEmittedUnline)
			return;
		if (dot.State != DotState.Lined)
			return;

		DotlineColor color = dot.Color;
		Queue<Dot> DotQueue = GetDotQueue(color);

		dot.hasEmittedUnline = true;
		dot.hasEmittedLineUp = false;

		if (dot == DotQueue.Peek())
		{

			string colorName = dot.Color switch
			{
				DotlineColor.Blue => "Blue",
				DotlineColor.Red => "Red",
				DotlineColor.Purple => "Purple",
				_ => "Blue"
			};
			dot.DotAnimPlayer.Play(colorName + "DotTrans_Inverse");
			dot.State = DotState.Static;

			foreach (Dot d in DotQueue.ToArray())
			{
				if (d == dot) continue;

				d.State = DotState.Static;
				d.hasEmittedLineUp = false;

				Line lineToClear = d.CurrentLine;
				if (lineToClear != null)
				{
					dot.CurrentLines.Remove(lineToClear);
					Animate(dot, d, lineToClear, "2u"); // 只播 endDot 动画 + 清线
				}
			}
			return;
			/*
			foreach(Dot d in DotQueue.ToArray())
			{
				if (d == dot) continue;
				UnlineDot(d);
			}
			dot.State = DotState.Static;
			*/
		}
		else
		{
			if (DotQueue.Count == 2 || DotQueue.Peek().CurrentLines.Count == 1)
			{
				GD.Print("Unlining last dot in queue");
				dot.State = DotState.Static;
				dot.hasEmittedLineUp = false;
				DotQueue.Peek().State = DotState.Static;
				DotQueue.Peek().hasEmittedLineUp = false;
				DotQueue.Peek().hasEmittedUnline = true;
				Line lineToClear = dot.CurrentLine;
				DotQueue.Peek().CurrentLines.Remove(lineToClear);
				Animate(DotQueue.Peek(), dot, lineToClear, "3u");
			}
			else
			{
				dot.State = DotState.Static;
				dot.hasEmittedLineUp = false;
				Line lineToClear = dot.CurrentLine;
				dot.CurrentLines.Remove(lineToClear);
				Animate(DotQueue.Peek(), dot, lineToClear, "2u");
			}
		}
	}

	public Line MakeLine(Dot startDot, Dot endDot)
	{
		Line line = LineScene.Instantiate<Line>();
		line.StartDot = startDot;
		line.EndDot = endDot;
		line.SetColor(startDot.Color);
		endDot.CurrentLine = line;
		startDot.CurrentLines.Add(line);
		GetTree().CurrentScene.AddChild(line);
		return line;
	}

	public async void Animate(Dot startDot, Dot endDot, Line line, string mode)
	{
		if (startDot == null || endDot == null || line == null)
			return;
		if (!IsInstanceValid(startDot) || !IsInstanceValid(endDot) || !IsInstanceValid(line))
			return;
		if (startDot.DotAnimPlayer == null || endDot.DotAnimPlayer == null)
			return;

		string colorName = "";
		switch (startDot.Color)
		{
			case DotlineColor.Blue:
				colorName = "Blue";
				break;
			case DotlineColor.Red:
				colorName = "Red";
				break;
			case DotlineColor.Purple:
				colorName = "Purple";
				break;
		}
		switch (mode)
		{
			case "3l":
				startDot.DotAnimPlayer.Play(colorName + "DotTrans");
				await ToSignal(startDot.DotAnimPlayer, "animation_finished");
				line.Spawn();
				endDot.DotAnimPlayer.Play(colorName + "DotTrans");
				await ToSignal(endDot.DotAnimPlayer, "animation_finished");
				break;
			case "2l":
				line.Spawn();
				endDot.DotAnimPlayer.Play(colorName + "DotTrans");
				await ToSignal(endDot.DotAnimPlayer, "animation_finished");
				break;
			case "3u":
				GD.Print("Animating 3u");
				endDot.DotAnimPlayer.Play(colorName + "DotTrans_Inverse");
				await ToSignal(endDot.DotAnimPlayer, "animation_finished");
				await line.Clear();
				startDot.DotAnimPlayer.Play(colorName + "DotTrans_Inverse");
				await ToSignal(startDot.DotAnimPlayer, "animation_finished");
				break;
			case "2u":
				GD.Print("Animating 2u");
				endDot.DotAnimPlayer.Play(colorName + "DotTrans_Inverse");
				await line.Clear();
				break;
		}
	}

	public async Task AnimateAsync(Dot startDot, Dot endDot, Line line, string mode)
	{
		if (startDot == null || endDot == null || line == null)
			return;
		if (!IsInstanceValid(startDot) || !IsInstanceValid(endDot) || !IsInstanceValid(line))
			return;
		if (startDot.DotAnimPlayer == null || endDot.DotAnimPlayer == null)
			return;

		string colorName = startDot.Color switch
		{
			DotlineColor.Blue => "Blue",
			DotlineColor.Red => "Red",
			DotlineColor.Purple => "Purple",
			_ => "Blue"
		};
		switch (mode)
		{
			case "2u":
				endDot.DotAnimPlayer.Play(colorName + "DotTrans_Inverse");
				await line.Clear();
				break;
			case "3u":
				endDot.DotAnimPlayer.Play(colorName + "DotTrans_Inverse");
				await ToSignal(endDot.DotAnimPlayer, "animation_finished");
				await line.Clear();
				startDot.DotAnimPlayer.Play(colorName + "DotTrans_Inverse");
				//await ToSignal(startDot.DotAnimPlayer, "animation_finished");
				break;
		}
	}

	public async Task ClearColorDot(DotlineColor color, bool DeleteDot = false)
	{
		await _clearLock.WaitAsync();
		try
		{
			Queue<Dot> DotQueue = color switch
			{
				DotlineColor.Blue => BlueDotQueue,
				DotlineColor.Red => RedDotQueue,
				DotlineColor.Purple => PurpleDotQueue,
				_ => null
			};
			if (DotQueue == null || DotQueue.Count == 0)
				return;
			Dot dotToClear = DotQueue.Dequeue();
			GD.Print("Clearing dot: " + dotToClear.Name);
			var tasks = new List<Task>();
			foreach (var d in DotQueue.ToArray())
			{
				if (d == dotToClear) continue;
				var line = d.CurrentLine;
				if (line == null || !IsInstanceValid(line)) continue;

				dotToClear.CurrentLines.Remove(line);
				tasks.Add(AnimateAsync(dotToClear, d, line, "3u"));
			}
			await Task.WhenAll(tasks);

			if (DeleteDot == false) dotToClear.Clear();

			foreach (var d in DotQueue.ToArray())
			{
				d.State = DotState.Static;
				d.hasEmittedLineUp = false;
				d.hasEmittedUnline = true;
				d.CurrentLine = null;
			}
			Dot FirstDot = DotQueue.Count > 0 ? DotQueue.Peek() : null;
			FirstDot?.CurrentLines.Clear();
			if (FirstDot != null)
				LineUpDot(FirstDot);
		}
		finally
		{
			_clearLock.Release();
		}
	}

	public async Task ClearDots()
	{
		var tasks = new List<Task>();
		foreach (var dot in BlueDotQueue)
		{
			if (dot != BlueDotQueue.Peek())
			{
				tasks.Add(dot.CurrentLine.Clear());
			}
		}
		foreach (var dot in RedDotQueue)
		{
			if (dot != RedDotQueue.Peek())
			{
				tasks.Add(dot.CurrentLine.Clear());
			}
		}
		foreach (var dot in PurpleDotQueue)
		{
			if (dot != PurpleDotQueue.Peek())
			{
				tasks.Add(dot.CurrentLine.Clear());
			}
		}
		await Task.WhenAll(tasks.Select(t => t.ContinueWith(task =>
		{
			if (task.Exception != null)
				GD.PrintErr($"任务异常: {task.Exception.InnerException?.Message}");
		})));
		for (int i = BlueDotQueue.Count - 1; i >= 0; i--)
		{
			BlueDotQueue.Dequeue().Clear();
		}
		for (int i = RedDotQueue.Count - 1; i >= 0; i--)
		{
			RedDotQueue.Dequeue().Clear();
		}
		for (int i = PurpleDotQueue.Count - 1; i >= 0; i--)
		{
			PurpleDotQueue.Dequeue().Clear();
		}

		BlueDotQueue.Clear();
		RedDotQueue.Clear();
		PurpleDotQueue.Clear();
		ClearHistoryDots();

		ForceDeleteObjects();
	}
	public void ForceDeleteObjects()
	{
		var lines = GetTree().GetNodesInGroup("Lines");
		foreach (var line in lines)
		{
			if (line is Line l)
			{
				l.QueueFree();
			}

		}
		var dots = GetTree().GetNodesInGroup("Dots");
		foreach (var dot in dots)
		{
			if (dot is Dot d)
			{
				d.QueueFree();
			}
		}
	}

	public async void SpawnDot(Vector2 velocity)
	{

		if (CurrentColor == DotlineColor.White)
			return;
		Dot dot = DotScene.Instantiate<Dot>();
		GetTree().CurrentScene.AddChild(dot);
		dot.GlobalPosition = Player.GlobalPosition;
		dot.SetColor(CurrentColor);
		dot.SetVelocity(velocity);
		dot.State = DotState.Static;
		dot.Spawn();
		Queue<Dot> DotQueue = CurrentColor switch
		{
			DotlineColor.Blue => BlueDotQueue,
			DotlineColor.Red => RedDotQueue,
			DotlineColor.Purple => PurpleDotQueue,
			_ => null
		};
		DotQueue.Enqueue(dot);
		IncrementHistoryDots(CurrentColor);
		GD.Print(HistoryDots.Count);

		if (HistoryDots.Count > MaxHistoryDots)
		{
			DotlineColor? ClearColor = DecrementHistoryDots();
			if (ClearColor != null)
			{
				await ClearColorDot(ClearColor.Value);
			}
		}


	}

	public void ChangeColor(DotlineColor newColor)
	{
		CurrentColor = newColor;
	}

	public async void OnCheckPointChanged(int checkPointID)
	{
		GD.Print("Checkpoint changed, clearing dots...");
		while (_clearLock.CurrentCount == 0)
		{
			await Task.Delay(2);
		}
		GD.Print("Checkpoint changed, clearing dots...");
		await ClearDots();
	}

	// functions for test

	public void testChangeColor()
	{
		CurrentColor = CurrentColor switch
		{
			DotlineColor.White => DotlineColor.Blue,
			DotlineColor.Blue => DotlineColor.Red,
			DotlineColor.Red => DotlineColor.Purple,
			DotlineColor.Purple => DotlineColor.White,
			_ => DotlineColor.White
		};

		GD.Print("Current Color: " + CurrentColor);
	}
	public override async void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.E)
			{
				testChangeColor();
			}
			if (keyEvent.Keycode == Key.B)
			{
				if (_clearLock.CurrentCount == 0)
				{
					GD.Print("Currently clearing dots, cannot clear another color now.");
					return;
				}
				await ClearDots();
				await GameManager.Instance.ReturnToLastCheckpoint();
			}
			if (keyEvent.Keycode == Key.R)
			{
				if (_clearLock.CurrentCount == 0)
				{
					GD.Print("Currently clearing dots, cannot clear another color now.");
					return;
				}
				if (Player.StateTree._currentState.Name == "PurpleAffected")
				{
					GD.Print("Cannot clear dots while in PurpleAffected state.");
					return;
				}
				DotlineColor? ClearColor = HistoryDots.Count > 0 ? DecrementHistoryDots() : null;
				if (ClearColor == null)
				{
					GD.Print("No history dots to clear.");
					return;
				}
				await ClearColorDot(ClearColor.Value);
			}
			//GD.Print("Key pressed: " + keyEvent.Keycode);

		}



		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			//GD.Print("Mouse clicked at: " + mouseEvent.Position);
			//SpawnDot();
		}

	}
}
