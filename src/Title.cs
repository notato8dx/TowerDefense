using MGLib;

Game.Start<Title>(160, 90);

internal sealed class Title : State {
	private static readonly Song music = new("title-screen");
	private static readonly Animation titleAnimation = new("title-animated", 160, 90, 4, 144, 215);

	public Title() {
		Game.ChangeSong(music);
	}

	protected override void OnConfirm() {
		Game.ChangeState<Battle>();
	}

	protected override void Draw() {
		titleAnimation.Draw();
	}
}