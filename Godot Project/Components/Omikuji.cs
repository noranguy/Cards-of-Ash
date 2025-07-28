using Godot;
using System;
using System.Threading.Tasks;

public partial class Omikuji : Control {
	private TextureRect fortune;
	private Control box;
	private Tween boxTween;
	private Tween fortuneTween;
	private Vector2 _start;
	private Vector2 _end;
	
	public override void _Ready() {
		fortune = GetNode<TextureRect>("fortune");
		box = GetNode<Control>("Box");
	}
	
	public async Task Start(int fortuneNum) {
		var texture = GD.Load<Texture2D>($"res://Assets/Omamori/fortune_slips/{fortuneNum}.png");
		fortune.Texture = texture;
		
		Vector2 mid = box.Position;
		Vector2 left = new Vector2(mid.X, mid.Y);
		Vector2 right = new Vector2(mid.X, mid.Y);
		
		left += new Vector2(-1.9f, 0.5f);
		right += new Vector2(1.9f, -0.5f);
		
		for (int i = 0; i < 3; i++) {
			await UpdatePosition(mid, left, 0.02f);
			await UpdatePosition(left, mid, 0.02f);
			await UpdatePosition(mid, right, 0.02f);
			await UpdatePosition(right, mid, 0.02f);
		}
		
		Vector2 up = new Vector2(fortune.Position.X, fortune.Position.Y - 63);
		fortuneTween = GetTree().CreateTween();
		fortuneTween.TweenProperty(fortune, "position", up, 1f);
		await ToSignal(fortuneTween, "finished");
	}
	
	private async Task UpdatePosition(Vector2 start, Vector2 end, float duration) {
		boxTween = CreateTween();
		_start = start;
		_end = end;
		boxTween.TweenMethod(
			new Callable(this, nameof(UpdatePositionHelper)),
			0.0f, 1.0f, duration
			);
		await ToSignal(boxTween, "finished");
	}
	
	private void UpdatePositionHelper(float t) {
		box.Position = _start.Lerp(_end, t);
	}
}
