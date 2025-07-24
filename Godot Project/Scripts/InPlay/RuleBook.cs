using Godot;
using System;

public partial class RuleBook : Panel {
	private RichTextLabel info;
	private TextureButton leftButton;
	private TextureButton rightButton;
	private Label pageNumberLabel;
	
	private int lastPage;
	private int curPage;
	
	public override void _Ready() {
		info = GetNode<RichTextLabel>("Info");
		leftButton = GetNode<TextureButton>("LeftButton");
		rightButton = GetNode<TextureButton>("RightButton");
		pageNumberLabel = GetNode<Label>("PageNumber");
		
		leftButton.Disabled = true;
		leftButton.Modulate = new Color(1, 1, 1, 0.4f);
		
		leftButton.Pressed += PrevPage;
		rightButton.Pressed += NextPage;
		
		curPage = 0;
		lastPage = GlobalState.Instance.GetDay() + 4;
		if (GlobalState.Instance.GetDay() == 5) lastPage--;
	}
	
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("ui_left")) {
			PrevPage();
		} else if (Input.IsActionJustPressed("ui_right")) {
			NextPage();
		}
	}
	
	private void NextPage() {
		if (curPage == lastPage) return;
		curPage++;
		info.Text = GlobalState.Instance.RulebookPages[curPage];
		if (curPage == lastPage) {
			rightButton.Disabled = true;
			rightButton.Modulate = new Color(1, 1, 1, 0.4f);
		} else if (curPage == 1) {
			leftButton.Disabled = false;
			leftButton.Modulate = Colors.White;
		}
		pageNumberLabel.Text = $"{curPage + 1}";
	}
	
	private void PrevPage() {
		if (curPage == 0) return;
		curPage--;
		info.Text = GlobalState.Instance.RulebookPages[curPage];
		if (curPage == 0) {
			leftButton.Disabled = true;
			leftButton.Modulate = new Color(1, 1, 1, 0.4f);
		} else if (curPage == lastPage - 1) {
			rightButton.Disabled = false;
			rightButton.Modulate = Colors.White;
		}
		pageNumberLabel.Text = $"{curPage + 1}";
	}
}
