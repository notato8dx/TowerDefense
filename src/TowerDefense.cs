using MGLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

new NotatoGame(160, 90, new TitleScene(), (ContentManager Content) => {
	TitleScene.background = Content.Load<Texture2D>("title");
	BattleScene.background = Content.Load<Texture2D>("frame");
	BattleScene.arrowUpTexture = Content.Load<Texture2D>("arrow_up");
	BattleScene.arrowDownTexture = Content.Load<Texture2D>("arrow_down");
	BattleScene.cursorTexture = Content.Load<Texture2D>("cursor");
});

internal sealed class TitleScene : Scene {
	internal static Texture2D background;

	internal TitleScene() {
		actions[Keys.Z] = () => {
			game.scene = new BattleScene();
		};
	}

	protected override void Update() {}

	protected override void Draw() {
		game.Draw(background, 0, 0);
	}
}

internal sealed class BattleScene : Scene {
	private struct Tower {
		internal byte[] name;
		internal Texture2D texture;
		internal byte cost;
	}

	internal static Texture2D background;
	internal static Texture2D arrowUpTexture;
	internal static Texture2D arrowDownTexture;
	internal static Texture2D cursorTexture;

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

	protected override void Update() {
		if (timer < 600) {
			timer++;
		} else {
			timer = 0;
			if (cheese < 99) {
				cheese++;
			}
		}
	}

	protected override void Draw() {
		game.Draw(background, 0, 0);
		game.DrawString(new[] { (byte) (cheese / 10), (byte) (cheese % 10) }, 7, 81);
		game.Draw(arrowUpTexture, 21, 81);
		game.Draw(arrowDownTexture, 21, 86);
		game.DrawString(towers[0].name, 27, 81);
		game.DrawString(new[] { towers[0].cost }, 154, 81);
		game.Draw(cursorTexture, cursorX, cursorY);
	}
}

/*internal sealed class TowerDefense : NotatoGame {
	private sealed class TitleScene : Scene {
		internal static Texture2D background;

		internal TitleScene() {
			actions[Keys.Z] = () => {
				game.scene = new BattleScene();
			};
		}

		protected override void Update() {}

		protected override void Draw() {
			game.Draw(background, 0, 0);
		}
	}

	private sealed class BattleScene : Scene {
		private struct Tower {
			internal byte[] name;
			internal Texture2D texture;
			internal byte cost;
		}

		internal static Texture2D background;
		internal static Texture2D arrowUpTexture;
		internal static Texture2D arrowDownTexture;
		internal static Texture2D cursorTexture;

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

		protected override void Update() {
			if (timer < 600) {
				timer++;
			} else {
				timer = 0;
				if (cheese < 99) {
					cheese++;
				}
			}
		}

		protected override void Draw() {
			game.Draw(background, 0, 0);
			game.DrawString(new[] { (byte) (cheese / 10), (byte) (cheese % 10) }, 7, 81);
			game.Draw(arrowUpTexture, 21, 81);
			game.Draw(arrowDownTexture, 21, 86);
			game.DrawString(towers[0].name, 27, 81);
			game.DrawString(new[] { towers[0].cost }, 154, 81);
			game.Draw(cursorTexture, cursorX, cursorY);
		}
	}

	internal TowerDefense() : base(160, 90, new TitleScene(), (ContentManager Content) => {
		TitleScene.background = Content.Load<Texture2D>("title");
		BattleScene.background = Content.Load<Texture2D>("frame");
		BattleScene.arrowUpTexture = Content.Load<Texture2D>("arrow_up");
		BattleScene.arrowDownTexture = Content.Load<Texture2D>("arrow_down");
		BattleScene.cursorTexture = Content.Load<Texture2D>("cursor");
	}) {}

	protected override void Load() {
	}
}*/
