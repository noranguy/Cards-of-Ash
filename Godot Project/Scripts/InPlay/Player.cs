using Godot;
using System;
using System.Collections.Generic;

public class Player {
	private List<string> humanHandTypes = new List<string> {
		"light",
		"regular",
		"heavy",
		"light",
		"regular",
		"heavy",
		"light",
		"regular",
		"heavy"
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
		"light",
		"regular",
		"heavy",
		"light",
		"regular",
		"heavy"
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
}
