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
	private AnimatedSprite2D playerCardThrow;
	private AnimatedSprite2D oppCardThrow;
	
	private BetterButton throwButton;
	private BetterButton infoButton;
	private Panel rulebook;
	private Label roundLabel;
	
	private Agent enemy;
	
	private bool allowThrow = false;
	
	// y values for hand positions
	private readonly int yEnemyHand = -140;
	private readonly int yPlayerHand = 90;
	
	// relationship between card types
	private readonly int[][] FlipRank = new int[][] {
		new int[]{1, 0, 2},
		new int[]{2, 1, 0},
		new int[]{0, 2, 1}
	};
	private readonly Random Rand = new Random();
	
	int round = 0;

	public async override void _Ready() {
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		playerCardThrow = GetNode<AnimatedSprite2D>("Player_CardThrowAnimate");
		oppCardThrow = GetNode<AnimatedSprite2D>("Opp_CardThrowAnimate");
		throwButton = GetNode<BetterButton>("ThrowButton");
		infoButton = GetNode<BetterButton>("InfoButton");
		rulebook = GetNode<Panel>("Rulebook");
		roundLabel = GetNode<Label>("RoundLabel");
		
		ThrowToggle(false);
		throwButton.Connect(BetterButton.SignalName.Pressed, new Callable(this, nameof(Round)));
		infoButton.Connect(BetterButton.SignalName.Pressed, new Callable(this, nameof(RulebookToggle)));
		
		playerHand = new Hand();
		enemyHand = new Hand();
		table = new CardTableContainer();
		
		AddChild(playerHand);
		AddChild(enemyHand);
		AddChild(table);
		
		enemy = GlobalState.Instance.GetNextAgent();
		
		// loads enemy hand/table
		var (enemyHandTypes, enemyHandClasses) = enemy.GetHandCards();
		var (enemyTableTypes, enemyTableClasses) = enemy.GetTableCards();
		
		// loads player decks as hand (will be replaced with deck builder scene)
		var (playerHandTypes, playerHandClasses) = GlobalState.Instance.GetHandCards();
		var playerTableClasses = GlobalState.Instance.GetTableClasses();
		var playerTableTypes = new List<string> {
			"tsunami", "volcano", "earthquake",
			"tsunami", "volcano", "earthquake"
		};
		
		enemyHand.Init(cardScene, yEnemyHand, 0.75f, false, enemyHandTypes, enemyHandClasses);
		table.Init(cardScene, playerTableTypes, playerTableClasses, enemyTableTypes,
		enemyTableClasses);
		playerHand.Init(cardScene, yPlayerHand, 1.125f, true, playerHandTypes, playerHandClasses);
		
		enemy.Init(enemyHand.GetCards(), table.GetPlayerCards(), table.GetEnemyCards());
		
		playerHand.Connect(Hand.SignalName.ActiveCard, new Callable(this, nameof(UpdateActiveHandCard)));
		table.Connect(CardTableContainer.SignalName.ActiveCard, new Callable(this, nameof(UpdateActiveTableCard)));
		
		anim.Play($"agent_{GlobalState.Instance.GetDay()}");

		var playerFirst = playerHand.GetCards()[0];
		var tableFirst = table.GetEnemyCards()[0];
		
		if (GlobalState.Instance.GetDay() == 0) {
			playerHand.restrictAllow = new HashSet<Card> { playerFirst };
			table.restrictAllow = new HashSet<Card> { tableFirst };
		}
		
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		
		await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/start");
		
		if (GlobalState.Instance.GetDay() == 0) {
			playerFirst.Focus();
			tableFirst.Focus();
		}
	}
	
	public void RulebookToggle() {
		if (rulebook.Visible) {
			rulebook.Visible = false;
			var sprite = GetNode<Sprite2D>("InfoButton/Image");
			var texture = GD.Load<Texture2D>("res://Assets/In Play Safe House/info_button.png");
			sprite.Texture = texture;
		} else {
			rulebook.Visible = true;
			var sprite = GetNode<Sprite2D>("InfoButton/Image");
			var texture = GD.Load<Texture2D>("res://Assets/In Play Safe House/x.png");
			sprite.Texture = texture;
		}
	}
	
	public void ThrowToggle(bool active) {
		bool res = active && table.activeCard != null && playerHand.activeCard != null;
		if (!active && allowThrow)
		{
			playerHand.RemoveCard(playerHand.activeCard);
			table.activeCard.locked = false;
			table.activeCard.Unhighlight();
			table.activeCard = playerHand.activeCard = null;
			playerCardThrow.Visible = true;
			playerCardThrow.Play("player_card_throw");			
		}
		allowThrow = res;
		throwButton.Disabled = !res;
		throwButton.Modulate = res ? Colors.White : new Color(1, 1, 1, 0.4f);
	}
	
	public void UpdateActiveHandCard(Card card) {
		playerHand.activeCard = card;
		ThrowToggle(true);
	}
	
	public void UpdateActiveTableCard(Card card) {
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
				
				if (tableCards[i].clas == "ceramic") {
					if (tableCards[i].index > 0) {
						active[tableCards[i].index - 1].ReduceDurability();
					}
					if (tableCards[i].index < 5) {
						active[tableCards[i].index + 1].ReduceDurability();
					}
				}
			}
		}
	}
	
	private async void Round() {
		if (!allowThrow) return;
		table.activeCard.Unfocus();
		
		// player turn
		ThrowCard(playerHand.activeCard, new List<Card> {table.activeCard});
		ThrowToggle(false);
		
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		
		// enemy turn
		var (throwingCard, tableCard) = enemy.Move();
		ThrowCard(throwingCard, new List<Card> {tableCard});
		enemy.Backward();
		enemyHand.RemoveCard(throwingCard);
		oppCardThrow.Visible = true;
		oppCardThrow.Play("opp_card_throw");
		
		// round end
		round++;
		playerHand.restrictAllow = table.restrictAllow = null;
		
		// tutorial dialogue
		if (GlobalState.Instance.GetDay() == 0) {
			switch (round) {
				case 1: case 3:
					await ToSignal(GetTree().CreateTimer(0.25), "timeout");
					await DialogueManager.Instance.StartDialogue($"agent_0/{round}");
					break;
				case 2:
					infoButton.Focus();
					await ToSignal(GetTree().CreateTimer(0.25), "timeout");
					await DialogueManager.Instance.StartDialogue($"agent_0/{round}");
					infoButton.Unfocus();
					break;
			}
		}
		
		// track player and enemy score
		int playerCount = table.GetEnemyCards().Count(card => card.visible);
		int enemyCount = table.GetPlayerCards().Count(card => card.visible);
		
		// game end
		if (round >= playerHand.startingAmount || playerCount == 6 || enemyCount == 6) {
			if (playerCount > enemyCount) {
				roundLabel.Text = "You Win";
				await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/end", "win");
			} else if (playerCount < enemyCount) {
				roundLabel.Text = "You Lose";
				await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/end", "lose");
			} else {
				roundLabel.Text = "Tie";
				await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/end", "tie");
			}
			GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
		} else {
			roundLabel.Text = $"Round {round+1}";
		}
	}
}
