using MGLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

new TowerDefense();

internal sealed class TowerDefense : NotatoGame {
	private struct Tower {
		internal byte[] name;
		internal Texture2D texture;
		internal byte cost;
	}

	private enum Scene : byte {
		Title,
		Battle
	}

	private Texture2D frameTexture;
	private Texture2D titleScreenTexture;
	private Texture2D sceneTexture;
	private Texture2D arrowUpTexture;
	private Texture2D arrowDownTexture;

	private readonly Tower[] towers = new Tower[] {
		new Tower() { name = new byte[] { 53, 10, 29, 38, 17, 14, 15 }, texture = null, cost = 2 }, // Produces cheese
		new Tower() { name = new byte[] { 53, 10, 29, 37, 10, 23, 13, 18, 29 }, texture = null, cost = 5 }, // Steals money from enemies, melee
		new Tower() { name = new byte[] { 53, 10, 29, 36, 27, 12, 17, 14, 27 }, texture = null, cost = 4 }, // Shoots in a straight line
		new Tower() { name = new byte[] { 53, 10, 29, 46, 23, 18, 16, 17, 29 }, texture = null, cost = 2 } // Blocks enemies
	};

	private int timer = 0;
	private byte cheese = 2;
	private Scene scene = Scene.Title;

	internal TowerDefense() : base(160, 90) {}

	protected override void InitializeMethod() {
		frameTexture = Content.Load<Texture2D>("frame");
		titleScreenTexture = Content.Load<Texture2D>("title");
		arrowUpTexture = Content.Load<Texture2D>("arrow_up");
		arrowDownTexture = Content.Load<Texture2D>("arrow_down");

		sceneTexture = titleScreenTexture;
	}

	protected override void Update(GameTime gameTime) {
		KeyboardState keyboard = Keyboard.GetState();

		if (scene == Scene.Title && keyboard[Keys.Z] == KeyState.Down) {
			sceneTexture = frameTexture;
			scene = Scene.Battle;
		} else if (scene == Scene.Battle) {
			if (timer < 600) {
				timer++;
			} else {
				timer = 0;
				if (cheese < 99) {
					cheese++;
				}
			}
		}
	}

	protected override void DrawMethod() {
		Draw(sceneTexture, 0, 0);

		if (scene == Scene.Battle) {
			DrawString(new[] { (byte) (cheese / 10), (byte) (cheese % 10) }, 7, 81);
			Draw(arrowUpTexture, 21, 81);
			Draw(arrowDownTexture, 21, 86);
			DrawString(towers[0].name, 27, 81);
			DrawString(new[] { towers[0].cost }, 154, 81);
		}
	}
}
