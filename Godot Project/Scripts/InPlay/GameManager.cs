using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node2D {
	[Export] public PackedScene cardScene;
	
	private CardHandContainer playerHand;
	private CardHandContainer enemyHand;
	private CardTableContainer table;
	
	private ThrowButton throwButton;
	private Label resultLabel;
	
	private Agent enemy;
	
	private bool allowThrow = false;	
	
	static readonly float ySpacing = 40;
	static readonly float yStart = 85;
	static readonly int[][] FlipRank = new int[][] {
		new int[]{1, 0, 2},
		new int[]{2, 1, 0},
		new int[]{0, 2, 1}
	};
	static readonly Random Rand = new Random();
	
	int round = 1;

	public override void _Ready() {
		resultLabel = GetParent().GetNode<Label>("ResultLabel");
		resultLabel.Text = "";
		
		throwButton = GetParent().GetNode<ThrowButton>("ThrowButton");
		ThrowToggle(false);
		throwButton.Connect(ThrowButton.SignalName.Pressed, new Callable(this, nameof(Round)));
		
		playerHand = new CardHandContainer();
		enemyHand = new CardHandContainer();
		table = new CardTableContainer();
		
		AddChild(playerHand);
		AddChild(enemyHand);
		AddChild(table);
		
		enemy = GlobalState.Instance.GetNextAgent();
		
		var (enemyHandTypes, enemyHandClasses) = enemy.GetHandCards();
		var (enemyTableTypes, enemyTableClasses) = enemy.GetTableCards();
		var (playerHandTypes, playerHandClasses) = GlobalState.Instance.GetHandCards();
		var (playerTableTypes, playerTableClasses) = GlobalState.Instance.GetTableCards();
		
		enemyHand.Init(cardScene, yStart, false, enemyHandTypes, enemyHandClasses);
		table.Init(cardScene, playerTableTypes, playerTableClasses, enemyTableTypes,
		enemyTableClasses, yStart + ySpacing * 2, yStart + ySpacing);
		playerHand.Init(cardScene, yStart + ySpacing * 3, true, playerHandTypes, playerHandClasses);
		
		enemy.Init(enemyHand.GetCards(), table.GetPlayerCards(), table.GetEnemyCards());
		
		playerHand.Connect(CardContainer.SignalName.ActiveCard, new Callable(this, nameof(UpdateActivePlayerHand)));
		table.Connect(CardContainer.SignalName.ActiveCard, new Callable(this, nameof(UpdateActivetable)));
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
	
	private void ThrowCard(Card throwingCard, List<Card> tableCards) {
		int throwingCardType = GlobalState.Instance.TypeMap[throwingCard.type];
		int tableCardType;
		double threshold;
		double rnd;
		List<Card> active = tableCards[0].isPlayer ? table.GetPlayerCards() : table.GetEnemyCards();
		
		if (throwingCard.clas == "ceramic") {
			if (tableCards[0].index > 0) {
				tableCards.Add(active[tableCards[0].index - 1]);
			}
			if (tableCards[0].index < 5) {
				tableCards.Add(active[tableCards[0].index + 1]);
			}
		}
		
		for (int i = 0; i < tableCards.Count; i++) {
			tableCardType = GlobalState.Instance.TypeMap[tableCards[i].type];
			threshold = GlobalState.Instance.FlipProb[FlipRank[throwingCardType][tableCardType]] * tableCards[i].durability;
			rnd = Rand.NextDouble();
			
			if (i != 0) threshold *= 0.25;
			
			if (rnd < threshold) {
				tableCards[i].Flip();
				tableCards[i].durability -= 0.2;
				
				if (tableCards[i].clas == "ceramic") {
					if (tableCards[i].index > 0) {
						active[tableCards[i].index - 1].durability -= 0.2;
					}
					if (tableCards[i].index < 5) {
						active[tableCards[i].index + 1].durability -= 0.2;
					}
				}
			}
		}
	}
	
	private async void Round() {
		if (!allowThrow) return;
		
		// player turn
		ThrowCard(playerHand.activeCard, new List<Card> {table.activeCard});		
		ThrowToggle(false);
		
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		
		// enemy turn
		var (throwingCard, tableCard) = enemy.Move();
		ThrowCard(throwingCard, new List<Card> {tableCard});
		enemy.Backward();
		enemyHand.RemoveCard(throwingCard);
		
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		
		// round end
		round++;
		int playerCount = table.GetPlayerCards().Count(card => card.visible);
		int enemyCount = table.GetEnemyCards().Count(card => card.visible);
		
		if (round > playerHand.numCards || playerCount == 6 || enemyCount == 6) {
			if (playerCount > enemyCount) {
				resultLabel.Text = "You Win";
			} else if (playerCount < enemyCount) {
				resultLabel.Text = "You Lose";
			} else {
				resultLabel.Text = "Tie";
			}
			await ToSignal(GetTree().CreateTimer(3), "timeout");
			GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
		}
	}
}
