using Godot;
using System;

public partial class CardSlot : CenterContainer {
	[Signal]
	public delegate void CardDroppedEventHandler();
	
	private bool isHand;
	
	public override void _Ready() {
		isHand = GetParent().Name == "HandSlots";
	}
	
	public override bool _CanDropData(Vector2 position, Variant data) {
		return data.As<Card>() != null && (isHand == (data.As<Card>().index == 0));
	}

	public override void _DropData(Vector2 position, Variant data) {
		var card = data.As<Card>();
		if (card != null) {
			card.GetParent().RemoveChild(card);
			AddChild(card);
			card.Position = Vector2.Zero;
			EmitSignal(SignalName.CardDropped);
		}
	}
}
