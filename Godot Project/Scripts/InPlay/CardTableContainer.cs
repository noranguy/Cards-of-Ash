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
	private Vector2 vanishingPoint = new Vector2(108.5f, -500);
	private Vector2 offset = new Vector2(-92.5f, 175);

	public void Init(
		PackedScene scene, List<string> playerTypes,
		List<string> playerClasses,
		List<string> enemyTypes, List<String> enemyClasses,
		float playerY, float enemyY
	) {
		rand = new Random();
		cardScene = scene;
		
		for (int i = 0; i < numCards; i++) {
			vectorX.Add(new Vector2(i * (32 + 5), 0));
			vectorX.Add(new Vector2(i * (32 + 5) + 32, 0));
		}
		
		enemyCards = SpawnCards(enemyTypes, enemyClasses, enemyY, false);
		playerCards = SpawnCards(playerTypes, playerClasses, playerY, true);
	}
	
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
	/*
		Vector2 bottomLeft = bottomCenter + new Vector2(-width / 2, 0);
		Vector2 bottomRight = bottomCenter + new Vector2(width / 2, 0);

		Vector2 dirLeft = (vanishingPoint - bottomLeft).Normalized();
		Vector2 dirRight = (vanishingPoint - bottomRight).Normalized();

		Vector2 topLeft = bottomLeft + dirLeft * height;
		Vector2 topRight = bottomRight + dirRight * height;*/
	
	public virtual List<Card> SpawnCards(
		List<string> types, List<string> classes,
		float y, bool isPlayer
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
				bottomLeft = lVec;
				bottomRight = rVec;
			} else {
				bottomLeft = lVec + dirLeft * (Card.SIZE.Y + 5);
				bottomRight = rVec + dirRight * (Card.SIZE.Y + 5);
			}
			Vector2 topLeft = bottomLeft + dirLeft * Card.SIZE.Y;
			Vector2 topRight = bottomRight + dirRight * Card.SIZE.Y;
			//GD.Print($"({topLeft.X}, {topLeft.Y}) ({topRight.X}, {topRight.Y}) ({bottomRight.X}, {bottomRight.Y}) ({bottomLeft.X}, {bottomLeft.Y})");
			Vector2[] vertices = new Vector2[] {
				topLeft + offset,
				topRight + offset,
				bottomRight + offset,
				bottomLeft + offset
			};
			card.Init(vertices, types[order[i]], classes[order[i]], false, isPlayer, i);
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));

			AddChild(card);
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
	public bool allowActive = true;
	
	public virtual void OnCardClicked(Card card) {
		if (activeCard == card || !allowActive || (!card.isPlayer && card.index == -1)) {
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
