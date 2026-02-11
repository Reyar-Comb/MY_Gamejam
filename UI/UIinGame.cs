using Godot;
using System;

public partial class UIinGame : CanvasLayer
{
	private Label DotsLabel => field ??= GetNode<Label>("%DotsLabel");
	public override void _Ready()
	{
		UpdateDotsLabel(0, DotlineManager.Instance.MaxHistoryDots);
		DotlineManager.Instance.DotsLabelRefresh += UpdateDotsLabel;
	}
	public override void _ExitTree()
	{
		DotlineManager.Instance.DotsLabelRefresh -= UpdateDotsLabel;
	}
	private void UpdateDotsLabel(int currentDots, int maxDots)
	{
		DotsLabel.Text = $"{currentDots}/{maxDots}";
	}
}
