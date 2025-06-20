using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public class DialogueOption {
	public string text { get; set; }
	public string next { get; set; }
}

public class DialogueNode {
	public string id { get; set; }
	public string text { get; set; }
	public List<DialogueOption> options { get; set; }
}

public partial class DialogueManager : CanvasLayer {
	private Dictionary<string, DialogueNode> dialogueTree = new();
	public static DialogueNode currentNode;

	private Label dialogueText;
	private VBoxContainer optionsContainer;
	private PackedScene optionButtonScene;

	public override void _Ready() {
		dialogueText = GetNode<Label>("DialoguePanel/DialogueText");
		optionsContainer = GetNode<VBoxContainer>("DialoguePanel/OptionsContainer");

		LoadDialogue("res://Dialogue/intro.json");
		ShowNode("intro");
	}

	public void LoadDialogue(string path) {
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		var jsonText = file.GetAsText();
		file.Close();

		var nodes = JsonSerializer.Deserialize<List<DialogueNode>>(jsonText);
		foreach (var node in nodes) {
			if (string.IsNullOrEmpty(node.id)) continue;
			node.options ??= new List<DialogueOption>();
			dialogueTree[node.id] = node;
		}
	}

	public void ShowNode(string id) {
		var currentNode = dialogueTree[id];
		if (!dialogueTree.TryGetValue(id, out currentNode)) {
			return;
		}

		dialogueText.Text = currentNode.text;
		ClearOptions();

		for (int i = 0; i < currentNode.options.Count; i++) {
			var option = currentNode.options[i];
			var button = new Button
			{
				Text = option.text,
				Name = i.ToString()
			};
			button.Pressed += () => ShowNode(option.next);
			optionsContainer.AddChild(button);
		}
		GD.Print(currentNode.options.Count);
	}

	private void ClearOptions() {
		foreach (Node child in optionsContainer.GetChildren()) {
			child.QueueFree();
		}
	}

	public void ChooseOption(int index) {
		GD.Print(currentNode);
		if (currentNode == null || index < 0 || index >= currentNode.options.Count) {
			GD.PrintErr("Invalid option index");
			return;
		}

		ShowNode(currentNode.options[index].next);
	}
}
