using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node2D
{
	[Export] public NodePath playerHandPath;
	[Export] public NodePath playerTablePath;
	[Export] public NodePath buttonPath;
	
	private CardHandContainer playerHand;
	private CardTableContainer playerTable;
	private ThrowButton throwButton;
	
	private bool allowThrow = false;
	
	static readonly int[][] FlipRank = new int[][]{
		new int[]{1, 0, 2},
		new int[]{2, 1, 0},
		new int[]{0, 2, 1}
	};
	static readonly double[] FlipProb = new double[]{0.1, 0.5, 0.9};
	private Random rand = new Random();
	static readonly Dictionary<string, int> TypeMap = new()
	{
		{ "light", 0 },
		{ "regular", 1 },
		{ "heavy", 2 }
	};
	
	int round = 1;

	public override void _Ready()
	{
		throwButton = GetNode<ThrowButton>(buttonPath);
		throwToggle(false);
		throwButton.Connect(ThrowButton.SignalName.Pressed, new Callable(this, nameof(throwCard)));
		
		playerHand = GetNode<CardHandContainer>(playerHandPath);
		playerTable = GetNode<CardTableContainer>(playerTablePath);
		
		playerHand.Connect(CardContainer.SignalName.ActiveCard, new Callable(this, nameof(updateActivePlayerHand)));
		playerTable.Connect(CardContainer.SignalName.ActiveCard, new Callable(this, nameof(updateActivePlayerTable)));
	}
	
	public void throwToggle(bool active) {
		bool res = active && playerTable.activeCard != null && playerHand.activeCard != null;
		if (!active && allowThrow) {
			playerHand.RemoveCard(playerHand.activeCard);
			playerTable.activeCard.locked = false;
			playerTable.activeCard.Unhighlight();
			playerTable.activeCard = playerHand.activeCard = null;
		}
		allowThrow = res;
		throwButton.Disabled = !res;
		throwButton.Modulate = res ? Colors.White : new Color(1, 1, 1, 0.4f);
	}
	
	public void updateActivePlayerHand(Card card) {
		playerHand.activeCard = card;
		throwToggle(true);
	}
	
	public void updateActivePlayerTable(Card card) {
		playerTable.activeCard = card;
		throwToggle(true);
	}
	
	private void throwCard() {
		if (!allowThrow) return;
		int throwingCard = TypeMap[playerHand.activeCard.type];
		int tableCard = TypeMap[playerTable.activeCard.type];
		double threshold = FlipProb[FlipRank[throwingCard][tableCard]];
		double rnd = rand.NextDouble();
		if (rnd < threshold) {
			playerTable.activeCard.Reveal();
		} else {
		}
		throwToggle(false);
		
		round++;
		if (round > playerHand.numCards) {
			GD.Print("game over");
		}
	}
}
