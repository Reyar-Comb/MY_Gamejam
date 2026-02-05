using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class DotlineManager : Node2D
{
	public static DotlineManager Instance;

	[Export] public PackedScene DotScene;

	public DotlineColor CurrentColor {
		 get => field; 
		 set {
			field = value;
			Player.PlayerColor = value.ToString();
		} }
		
	public Player Player;
	[Export] public float DotVelocity = 200f;


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
		var tasks = new List<Task>();
		foreach (Line line in dotToClear.CurrentLines)
		{
			tasks.Add(line.Clear());
			switch (color)
			{
				case DotlineColor.Blue:
					BlueLines.Remove(line);
					break;
				case DotlineColor.Red:
					RedLines.Remove(line);
					break;
				case DotlineColor.Purple:
					PurpleLines.Remove(line);
					break;
			}
		}
		await Task.WhenAll(tasks);
		if (DeleteDot == false) await dotToClear.Clear();

		Dot FirstDot = DotQueue.Count > 0 ? DotQueue.Peek() : null;
		FirstDot?.CurrentLines.Clear();
		foreach (Dot dot in DotQueue)
		{
			if (dot != FirstDot)
			{
				Line newLine = FirstDot.LineUp(dot);
				switch (color)
				{
					case DotlineColor.Blue:
						BlueLines.Add(newLine);
						break;
					case DotlineColor.Red:
						RedLines.Add(newLine);
						break;
					case DotlineColor.Purple:
						PurpleLines.Add(newLine);
						break;
				}
			}
		}
	}
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
	public void SpawnDot()
	{
		if (CurrentColor == DotlineColor.White)
			return;
		Dot dot = DotScene.Instantiate<Dot>();
		GetTree().CurrentScene.AddChild(dot);
		dot.GlobalPosition = Player.GlobalPosition;
		dot.SetColor(CurrentColor);
		dot.SetVelocity(GetDirection() * DotVelocity);
		dot.Spawn();
		Queue<Dot> DotQueue = CurrentColor switch
		{
			DotlineColor.Blue => BlueDotQueue,
			DotlineColor.Red => RedDotQueue,
			DotlineColor.Purple => PurpleDotQueue,
			_ => null
		};
		DotQueue.Enqueue(dot);

		if (DotQueue.Count >= 2)
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
		// TODO: Enable dot collision handling
		// dot.DotCollide += OnDotCollide;
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
				await ClearDots();
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
