using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
	
	private List<Card> playerTableCards;
	private List<Card> enemyTableCards;
	
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
		var enemyHandInfo = enemy.GetHandCards();
		var enemyTableInfo = enemy.GetTableCards();
		
		// loads player decks as hand (will be replaced with deck builder scene)
		var playerHandInfo = GlobalState.Instance.GetHandCards();
		var playerTableInfo = GlobalState.Instance.GetTableCards();
		
		enemyHand.Init(cardScene, yEnemyHand, 0.75f, false, enemyHandInfo);
		table.Init(cardScene, playerTableInfo, enemyTableInfo);
		playerHand.Init(cardScene, yPlayerHand, 1.5f, true, playerHandInfo);
		
		playerTableCards = table.GetPlayerCards();
		enemyTableCards = table.GetEnemyCards();
		
		enemy.Init(enemyHand.GetCards(), playerTableCards, enemyTableCards);
		
		playerHand.Connect(Hand.SignalName.ActiveCard, new Callable(this, nameof(UpdateActiveHandCard)));
		table.Connect(CardTableContainer.SignalName.ActiveCard, new Callable(this, nameof(UpdateActiveTableCard)));
		
		anim.Play($"agent_{GlobalState.Instance.GetDay()}");

		var playerFirst = playerHand.GetCards()[0];
		var tableFirst = enemyTableCards[0];
		
		if (GlobalState.Instance.GetDay() == 0) {
			playerHand.restrictAllow = new HashSet<Card> { playerFirst };
			table.restrictAllow = new HashSet<Card> { tableFirst };
		}
		
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		
		await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/start", true);
		
		if (GlobalState.Instance.GetDay() == 0) {
			playerFirst.Focus();
			tableFirst.Focus();
		}
	}
	
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("info")) {
			RulebookToggle();
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
		if (!active && allowThrow) {
			if (!playerHand.restrictAllow.Contains(playerHand.activeCard)) {
				playerHand.RemoveCard(playerHand.activeCard);
				playerHand.activeCard = null;
			}
			table.activeCard.locked = false;
			table.activeCard.Unhighlight();
			table.activeCard = null;
	
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
	
	private async Task ThrowCard(Card throwingCard, List<Card> tableCards) {
		int throwingCardType = GlobalState.Instance.TypeMap[throwingCard.type];
		int tableCardType;
		double threshold;
		double rnd;
		List<Card> active = tableCards[0].isPlayer ? playerTableCards : enemyTableCards;
		
		if (throwingCard.clas == "ceramic") {
			if (tableCards[0].index > 0) {
				tableCards.Add(active[tableCards[0].index - 1]);
			}
			if (tableCards[0].index < 5) {
				tableCards.Add(active[tableCards[0].index + 1]);
			}
		} else if (throwingCard.clas == "elastic" && throwingCard.isPlayer == true) {
			if (playerHand.restrictAllow.Contains(throwingCard)) {
				playerHand.restrictAllow.Remove(throwingCard);
			} else {
				playerHand.restrictAllow.Add(throwingCard);
			}
		} else if (throwingCard.clas == "vision") {
			tableCards[0].Flip();
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			tableCards[0].Flip();
			return;
		} else if (throwingCard.clas == "defense") {
			tableCards[0].ReduceDurability();
			tableCards[0].Shake();
			return;
		}
		
		for (int i = 0; i < tableCards.Count; i++) {
			tableCardType = GlobalState.Instance.TypeMap[tableCards[i].type];
			threshold = GlobalState.Instance.FlipProb[FlipRank[throwingCardType][tableCardType]] * tableCards[i].durability;
			rnd = Rand.NextDouble();
			if (GlobalState.Instance.GetDay() == 0 && round == 0) {
				rnd = 0;
			}
			
			if (i != 0) threshold *= GlobalState.Instance.CeramicProb;
			if (throwingCard.clas == "elastic") threshold *= GlobalState.Instance.ElasticProb;
			if (tableCards[i].clas == "defense") threshold *= GlobalState.Instance.DefenseProb;
			
			if (rnd < threshold) {
				tableCards[i].Flip();
				tableCards[i].ReduceDurability();
				
				if (tableCards[i].clas == "ceramic") {
					if (tableCards[i].index > 0) {
						active[tableCards[i].index - 1].ReduceDurability();
					}
					if (tableCards[i].index < 5) {
						active[tableCards[i].index + 1].ReduceDurability();
					}
				} else if (tableCards[i].clas == "elastic") {
					List<Card> unFlippedCards =
						(throwingCard.isPlayer ? playerTableCards : enemyTableCards)
						.Where(x => !x.visible).ToList();
					
					List<Card> eTableCards = new List<Card> {unFlippedCards[Rand.Next(unFlippedCards.Count)]};
					await ThrowCard(tableCards[i], eTableCards);
				}
			} else {
				tableCards[i].Shake();
			}
		}
	}
	
	private async void Round() {
		if (!allowThrow) return;
		if (GlobalState.Instance.GetDay() == 0 && round == 0) {
			playerHand.restrictAllow.Clear();
			table.restrictAllow.Clear();
			table.activeCard.Unfocus();
		}

		// player turn
		playerCardThrow.Visible = true;
		playerCardThrow.Play("player_card_throw");
		await ThrowCard(playerHand.activeCard, new List<Card> { table.activeCard });
		ThrowToggle(false);
		
		// second throw with elastic card
		if (playerHand.restrictAllow.Contains(playerHand.activeCard)) {
			return;
		}
		
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		
		// enemy turn
		var (throwingCard, tableCard1, tableCard2) = enemy.Move();
		oppCardThrow.Visible = true;
		oppCardThrow.Play("opp_card_throw");
		ThrowCard(throwingCard, new List<Card> {tableCard1});
		if (throwingCard.clas == "elastic" || throwingCard.clas == "vision") {
			await ThrowCard(throwingCard, new List<Card> {tableCard2});
		}
		if (throwingCard.clas == "vision") {
			enemy.RevealCard(tableCard1, playerTableCards.IndexOf(tableCard1));
			enemy.RevealCard(tableCard2, playerTableCards.IndexOf(tableCard2));
		}
		enemy.Backward();
		enemyHand.RemoveCard(throwingCard);

		await ToSignal(GetTree().CreateTimer(1), "timeout");
		
		// round end
		round++;
		
		// tutorial dialogue
		if (GlobalState.Instance.GetDay() == 0) {
			switch (round) {
				case 1: case 3:
					await ToSignal(GetTree().CreateTimer(0.25), "timeout");
					await DialogueManager.Instance.StartDialogue($"agent_0/{round}", true);
					break;
				case 2:
					infoButton.Focus();
					await ToSignal(GetTree().CreateTimer(0.25), "timeout");
					await DialogueManager.Instance.StartDialogue($"agent_0/{round}", true);
					infoButton.Unfocus();
					break;
			}
		}
		
		// track player and enemy score
		int playerCount = enemyTableCards.Count(card => card.visible);
		int enemyCount = playerTableCards.Count(card => card.visible);
		
		// game end
		if (round >= playerHand.startingAmount || playerCount == 6 || enemyCount == 6) {
			if (playerCount > enemyCount) {
				roundLabel.Text = "You Win";
				GlobalState.Instance.NewInhabitant();
				await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/end", true, "win");
			} else if (playerCount < enemyCount) {
				roundLabel.Text = "You Lose";
				await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/end", true, "lose");
			} else {
				roundLabel.Text = "Tie";
				await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/end", true, "tie");
			}
			GetNode<SceneLoader>("/root/SceneLoader").ChangeToScene("safehouse.tscn");
		} else {
			if (round == 3 && (GlobalState.Instance.GetInfo().Class == "vision" || playerTableCards.Any(card => card.clas == "vision"))) {
				roundLabel.Text = $"Vision Cards Are Swapping";
				await ToSignal(GetTree().CreateTimer(0.5), "timeout");
				if (GlobalState.Instance.GetInfo().Class == "vision") {
					await SwapVision(enemyTableCards);
				} else {
					await SwapVision(playerTableCards);
				}
			}
			roundLabel.Text = $"Round {round+1}";
		}
	}
	
	private async Task SwapVision(List<Card> cards) {
		List<Vector2> start = cards.Select(card => card.Position).ToList();
		List<Vector2> end = cards.Select(card => new Vector2(-(card.sprite.Polygon[0] + card.sprite.Polygon[1]).X / 2, 0)).ToList();
		
		for (int i = 0; i < cards.Count; i++) {
			cards[i].UpdatePosition(end[i]);
		}
		
		await ToSignal(GetTree().CreateTimer(0.25), "timeout");
		
		List<Card> visionCards = cards.Where(card => card.clas == "vision").ToList();
		List<Card> nonVisionCards = cards.Where(card => card.clas != "vision").ToList();
		
		foreach (Card visionCard in visionCards) {
			Card nonVisionCard = nonVisionCards[Rand.Next(nonVisionCards.Count)];
			SwapCardFields(visionCard, nonVisionCard);
			visionCard.UpdateTexture();
			nonVisionCard.UpdateTexture();
			nonVisionCards.Remove(nonVisionCard);
		}
		
		for (int i = 0; i < cards.Count; i++) {
			cards[i].UpdatePosition(start[i]);
		}
	}
	
	void SwapCardFields(Card a, Card b) {
		(a.type, b.type) = (b.type, a.type);
		(a.clas, b.clas) = (b.clas, a.clas);
		(a.visible, b.visible) = (b.visible, a.visible);
		(a.durability, b.durability) = (b.durability, a.durability);
		(a.durabilityBar.Value, b.durabilityBar.Value) = (b.durabilityBar.Value, a.durabilityBar.Value);
	}
}
