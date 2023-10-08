﻿using MGLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

new NotatoGame(160, 90, new TitleScene());

internal sealed class TitleScene : Scene {
	private Texture2D background;

	protected override void Initialize(NotatoGame game) {
		background = game.Content.Load<Texture2D>("title");
	}

	protected override void OnConfirm(NotatoGame game) {
		game.ChangeScene(new BattleScene());
	}

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
	
	private State<BattleScene> state = new SelectState();

	private int timer = 0;
	private byte cheese = 2;
	private byte cursorX = 4;
	private byte cursorY = 4;
	private byte cursorIndex = 0;
	internal byte towerIndex = 0;

	private Tower[] towers;

	private byte[] tiles = new byte[TileCount];

	protected override void OnConfirm(NotatoGame game) {
		state.OnConfirm(this);
	}

	protected override void OnCancel(NotatoGame game) {
		state.OnCancel(this);
	}

	protected override void OnMoveUp(NotatoGame game) {
		state.OnMoveUp(this);
	}

	protected override void OnMoveDown(NotatoGame game) {
		state.OnMoveDown(this);
	}

	protected override void OnMoveLeft(NotatoGame game) {
		state.OnMoveLeft(this);
	}

	protected override void OnMoveRight(NotatoGame game) {
		state.OnMoveRight(this);
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
		
		for (byte row = 0; row < RowCount; row++) {
			for (byte col = 0; col < ColumnCount; col++) {
				byte tower = tiles[row * ColumnCount + col];

				if (tower == 0) {
					continue;
				}

				game.Draw(towers[tiles[row * ColumnCount + col]].texture, CursorOffset + col * TileWidth, CursorOffset + row * TileHeight);
			}
		}

		game.Draw(cursorTexture, cursorX, cursorY);

		state.Draw(game, this);
	}

	internal struct Tower {
		internal byte[] name;
		internal Texture2D texture;
		internal byte cost;

		internal void Update(BattleScene battleScene) {

		}
	}

	/*private struct GameTower {
		private Tower tower;
		private byte health = 5;
		private int timer = 0;
	}*/

	private sealed class SelectState : State<BattleScene> {
		public override void OnConfirm(BattleScene scene) {
			scene.state = new BuildState();
		}

		public override void OnMoveUp(BattleScene scene) {
			if (scene.cursorY > CursorOffset) {
				scene.cursorIndex -= ColumnCount;
				scene.cursorY -= TileHeight;
			}
		}

		public override void OnMoveDown(BattleScene scene) {
			if (scene.cursorY < TileHeight * (RowCount - 1) + CursorOffset) {
				scene.cursorIndex += ColumnCount;
				scene.cursorY += TileHeight;
			}
		}

		public override void OnMoveLeft(BattleScene scene) {
			if (scene.cursorX > CursorOffset) {
				scene.cursorIndex -= 1;
				scene.cursorX -= TileWidth;
			}
		}

		public override void OnMoveRight(BattleScene scene) {
			if (scene.cursorX < TileWidth * (ColumnCount - 1) + CursorOffset) {
				scene.cursorIndex += 1;
				scene.cursorX += TileWidth;
			}
		}
	}

	private sealed class BuildState : State<BattleScene> {
		public override void OnConfirm(BattleScene scene) {
			if (scene.cheese >= scene.towers[scene.towerIndex].cost) {
				scene.cheese -= scene.towers[scene.towerIndex].cost;
				scene.tiles[scene.cursorIndex] = scene.towerIndex;
				scene.state = new SelectState();
			}
		}

		public override void OnCancel(BattleScene scene) {
			scene.state = new SelectState();
		}

		public override void OnMoveUp(BattleScene scene) {
			if (scene.towerIndex > 0) {
				scene.towerIndex--;
			}
		}

		public override void OnMoveDown(BattleScene scene) {
			if (scene.towerIndex < TowerCount) {
				scene.towerIndex++;
			}
		}

		// Consider putting textures in the state itself
		public override void Draw(NotatoGame game, BattleScene scene) {
			if (scene.towerIndex > 0) {
				game.Draw(scene.arrowUpTexture, 21, 81);
			}

			if (scene.towerIndex < TowerCount) {
				game.Draw(scene.arrowDownTexture, 21, 86);
			}

			game.DrawString(scene.towers[scene.towerIndex].name, 27, 81);
			game.DrawString(new[] { scene.towers[scene.towerIndex].cost }, 154, 81);
		}
	}
}
