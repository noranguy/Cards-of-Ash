using Godot;
using System;
using System.Collections.Generic;

public abstract class Agent {
	public abstract void Init();
	
	public abstract (Card, int) Move(List<Card> hand);
	
	public abstract void Backward(List<int> indices, List<int> types);
}
