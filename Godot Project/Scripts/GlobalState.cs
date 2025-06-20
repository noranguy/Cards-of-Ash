using Godot;
using System;
using System.Collections.Generic;

public partial class GlobalState : Node {
	public static GlobalState Instance { get; private set; }
	
	public override void _Ready() {
		Instance = this;
	}
	
	private List<string> humanHandTypes = new List<string> {
		"tsunami",
		"volcano",
		"earthquake",
		"tsunami",
		"volcano",
		"earthquake",
		"tsunami",
		"volcano",
		"earthquake"
	};
	private List<string> humanHandClasses = new List<string> {
		"basic",
		"basic",
		"basic",
		"basic",
		"basic",
		"basic",
		"basic",
		"basic",
		"basic"
	};
	private List<string> humanTableTypes = new List<string> {
		"tsunami",
		"volcano",
		"earthquake",
		"tsunami",
		"volcano",
		"earthquake"
	};
	private List<string> humanTableClasses = new List<string> {
		"basic",
		"basic",
		"basic",
		"basic",
		"basic",
		"basic"
	};
	
	public (List<string>, List<string>) GetHandCards() {
		return (humanHandTypes, humanHandClasses);
	}
	
	public (List<string>, List<string>) GetTableCards() {
		return (humanTableTypes, humanTableClasses);
	}
	
	public void AddHandCard(string type, string clas) {
		humanHandTypes.Add(type);
		humanHandClasses.Add(clas);
	}
	
	public void AddTableCard(string type, string clas) {
		humanTableTypes.Add(type);
		humanTableClasses.Add(clas);
	}
	
	public readonly float[] FlipProb = new float[] {0.05f, 0.5f, 0.95f};
	
	public readonly Dictionary<string, int> TypeMap = new() {
		{ "tsunami", 0 },
		{ "volcano", 1 },
		{ "earthquake", 2 }
	};
	
	public readonly float CeramicProb = 0.25f;
	
	private int day = 0;
	private List<Func<Agent>> AgentFactories = new List<Func<Agent>> {
		() => new Agent0(),
		() => new Agent1()
	};
	
	public Agent GetNextAgent() {
		return AgentFactories[day++]();
	}
	
	public int GetDay() {
		return day;
	}
}
