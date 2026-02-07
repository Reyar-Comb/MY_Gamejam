using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

public partial class DotlineManager : Node2D
{
	public static DotlineManager Instance;

	[Export] public PackedScene DotScene;
	[Export] public PackedScene LineScene;

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
	[Export] public float DotVelocity = 300f;


	public Queue<Dot> BlueDotQueue { get; private set; } = new Queue<Dot>();
	public Queue<Dot> RedDotQueue { get; private set; } = new Queue<Dot>();
	public Queue<Dot> PurpleDotQueue { get; private set; } = new Queue<Dot>();
	public List<Line> BlueLines = new List<Line>();
	public List<Line> RedLines = new List<Line>();
	public List<Line> PurpleLines = new List<Line>();
	public Dot FirstBlueDot => BlueDotQueue.Count > 0 ? BlueDotQueue.Peek() : null;
	public Dot FirstRedDot => RedDotQueue.Count > 0 ? RedDotQueue.Peek() : null;
	public Dot FirstPurpleDot => PurpleDotQueue.Count > 0 ? PurpleDotQueue.Peek() : null;
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
	}

	public Vector2 GetDirection()
	{
		return (GetGlobalMousePosition() - Player.GlobalPosition).Normalized();
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
				await ToSignal(startDot.DotAnimPlayer, "animation_finished");
				break;
		}
	}

	public async Task ClearColorDot(DotlineColor color, bool DeleteDot = false)
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

		if (DeleteDot == false) await dotToClear.Clear();

		foreach (var d in DotQueue.ToArray())
		{
			d.State = DotState.Static;
			d.hasEmittedLineUp = false;
			d.hasEmittedUnline = true;
			d.CurrentLine = null;
		}
		Dot FirstDot = DotQueue.Count > 0 ? DotQueue.Peek() : null;
		FirstDot?.CurrentLines.Clear();
		LineUpDot(FirstDot);
	}
	/*
	public async Task ClearDots()
	{
		var tasks = new System.Collections.Generic.List<Task>();
		foreach (Line line in BlueLines)
			tasks.Add(line.Clear());
		foreach (Line line in RedLines)
			tasks.Add(line.Clear());
		foreach (Line line in PurpleLines)
			tasks.Add(line.Clear());

		await Task.WhenAll(tasks);
		tasks.Clear();
		
		foreach (Dot dot in BlueDotQueue)
			tasks.Add(dot.Clear());
		foreach (Dot dot in RedDotQueue)
			tasks.Add(dot.Clear());
		foreach (Dot dot in PurpleDotQueue)
			tasks.Add(dot.Clear());
		
		await Task.WhenAll(tasks);
		
		BlueDotQueue.Clear();
		RedDotQueue.Clear();
		PurpleDotQueue.Clear();
		BlueLines.Clear();
		RedLines.Clear();
		PurpleLines.Clear();
	}
	*/
	public async void SpawnDot()
	{
		if (CurrentColor == DotlineColor.White)
			return;
		Dot dot = DotScene.Instantiate<Dot>();
		GetTree().CurrentScene.AddChild(dot);
		dot.GlobalPosition = Player.GlobalPosition;
		dot.SetColor(CurrentColor);
		dot.SetVelocity(GetDirection() * DotVelocity);
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
		// dot.OnIdle += () => OnDotIdjjhhcle(dot, DotQueue);
		// TODO: Enable dot collision handling
		// dot.DotCollide += OnDotCollide;
	}
	/*
	public void OnDotIdle(Dot dot, Queue<Dot> dotQueue)
	{
		if (dotQueue.Count >= 2)
		{
			Dot FirstDot = CurrentColor switch
			{
				DotlineColor.Blue => FirstBlueDot,
				DotlineColor.Red => FirstRedDot,
				DotlineColor.Purple => FirstPurpleDot,
				_ => null
			};
			switch (CurrentColor)
			{
				case DotlineColor.Blue:
					BlueLines.Add(FirstDot.LineUp(dot));
					break;
				case DotlineColor.Red:
					RedLines.Add(FirstDot.LineUp(dot));
					break;
				case DotlineColor.Purple:
					PurpleLines.Add(FirstDot.LineUp(dot));
					break;
			}
		}
	}

	public void OnDotCollide(Dot collidedDot, Dot selfDot)
	{
		CallDeferred(MethodName.HandleDotCollide, collidedDot, selfDot);
	}
	public async void HandleDotCollide(Dot collidedDot, Dot selfDot)
	{
		DotlineColor collidedColor = collidedDot.Color;
		DotlineColor selfColor = selfDot.Color;

		if (collidedColor == selfColor)
			return;

		Queue<Dot> collidedQueue = collidedColor switch
		{
			DotlineColor.Blue => BlueDotQueue,
			DotlineColor.Red => RedDotQueue,
			DotlineColor.Purple => PurpleDotQueue,
			_ => null
		};
		Queue<Dot> selfQueue = selfColor switch
		{
			DotlineColor.Blue => BlueDotQueue,
			DotlineColor.Red => RedDotQueue,
			DotlineColor.Purple => PurpleDotQueue,
			_ => null
		};

		if (selfDot == selfQueue.Peek())
		{
			await ClearColorDot(selfColor, true);
		}
		else
		{switch (selfColor)
			{
				case DotlineColor.Blue:
					BlueLines.Remove(collidedDot.CurrentLine);
					break;
				case DotlineColor.Red:
					RedLines.Remove(collidedDot.CurrentLine);
					break;
				case DotlineColor.Purple:
					PurpleLines.Remove(collidedDot.CurrentLine);
					break;
			}
			await collidedDot.CurrentLine?.Clear();
			
			var tempList = new List<Dot>(selfQueue);
			tempList.Remove(selfDot);
			selfQueue.Clear();
			foreach (var d in tempList)
				selfQueue.Enqueue(d);
		}
		

		selfDot.SetColor(collidedColor);
		collidedQueue.Enqueue(selfDot);
		Line line = collidedQueue.Peek().LineUp(selfDot);
		switch (collidedColor)
		{
			case DotlineColor.Blue:
				BlueLines.Add(line);
				break;
			case DotlineColor.Red:
				RedLines.Add(line);
				break;
			case DotlineColor.Purple:
				PurpleLines.Add(line);
				break;
		}
		GD.Print("Dot collided: " + selfDot.Color.ToString());
	}
	*/
	public void ChangeColor(DotlineColor newColor)
	{
		CurrentColor = newColor;
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
			if (keyEvent.Keycode == Key.R)
			{
				testChangeColor();
			}
			if (keyEvent.Keycode == Key.C)
			{
				//await ClearDots();
			}
			if (keyEvent.Keycode == Key.L)
			{
				await ClearColorDot(CurrentColor);
			}
			GD.Print("Key pressed: " + keyEvent.Keycode);

		}



		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			GD.Print("Mouse clicked at: " + mouseEvent.Position);
			SpawnDot();
		}

	}
}
