using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardTableContainer : Node2D {
	public readonly int numCards = 6;
	
	private List<Card> playerCards;
	private List<Card> enemyCards;
	private Random rand;
	
	private List<Vector2> vectorX = new List<Vector2>();
	private Vector2 vanishingPoint = new Vector2(108.5f, -2000);
	private Vector2 offset;

	public void Init(
		PackedScene scene, List<string> playerTypes,
		List<string> playerClasses,
		List<string> enemyTypes, List<string> enemyClasses
	) {
		rand = new Random();
		cardScene = scene;
		
		for (int i = 0; i < numCards; i++) {
			vectorX.Add(new Vector2(i * (Card.SIZE2.X + 5), 0));
			vectorX.Add(new Vector2(i * (Card.SIZE2.X + 5) + Card.SIZE2.X, 0));
		}
		
		offset = new Vector2(-(vectorX[0].X + vectorX[^1].X) / 2, 175);
		
		enemyCards = SpawnCards(enemyTypes, enemyClasses, false);
		playerCards = SpawnCards(playerTypes, playerClasses, true);
	}
	
	// randomize order of table cards
	private List<int> GenRandomOrder() {
		List<int> order = Enumerable.Range(0, numCards).ToList();
		
		if (GlobalState.Instance.GetDay() == 0) {
			for (int i = numCards-1; i > 1; i--) {
				int j = rand.Next(1, i+1);
				(order[i], order[j]) = (order[j], order[i]);
			}
		} else {
			for (int i = numCards-1; i > 0; i--) {
				int j = rand.Next(i+1);
				(order[i], order[j]) = (order[j], order[i]);
			}
		}
		
		return order;
	}
	
	// instantiate cards using vectors to a vanishing point for perspective distortion
	public virtual List<Card> SpawnCards(
		List<string> types, List<string> classes,
		bool isPlayer
	) {
		List<int> order = GenRandomOrder();
		List<Card> cards = new List<Card>();
		
		for (int i = 0; i < numCards; i++) {
			Card card = cardScene.Instantiate<Card>();
			card.Name = $"Card{i}";
			card.Position = Vector2.Zero;
			Vector2 lVec = vectorX[i*2];
			Vector2 rVec = vectorX[i*2+1];
			Vector2 dirLeft = (vanishingPoint - lVec).Normalized();
			Vector2 dirRight = (vanishingPoint - rVec).Normalized();
			Vector2 bottomLeft, bottomRight;
			
			if (isPlayer) {
				bottomLeft = lVec + dirLeft * 10;
				bottomRight = rVec + dirRight * 10;
			} else {
				bottomLeft = lVec + dirLeft * (Card.SIZE2.Y + 20);
				bottomRight = rVec + dirRight * (Card.SIZE2.Y + 20);
			}
			Vector2 topLeft = bottomLeft + dirLeft * Card.SIZE2.Y;
			Vector2 topRight = bottomRight + dirRight * Card.SIZE2.Y;
			
			Vector2[] vertices = new Vector2[] {
				topLeft + offset,
				topRight + offset,
				bottomRight + offset,
				bottomLeft + offset
			};
			card.Init(vertices, types[order[i]], classes[order[i]], false, isPlayer, i);
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));
			
			Vector2 dPos = new Vector2((topLeft.X + topRight.X) / 2f + offset.X - 16, topLeft.Y + offset.Y - 3);
			card.durabilityBar.Visible = true;
			card.durabilityBar.Position = dPos;
			card.durabilityBar.Size *= new Vector2(1, 0.25f);
			card.durabilityBar.Scale *= new Vector2(1, 0.25f);
			
			AddChild(card);
			card.ready = true;
			cards.Add(card);
		}

		return cards;
	}
	
	public List<Card> GetPlayerCards() {
		return playerCards;
	}
	
	public List<Card> GetEnemyCards() {
		return enemyCards;
	}
	
	[Signal]
	public delegate void ActiveCardEventHandler(Card newCard);

	public PackedScene cardScene;
	public Card activeCard = null;
	public HashSet<Card> restrictAllow = new HashSet<Card>();
	
	public virtual void OnCardClicked(Card card) {
		if (activeCard == card || (restrictAllow.Count > 0 && !restrictAllow.Contains(card)) || (!card.isPlayer && card.index == -1)) {
			return;
		}
		
		if (activeCard != null) {
			activeCard.locked = false;
			activeCard.Unhighlight();
		}
		card.locked = true;
		card.Highlight();
		activeCard = card;
		
		EmitSignal(SignalName.ActiveCard, card);
	}
}
