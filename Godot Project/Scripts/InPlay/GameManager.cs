using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node2D {
	[Export] public NodePath buttonPath;
	[Export] public PackedScene cardScene;
	
	private CardHandContainer playerHand;
	private CardHandContainer enemyHand;
	private CardTableContainer table;
	
	private ThrowButton throwButton;
	
	private Agent agent;
	
	private bool allowThrow = false;
	
	static readonly float ySpacing = 40;
	static readonly float yStart = 85;
	static readonly int[][] FlipRank = new int[][] {
		new int[]{1, 0, 2},
		new int[]{2, 1, 0},
		new int[]{0, 2, 1}
	};
	static readonly double[] FlipProb = new double[]{0.1, 0.5, 0.9};
	static readonly Random Rand = new Random();
	static readonly Dictionary<string, int> TypeMap = new() {
		{ "light", 0 },
		{ "regular", 1 },
		{ "heavy", 2 }
	};
	
	int round = 1;

	public override void _Ready() {
		throwButton = GetNode<ThrowButton>(buttonPath);
		ThrowToggle(false);
		throwButton.Connect(ThrowButton.SignalName.Pressed, new Callable(this, nameof(ThrowCard)));
		
		playerHand = new CardHandContainer();
		enemyHand = new CardHandContainer();
		table = new CardTableContainer();
		
		AddChild(playerHand);
		AddChild(enemyHand);
		AddChild(table);
		
		enemyHand.Init(cardScene, yStart, false);
		table.Init(cardScene, yStart + ySpacing, yStart + ySpacing * 2);
		playerHand.Init(cardScene, yStart + ySpacing * 3, true);
		
		playerHand.Connect(CardContainer.SignalName.ActiveCard, new Callable(this, nameof(UpdateActivePlayerHand)));
		table.Connect(CardContainer.SignalName.ActiveCard, new Callable(this, nameof(UpdateActivetable)));
		
		agent = new Agent0();
		agent.Init();
	}
	
	public void ThrowToggle(bool active) {
		bool res = active && table.activeCard != null && playerHand.activeCard != null;
		if (!active && allowThrow) {
			playerHand.RemoveCard(playerHand.activeCard);
			table.activeCard.locked = false;
			table.activeCard.Unhighlight();
			table.activeCard = playerHand.activeCard = null;
		}
		allowThrow = res;
		throwButton.Disabled = !res;
		throwButton.Modulate = res ? Colors.White : new Color(1, 1, 1, 0.4f);
	}
	
	public void UpdateActivePlayerHand(Card card) {
		playerHand.activeCard = card;
		ThrowToggle(true);
	}
	
	public void UpdateActivetable(Card card) {
		table.activeCard = card;
		ThrowToggle(true);
	}
	
	private async void ThrowCard() {
		if (!allowThrow) return;
		double rnd = Rand.NextDouble();
		
		// player turn
		int throwingCardType = TypeMap[playerHand.activeCard.type];
		int tableCardType = TypeMap[table.activeCard.type];
		double threshold = FlipProb[FlipRank[throwingCardType][tableCardType]] * table.activeCard.durability;
		if (rnd < threshold) {
			table.activeCard.Flip();
			table.activeCard.durability -= 0.2;
		}
		
		ThrowToggle(false);
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		
		// agent turn
		
		var (throwingCard, tableCardIdx) = agent.Move(enemyHand.GetCards());
		Card tableCard = table.GetEnemyCards()[tableCardIdx];
		throwingCardType = TypeMap[throwingCard.type];
		tableCardType = TypeMap[tableCard.type];
		threshold = FlipProb[FlipRank[throwingCardType][tableCardType]] * tableCard.durability;
		
		List<int> indices = new List<int>();
		List<int> types = new List<int>();
		
		if (rnd < threshold) {
			tableCard.Flip();
			indices.Add(tableCardIdx);
			indices.Add(tableCardType);
			tableCard.durability -= 0.2;
		}
		agent.Backward(indices, types);
		enemyHand.RemoveCard(throwingCard);
		
		// round end
		
		round++;
		int playerCount = table.GetPlayerCards().Count(card => card.visible);
		int enemyCount = table.GetEnemyCards().Count(card => card.visible);
		
		if (round > playerHand.numCards || playerCount == 6 || enemyCount == 6) {
			GD.Print("game over");
			if (playerCount > enemyCount) {
				GD.Print("player wins");
			} else if (playerCount < enemyCount) {
				GD.Print("enemy wins");
			} else {
				GD.Print("tie");
			}
		}
	}
}
