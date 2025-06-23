using Godot;
using System;
using System.Collections.Generic;

public partial class Hand : Control {
	private List<Card> cards = new();
	public readonly int startingAmount = 9;
	private readonly Curve hand_curve = GD.Load<Curve>($"res://Components/hand_y_curve.tres");
	private readonly Curve rotation_curve = GD.Load<Curve>($"res://Components/hand_rotation_curve.tres");
	private readonly int max_rotation_degrees = 10;
	private readonly int x_sep = 0;
	private int y_min;
	private int y_max;
	private float scale;
	
	public void Init(PackedScene scene, int y, float scale, bool visible, List<string> types, List<string> classes) {
		SetMouseFilter(Control.MouseFilterEnum.Ignore);
		cardScene = scene;
		y_max = y;
		y_min = y + 100;
		scale *= 0.75f;
		this.scale = scale;
		Size = new Vector2(250 * scale, 100 * scale);
		SpawnCards(visible, types, classes);
	}

	public virtual void SpawnCards(bool visible, List<string> types, List<string> classes) {
		for (int i = 0; i < startingAmount; i++) {
			Card card = cardScene.Instantiate<Card>();
			card.Name = $"Card{i}";
			card.Position = Vector2.Zero;
			card.Scale = new Vector2(scale, scale);
			card.Init(Card.DEFAULT_VERTICES, types[i], classes[i], visible, visible, -1);
			card.Connect(Card.SignalName.CardClicked, new Callable(this, nameof(OnCardClicked)));

			AddChild(card);
			cards.Add(card);
		}

		UpdateCardPositions();
	}
	
	public virtual void UpdateCardPositions() {
		int numCards = cards.Count;
		int cardFormat = 8 + cards.Count % 2;
		int cardSize = (int)(Card.SIZE.X * scale);
		
		float final_x_sep = (Size.X - cardSize * cardFormat) / (cardFormat - 1);
		float all_cards_size = Size.X;

		float offset = (cardSize - (cardSize * numCards + final_x_sep * (numCards - 1))) / 2;
		
		for (int i = 0; i < numCards; i++) {
			Card card = cards[i];
			int alignIdx = (cardFormat - numCards) / 2 + i;
			float y_multiplier, rot_multiplier;
			
			if (numCards > 1) {
				y_multiplier = hand_curve.Sample(1f / (cardFormat-1) * alignIdx);
				rot_multiplier = rotation_curve.Sample(1f / (cardFormat-1) * alignIdx);
			} else {
				y_multiplier = rot_multiplier = 0;
			}
			
			float final_x = (cardSize + final_x_sep) * i + offset;
			float final_y = y_min - Size.Y * y_multiplier;
			
			card.Position = new Vector2(final_x, final_y);
			card.RotationDegrees = max_rotation_degrees * rot_multiplier;
		}
	}

	public void RemoveCard(Card card) {
		if (cards.Contains(card)) {
			cards.Remove(card);
			card.QueueFree();
			UpdateCardPositions();
		}
	}
	
	public List<Card> GetCards() {
		return cards;
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
