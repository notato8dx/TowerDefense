using MGLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

new NotatoGame(160, 90, new TitleScene());

internal sealed class TitleScene : Scene {
	private Texture2D background;

	internal TitleScene() {
		actions[Keys.Z] = (NotatoGame game) => {
			game.changeScene(new BattleScene());
		};
	}

	protected override void Initialize(NotatoGame game) {
		background = game.Content.Load<Texture2D>("title");
	}

	protected override void Update(NotatoGame game) {}

	protected override void Draw(NotatoGame game) {
		game.Draw(background, 0, 0);
	}
}

internal sealed class BattleScene : Scene {
	private Texture2D background;
	private Texture2D arrowUpTexture;
	private Texture2D arrowDownTexture;
	private Texture2D cursorTexture;

	private int timer = 0;
	private byte cheese = 2;
	private byte cursorX = 4;
	private byte cursorY = 4;

	private readonly Tower[] towers = new Tower[] {
		new Tower() { name = new byte[] { 53, 10, 29, 38, 17, 14, 15 }, texture = null, cost = 2 }, // Produces cheese
		new Tower() { name = new byte[] { 53, 10, 29, 37, 10, 23, 13, 18, 29 }, texture = null, cost = 5 }, // Steals money from enemies, melee
		new Tower() { name = new byte[] { 53, 10, 29, 36, 27, 12, 17, 14, 27 }, texture = null, cost = 4 }, // Shoots in a straight line
		new Tower() { name = new byte[] { 53, 10, 29, 46, 23, 18, 16, 17, 29 }, texture = null, cost = 2 } // Blocks enemies
	};

	internal BattleScene() {
		actions[Keys.X] = (NotatoGame game) => {
			game.changeScene(new TitleScene());
		};
	}

	protected override void Initialize(NotatoGame game) {
		background = game.Content.Load<Texture2D>("frame");
		arrowUpTexture = game.Content.Load<Texture2D>("arrow_up");
		arrowDownTexture = game.Content.Load<Texture2D>("arrow_down");
		cursorTexture = game.Content.Load<Texture2D>("cursor");
	}

	protected override void Update(NotatoGame game) {
		if (timer < 600) {
			timer++;
		} else {
			timer = 0;
			if (cheese < 99) {
				cheese++;
			}
		}
	}

	protected override void Draw(NotatoGame game) {
		game.Draw(background, 0, 0);
		game.DrawString(new[] { (byte) (cheese / 10), (byte) (cheese % 10) }, 7, 81);
		game.Draw(arrowUpTexture, 21, 81);
		game.Draw(arrowDownTexture, 21, 86);
		game.DrawString(towers[0].name, 27, 81);
		game.DrawString(new[] { towers[0].cost }, 154, 81);
		game.Draw(cursorTexture, cursorX, cursorY);
	}

	private struct Tower {
		internal byte[] name;
		internal Texture2D texture;
		internal byte cost;
	}
}
