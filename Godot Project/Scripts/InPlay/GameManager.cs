using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node2D {
	[Export] public PackedScene cardScene;
	
	private Hand playerHand;
	private Hand enemyHand;
	private CardTableContainer table;
	private AnimatedSprite2D anim;
	
	private ThrowButton throwButton;
	
	private Agent enemy;
	
	private bool allowThrow = false;	
	
	static readonly int yEnemyHand = -140;
	static readonly int yPlayerHand = 90;
	static readonly int[][] FlipRank = new int[][] {
		new int[]{1, 0, 2},
		new int[]{2, 1, 0},
		new int[]{0, 2, 1}
	};
	static readonly Random Rand = new Random();
	
	int round = 0;

	public async override void _Ready() {
		anim = GetParent().GetNode<AnimatedSprite2D>("AnimatedSprite2D");
				
		throwButton = GetParent().GetNode<ThrowButton>("ThrowButton");
		ThrowToggle(false);
		throwButton.Connect(ThrowButton.SignalName.Pressed, new Callable(this, nameof(Round)));
		
		playerHand = new Hand();
		enemyHand = new Hand();
		table = new CardTableContainer();
		
		AddChild(playerHand);
		AddChild(enemyHand);
		AddChild(table);
		
		enemy = GlobalState.Instance.GetNextAgent();
		
		var (enemyHandTypes, enemyHandClasses) = enemy.GetHandCards();
		var (enemyTableTypes, enemyTableClasses) = enemy.GetTableCards();
		var (playerHandTypes, playerHandClasses) = GlobalState.Instance.GetHandCards();
		var playerTableClasses = GlobalState.Instance.GetTableClasses();
		var playerTableTypes = new List<string> {
			"tsunami", "volcano", "earthquake",
			"tsunami", "volcano", "earthquake"
		};
		
		enemyHand.Init(cardScene, yEnemyHand, 1, false, enemyHandTypes, enemyHandClasses);
		table.Init(cardScene, playerTableTypes, playerTableClasses, enemyTableTypes,
		enemyTableClasses);
		playerHand.Init(cardScene, yPlayerHand, 1.5f, true, playerHandTypes, playerHandClasses);
		
		enemy.Init(enemyHand.GetCards(), table.GetPlayerCards(), table.GetEnemyCards());
		
		playerHand.Connect(Hand.SignalName.ActiveCard, new Callable(this, nameof(UpdateActivePlayerHand)));
		table.Connect(CardTableContainer.SignalName.ActiveCard, new Callable(this, nameof(UpdateActivetable)));
		
		anim.Play($"agent_{GlobalState.Instance.GetDay()}");

		await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/start");

		if (GlobalState.Instance.GetDay() == 0)
		{
			playerHand.OnCardClicked(playerHand.GetCards()[0]);
			table.OnCardClicked(table.GetEnemyCards()[0]);
			playerHand.allowActive = table.allowActive = false;
		} 
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
			if (GlobalState.Instance.GetDay() == 0 && round == 0) {
				rnd = 0;
			}
			
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
		
		// round end
		round++;
		
		if (GlobalState.Instance.GetDay() == 0 && (round == 1 || round == 3)) {
			await ToSignal(GetTree().CreateTimer(0.5), "timeout");
			playerHand.allowActive = table.allowActive = true;
			await DialogueManager.Instance.StartDialogue($"agent_0/{round}");
		}
		int playerCount = table.GetEnemyCards().Count(card => card.visible);
		int enemyCount = table.GetPlayerCards().Count(card => card.visible);
		
		if (round >= playerHand.startingAmount || playerCount == 6 || enemyCount == 6) {
			if (playerCount > enemyCount) {
				await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/end", "win");
			} else if (playerCount < enemyCount) {
				await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/end", "lose");
			} else {
				await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/end", "tie");
			}
			GlobalState.Instance.NextDay();
			GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
		}
	}
}
