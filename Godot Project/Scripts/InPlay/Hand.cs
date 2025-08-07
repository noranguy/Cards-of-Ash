using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Hand : Control {
	private List<Card> cards = new();
	public readonly int startingAmount = 9;
	private readonly Curve hand_curve = GD.Load<Curve>($"res://Components/hand_y_curve.tres");
	private readonly Curve rotation_curve = GD.Load<Curve>($"res://Components/hand_rotation_curve.tres");
	private readonly int max_rotation_degrees = 10;
	private readonly int x_sep = 0;
	private int y_min;
	private int y_max;
	private Vector2 scaleV;
	private bool visible;
	
	public void Init(PackedScene scene, int y, float scale, bool visible, List<(string, string)> cardInfo) {
		SetMouseFilter(Control.MouseFilterEnum.Pass);
		cardScene = scene;
		y_max = y;
		y_min = y + 100;
		Size = new Vector2(250 * scale, 100 * scale);
		scaleV = new Vector2(scale, scale);
		this.visible = visible;
		SpawnCards(visible, cardInfo);
	}

	public async virtual void SpawnCards(bool visible, List<(string, string)> cardInfo) {
		foreach ((string type, string clas) in cardInfo) {
			Card card = cardScene.Instantiate<Card>();
			card.Position = Vector2.Zero;
			card.ZIndex = 15;
			card.Init(Card.DEFAULT_VERTICES, type, clas, false, visible, -1);
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));

			AddChild(card);
			cards.Add(card);
		}

		await UpdateCardPositions(true);
		if (!visible) {
			foreach (Card card in cards) {
				card.ZIndex = 4;
			}
		}
	}
	
	// card fan
	public async virtual Task UpdateCardPositions(bool first) {
		int numCards = cards.Count;
		int cardSize = (int)(Card.SIZE.X * scaleV.X);
		if (numCards == 1) {
			Vector2 pos = new Vector2(-cardSize / 2f, cards[0].Position.Y);
			cards[0].UpdatePosition(pos);
			cards[0].RotationDegrees = 0;
			cards[0].upperPosition = pos + new Vector2(0, -10);
			cards[0].lowerPosition = pos;
			return;
		}
		
		float final_x_sep = (Size.X - cardSize * numCards) / (numCards - 1);
		float all_cards_size = Size.X;

		float offset = (-(cardSize * numCards + final_x_sep * (numCards - 1))) / 2;
		
		for (int i = 0; i < numCards; i++) {
			Card card = cards[i];
			float y_multiplier, rot_multiplier;
			
			if (numCards > 1) {
				y_multiplier = hand_curve.Sample(1f / (numCards-1) * i);
				rot_multiplier = rotation_curve.Sample(1f / (numCards-1) * i);
			} else {
				y_multiplier = rot_multiplier = 0;
			}
			
			float finalX = (cardSize + final_x_sep) * i + offset;
			float finalY = y_min - Size.Y * y_multiplier;
			Vector2 finalV = new Vector2(finalX, finalY);
			
			if (first) {
				card.visible = visible;
				card.UpdateTexture();
				await card.UpdatePosition(finalV, scaleV);
			} else {
				card.UpdatePosition(finalV);
			}
			card.RotationDegrees = max_rotation_degrees * rot_multiplier;
			card.upperPosition = finalV + -10 * Vector2.FromAngle((card.RotationDegrees + 90) * (float)Math.PI / 180);
			card.lowerPosition = finalV;
			card.ready = true;
		}
	}

	public async void RemoveCard(Card card) {
		if (cards.Contains(card)) {
			await card.UpdatePosition(card.Position + new Vector2(0, 40));
			cards.Remove(card);
			card.QueueFree();
			Size -= new Vector2(26.5f * scaleV.X, 0);
			UpdateCardPositions(false);
		}
	}
	
	public List<Card> GetCards() {
		return cards;
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
