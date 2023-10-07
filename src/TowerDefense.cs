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
	private const byte TowerCount = 4;
	private const byte RowCount = 5;
	private const byte ColumnCount = 9;
	private const byte TileCount = RowCount * ColumnCount;
	private const byte TileWidth = 17;
	private const byte TileHeight = 15;
	private const byte CursorOffset = 4;

	private Texture2D background;
	private Texture2D arrowUpTexture;
	private Texture2D arrowDownTexture;
	private Texture2D cursorTexture;

	private int timer = 0;
	private byte cheese = 2;
	private byte cursorX = 4;
	private byte cursorY = 4;
	private byte cursorIndex = 0;
	private bool selecting = false;
	private byte towerIndex = 0;

	private Tower[] towers;

	private byte[] tiles = new byte[TileCount];

	internal BattleScene() {
		actions[Keys.Z] = (NotatoGame game) => {
			if (selecting) {
				tiles[cursorIndex] = towerIndex;
				selecting = false;
			} else {
				selecting = true;
			}
		};

		actions[Keys.X] = (NotatoGame game) => {
			if (selecting) {
				selecting = false;
			}
		};

		actions[Keys.Up] = (NotatoGame game) => {
			if (selecting) {
				if (towerIndex > 0) {
					towerIndex--;
				}
			} else if (cursorY > CursorOffset) {
				cursorIndex -= ColumnCount;
				cursorY -= TileHeight;
			}
		};

		actions[Keys.Down] = (NotatoGame game) => {
			if (selecting) {
				if (towerIndex < TowerCount) {
					towerIndex++;
				}
			} else if (cursorY < TileHeight * (RowCount - 1) + CursorOffset) {
				cursorIndex += ColumnCount;
				cursorY += TileHeight;
			}
		};

		actions[Keys.Left] = (NotatoGame game) => {
			if (selecting) {
			} else if (cursorX > CursorOffset) {
				cursorIndex -= 1;
				cursorX -= TileWidth;
			}
		};

		actions[Keys.Right] = (NotatoGame game) => {
			if (selecting) {
			} else if (cursorX < TileWidth * (ColumnCount - 1) + CursorOffset) {
				cursorIndex += 1;
				cursorX += TileWidth;
			}
		};
	}

	protected override void Initialize(NotatoGame game) {
		background = game.Content.Load<Texture2D>("frame");
		arrowUpTexture = game.Content.Load<Texture2D>("arrow_up");
		arrowDownTexture = game.Content.Load<Texture2D>("arrow_down");
		cursorTexture = game.Content.Load<Texture2D>("cursor");
		towers = new Tower[TowerCount + 1] {
			new Tower() { name = new byte[] { }, texture = null, cost = 0 },
			new Tower() { name = new byte[] { 53, 10, 29, 38, 17, 14, 15 }, texture = game.Content.Load<Texture2D>("rat_chef"), cost = 2 }, // Produces cheese
			new Tower() { name = new byte[] { 53, 10, 29, 37, 10, 23, 13, 18, 29 }, texture = game.Content.Load<Texture2D>("rat_bandit"), cost = 5 }, // Steals money from enemies, melee
			new Tower() { name = new byte[] { 53, 10, 29, 36, 27, 12, 17, 14, 27 }, texture = game.Content.Load<Texture2D>("rat_archer"), cost = 4 }, // Shoots in a straight line
			new Tower() { name = new byte[] { 53, 10, 29, 46, 23, 18, 16, 17, 29 }, texture = game.Content.Load<Texture2D>("rat_bandit"), cost = 2 } // Blocks enemies
		};
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
		
		if (towerIndex > 0) {
			game.Draw(arrowUpTexture, 21, 81);
		}

		if (towerIndex < TowerCount) {
			game.Draw(arrowDownTexture, 21, 86);
		}

		for (byte row = 0; row < RowCount; row++) {
			for (byte col = 0; col < ColumnCount; col++) {
				byte tower = tiles[row * ColumnCount + col];

				if (tower == 0) {
					continue;
				}

				game.Draw(towers[tiles[row * ColumnCount + col]].texture, CursorOffset + col * TileWidth, CursorOffset + row * TileHeight);
			}
		}

		game.DrawString(towers[towerIndex].name, 27, 81);
		game.DrawString(new[] { towers[towerIndex].cost }, 154, 81);
		game.Draw(cursorTexture, cursorX, cursorY);
	}

	private struct Tower {
		internal byte[] name;
		internal Texture2D texture;
		internal byte cost;
	}
}
