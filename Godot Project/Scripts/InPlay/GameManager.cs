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
	
	private string omamori;
	private TextureButton omamoriButton;
	
	// y values for hand positions
	private int yEnemyHand = -140;
	private int yPlayerHand = 90;
	
	bool blindEnemy = false;
	
	private int lastFortune = -1;
	
	// relationship between card types
	private readonly int[][] FlipRank = new int[][] {
		new int[]{1, 0, 2},
		new int[]{2, 1, 0},
		new int[]{0, 2, 1}
	};
	private readonly Random Rand = new Random();
	
	int round = 0;

	public async override void _Ready() {
		GlobalState.Instance.allowCardSelect = false;
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		playerCardThrow = GetNode<AnimatedSprite2D>("Player_CardThrowAnimate");
		oppCardThrow = GetNode<AnimatedSprite2D>("Opp_CardThrowAnimate");
		throwButton = GetNode<BetterButton>("ThrowButton");
		infoButton = GetNode<BetterButton>("InfoButton");
		rulebook = GetNode<Panel>("Rulebook");
		roundLabel = GetNode<Label>("RoundLabel");
		omamoriButton = GetNode<TextureButton>("OmamoriBox/OmamoriButton");
		
		ThrowToggle(false);
		throwButton.Connect(BetterButton.SignalName.Pressed, new Callable(this, nameof(Round)));
		infoButton.Connect(BetterButton.SignalName.Pressed, new Callable(this, nameof(RulebookToggle)));
		
		playerHand = new Hand();
		enemyHand = new Hand();
		table = new CardTableContainer();
		
		omamori = GlobalState.Instance.GetOmamori();
		var texture = GD.Load<Texture2D>($"res://Assets/Omamori/{omamori}.png");
		omamoriButton.TextureNormal = texture;
		
		var hoverOverlay = omamoriButton.GetNode<ColorRect>("HoverOverlay");
		hoverOverlay.MouseEntered += () => hoverOverlay.Visible = true;
		hoverOverlay.MouseExited += () => hoverOverlay.Visible = false;
		
		AddChild(playerHand);
		AddChild(enemyHand);
		AddChild(table);
		
		enemy = GlobalState.Instance.GetNextAgent();
		
		// loads enemy hand/table
		var enemyHandInfo = enemy.GetHandCards();
		var enemyTableInfo = enemy.GetTableCards();
		
		var playerHandInfo = GlobalState.Instance.GetHandCards();
		var playerTableInfo = GlobalState.Instance.GetTableCards();
		
		enemyHand.Init(cardScene, yEnemyHand, 0.75f, false, enemyHandInfo);
		table.Init(cardScene, playerTableInfo, enemyTableInfo, GlobalState.Instance.GetDay() != 0);
		playerHand.Init(cardScene, yPlayerHand, 1.5f, true, playerHandInfo);
		
		playerTableCards = table.GetPlayerCards();
		enemyTableCards = table.GetEnemyCards();
		
		enemy.Init(enemyHand.GetCards(), playerTableCards, enemyTableCards);
		
		playerHand.Connect(Hand.SignalName.ActiveCard, new Callable(this, nameof(UpdateActiveHandCard)));
		table.Connect(CardTableContainer.SignalName.ActiveCard, new Callable(this, nameof(UpdateActiveTableCard)));
		
		switch (omamori) {
			case "none":
				GetNode<TextureRect>("OmamoriBox").Visible = false;
				break;
			case "aftershock": case "stone_cast": case "fortune_slip":
				omamoriButton.Disabled = true;
				omamoriButton.Modulate = new Color(1, 1, 1, 0.4f);
				break;
			case "bag_of_sand":
				omamoriButton.Pressed += BagOfSandOmamori;
				break;
		}
		
		anim.Play($"agent_{GlobalState.Instance.GetDay()}");

		var playerFirst = playerHand.GetCards()[0];
		var tableFirst = enemyTableCards[0];
		
		await ToSignal(GetTree().CreateTimer(2), "timeout");
		
		await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/start", true);
		
		if (GlobalState.Instance.GetDay() == 0) {
			playerHand.restrictAllow = new HashSet<Card> { playerFirst };
			table.restrictAllow = new HashSet<Card> { tableFirst };
			
			try {
				playerFirst.Focus();
				tableFirst.Focus();
			} catch (Exception e) {
				GD.Print(e);
			}
		}
		
		GlobalState.Instance.allowCardSelect = true;
		
		await NextRound();
	}
	
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("info")) {
			RulebookToggle();
		}
	}
	
	private async Task NextRound() {
		if (omamori == "fortune_slip") {
			await FortuneSlipOmamori();
		}
	}
	
	private async Task FortuneSlipOmamori() {
		lastFortune = Rand.Next(4);
		await DialogueManager.Instance.StartDialogue($"FortuneSlips/{lastFortune}", true);
		
		if (lastFortune == 3) {
			var unflippedTableCards = enemyTableCards.Where(x => !x.visible).ToList();
			Card card = unflippedTableCards[Rand.Next(unflippedTableCards.Count)];
			card.Flip();
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			card.Flip();
		}
	}
	
	private void BagOfSandOmamori() {
		blindEnemy = true;
		omamoriButton.Disabled = true;
		omamoriButton.Modulate = new Color(1, 1, 1, 0.4f);
	}
	
	private void RulebookToggle() {
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
	
	private void ThrowToggle(bool active) {
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
		if (res && GlobalState.Instance.GetDay() == 0 && round == 0) {
			throwButton.Focus();
		}
	}
	
	private void UpdateActiveHandCard(Card card) {
		card.Unfocus();
		playerHand.activeCard = card;
		ThrowToggle(true);
	}
	
	private void UpdateActiveTableCard(Card card) {
		card.Unfocus();
		table.activeCard = card;
		ThrowToggle(true);
	}
	
	private async Task ThrowCard(Card throwingCard, List<Card> tableCards, bool fromTable) {
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
		} else if (throwingCard.clas == "elastic" && throwingCard.isPlayer == true && !fromTable) {
			if (playerHand.restrictAllow.Contains(throwingCard)) {
				playerHand.restrictAllow.Clear();
			} else {
				playerHand.restrictAllow.Add(throwingCard);
			}
		} else if (throwingCard.clas == "vision") {
			if (throwingCard.isPlayer == true && !fromTable) {
				if (playerHand.restrictAllow.Contains(throwingCard)) {
					playerHand.restrictAllow.Clear();
				} else {
					playerHand.restrictAllow.Add(throwingCard);
				}
			}
			tableCards[0].Flip();
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			tableCards[0].Flip();
			return;
		} else if (throwingCard.clas == "defense") {
			tableCards[0].ReduceDurability(0.2);
			tableCards[0].Shake();
			return;
		}
		
		for (int i = 0; i < tableCards.Count; i++) {
			tableCardType = GlobalState.Instance.TypeMap[tableCards[i].type];
			threshold = GlobalState.Instance.FlipProb[FlipRank[throwingCardType][tableCardType]];
			
			if (lastFortune != 2) {
				threshold *= tableCards[i].durability;
			}
			
			rnd = Rand.NextDouble();
			if (GlobalState.Instance.GetDay() == 0) {
				if (round == 0 || (round == 2 && throwingCard.isPlayer)) {
					rnd = 0;
				} else if ((round == 1 && throwingCard.isPlayer) ||
					(!throwingCard.isPlayer &&
					playerTableCards.Count(card => card.visible) >=
					enemyTableCards.Count(card => card.visible) - 1)
				) {
					rnd = 100;
				}
			}
			
			if (i != 0) threshold *= GlobalState.Instance.CeramicProb;
			if (throwingCard.clas == "elastic") threshold *= GlobalState.Instance.ElasticProb;
			if (tableCards[i].clas == "defense") threshold *= GlobalState.Instance.DefenseProb;
			
			if (throwingCard.isPlayer && omamori == "aftershock" && tableCards[i].lastRound + 1 == round) {
				threshold *= (tableCards[i].repeatMult *= 1.2);
			} else {
				tableCards[i].repeatMult = 1;
			}
			
			if (lastFortune == 0) {
				threshold += 0.05;
			} else if (lastFortune == 1) {
				threshold -= 0.05;
			}

			tableCards[i].lastRound = round;
			
			if (rnd <= threshold) {
				tableCards[i].Flip();
				tableCards[i].ReduceDurability(0.15);
				
				if (tableCards[i].clas == "ceramic") {
					if (tableCards[i].index > 0) {
						active[tableCards[i].index - 1].ReduceDurability(0.1);
					}
					if (tableCards[i].index < 5) {
						active[tableCards[i].index + 1].ReduceDurability(0.1);
					}
				} else if (tableCards[i].clas == "elastic") {
					List<Card> unFlippedCards =
						(throwingCard.isPlayer ? playerTableCards : enemyTableCards)
						.Where(x => !x.visible).ToList();
					
					List<Card> eTableCards = new List<Card> {unFlippedCards[Rand.Next(unFlippedCards.Count)]};
					await ThrowCard(tableCards[i], eTableCards, true);
				}
			} else {
				tableCards[i].Shake();
				
				if (!throwingCard.isPlayer && omamori == "stone_cast") {
					tableCards[i].ReduceDurability(0.05);
				}
			}
		}
	}
	
	private async void Round() {
		if (!allowThrow) return;
		GlobalState.Instance.allowCardSelect = false;
		if (GlobalState.Instance.GetDay() == 0 && round < 3) {
			playerHand.restrictAllow.Clear();
			table.restrictAllow.Clear();
			if (round == 0) {
				throwButton.Unfocus();
			}
		}
		
		// player turn
		playerCardThrow.Visible = true;
		playerCardThrow.Play("player_card_throw");
		await ThrowCard(playerHand.activeCard, new List<Card> { table.activeCard }, false);
		ThrowToggle(false);
		
		// second throw with elastic or vision card
		if (playerHand.restrictAllow.Contains(playerHand.activeCard)) {
			GlobalState.Instance.allowCardSelect = true;
			return;
		}
		
		await ToSignal(GetTree().CreateTimer(0.5), "timeout");
		
		// enemy turn
		
		var (throwingCard, tableCard1, tableCard2) = enemy.Move(blindEnemy);
		blindEnemy = false;
		oppCardThrow.Visible = true;
		oppCardThrow.Play("opp_card_throw");
		if (throwingCard.clas == "elastic" || throwingCard.clas == "vision") {
			ThrowCard(throwingCard, new List<Card> {tableCard1}, false);
			await ThrowCard(throwingCard, new List<Card> {tableCard2}, false);
		} else {
			await ThrowCard(throwingCard, new List<Card> {tableCard1}, false);
		}
		if (throwingCard.clas == "vision") {
			enemy.RevealCard(tableCard1, playerTableCards.IndexOf(tableCard1));
			enemy.RevealCard(tableCard2, playerTableCards.IndexOf(tableCard2));
		}
		enemy.Backward();
		enemyHand.RemoveCard(throwingCard);

		await ToSignal(GetTree().CreateTimer(0.25), "timeout");
		
		// round end
		round++;
		
		// track player and enemy score
		int playerCount = enemyTableCards.Count(card => card.visible);
		int enemyCount = playerTableCards.Count(card => card.visible);

		// game end
		GlobalState.Instance.NewInhabitant();
		if (round >= playerHand.startingAmount || playerCount == 6 || enemyCount == 6) {
			if (playerCount > enemyCount) {
				roundLabel.Text = "You Win";
				await DialogueManager.Instance.StartDialogue($"agent_{GlobalState.Instance.GetDay()}/end", true, "win");
			}
			else if (playerCount < enemyCount) {
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
			roundLabel.Text = $"Round {round + 1}";
		}
		
		// tutorial dialogue
		if (GlobalState.Instance.GetDay() == 0) {
			Card playerCard;
			Card tableCard;
			
			switch (round) {
				case 1:
					await DialogueManager.Instance.StartDialogue($"agent_0/{round}", true);
					playerCard = playerHand.GetCards()[0];
					tableCard = enemyTableCards[1];
					
					playerHand.restrictAllow = new HashSet<Card> { playerCard };
					table.restrictAllow = new HashSet<Card> { tableCard };
					
					playerCard.Focus();
					tableCard.Focus();
					break;
					
				case 2:
					await DialogueManager.Instance.StartDialogue($"agent_0/{round}", true);
					playerCard = playerHand.GetCards()[0];
					tableCard = playerTableCards[0];
					
					playerHand.restrictAllow = new HashSet<Card> { playerCard };
					table.restrictAllow = new HashSet<Card> { tableCard };
					
					playerCard.Focus();
					tableCard.Focus();
					break;
					
				case 3:
					infoButton.Focus();
					await DialogueManager.Instance.StartDialogue($"agent_0/{round}", true);
					infoButton.Unfocus();
					break;
			}
		}
		
		// next round start
		
		lastFortune = -1;
		
		await NextRound();
		GlobalState.Instance.allowCardSelect = true;
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
	
	private void SwapCardFields(Card a, Card b) {
		(a.type, b.type) = (b.type, a.type);
		(a.clas, b.clas) = (b.clas, a.clas);
		(a.visible, b.visible) = (b.visible, a.visible);
		(a.durability, b.durability) = (b.durability, a.durability);
		(a.durabilityBar.Value, b.durabilityBar.Value) = (b.durabilityBar.Value, a.durabilityBar.Value);
	}
}
