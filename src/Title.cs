using MGLib;

Game.Start<Title>(160, 90);

internal sealed class Title : State {
	private readonly Sprite background = new Sprite("title");

	protected override void OnConfirm() {
		Game.ChangeState<Battle>();
	}

	protected override void Draw() {
		Game.Draw(background);
	}
}
