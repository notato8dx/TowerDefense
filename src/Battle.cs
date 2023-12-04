using MGLib;
using System;

internal sealed class Battle : Superstate<Battle> {
	// The number of unique towers in the game
	private const byte TowerCount = 4;

	// The number of rows on the battlefield
	private const byte RowCount = 5;

	// The number of columns on the battlefield
	private const byte ColumnCount = 9;

	// The pixel width of a tile
	private const byte TileWidth = 17;

	// The pixel height of a tile
	private const byte TileHeight = 15;

	// The number of pixels from the origin (both x and y) until tiles start
	private const byte FieldOffset = 4;

	// The data for all towers
	private readonly Tower[] towers = new Tower[TowerCount + 1] {
		new Tower(new byte[] {}, "null", 0, (Battle battle) => {}, 0),
		new Tower(new byte[] { 1 }, "tower_1", 2, (Battle battle) => {
			battle.moneyClock.Tick(battle);
		}, 1),
		new Tower(new byte[] { 2 }, "tower_2", 5, (Battle battle) => {}, 0),
		new Tower(new byte[] { 3 }, "tower_3", 4, (Battle battle) => {
			Projectile headProjectile = battle.headProjectile;
			battle.headProjectile = new Projectile(0);
			battle.headProjectile.next = headProjectile;
		}, 90),
		new Tower(new byte[] { 4 }, "tower_2", 2, (Battle battle) => {}, 0)
	};

	// The data for all projectiles
	private readonly ProjectileData[] projectiles = new ProjectileData[] {
		new ProjectileData("tower_1", 1, 2)
	};

	// The data for all enemies
	private readonly EnemyData[] enemies = new EnemyData[] {
		new EnemyData("tower_2", 10, 1),
		new EnemyData("tower_2", 28, 1),
		new EnemyData("tower_2", 17, 2),
		new EnemyData("tower_2", 65, 1)
	};

	// The tile data for the current battle
	private readonly Tile[,] tiles = new Tile[RowCount, ColumnCount];

	// The clock that passively increments money
	private readonly Clock<Battle> moneyClock = new Clock<Battle>(600, (Battle battle) => {
		if (battle.money < 99) {
			battle.money += 1;
		}
	});

	private readonly Sprite background = new Sprite("frame");
	private readonly Sprite arrowUpTexture = new Sprite("arrow_up");
	private readonly Sprite arrowDownTexture = new Sprite("arrow_down");
	private readonly Sprite cursorTexture = new Sprite("cursor");

	private Projectile headProjectile;
	private Enemy headEnemy;

	private byte money = 20;
	private byte cursorRow;
	private byte cursorColumn;

	public Battle() {
		ChangeSubstate<SelectState>();

		for (byte row = 0; row < RowCount; row += 1) {
			for (byte col = 0; col < ColumnCount; col += 1) {
				tiles[row, col] = new Tile(this, 0);
			}
		}

		Enemy headEnemy = this.headEnemy;
		this.headEnemy = new Enemy(0);
		this.headEnemy.next = headEnemy;
	}

	protected override void Update() {
		moneyClock.Tick(this);

		foreach (Tile tile in tiles) {
			tile.clock.Tick(this);
		}

		Projectile previousProjectile = null;
		Projectile currentProjectile = headProjectile;
		while (currentProjectile != null) {
			currentProjectile.position += projectiles[currentProjectile.id].speed;

			if (currentProjectile.position >= TileWidth * ColumnCount + FieldOffset) {
				if (previousProjectile != null) {
					previousProjectile.next = currentProjectile.next;
				} else {
					headProjectile = currentProjectile.next;
				}
			}

			previousProjectile = currentProjectile;
			currentProjectile = currentProjectile.next;
		}

		Enemy previousEnemy = null;
		Enemy currentEnemy = headEnemy;
		while (currentEnemy != null) {
			currentEnemy.position -= enemies[currentEnemy.id].speed;

			if (currentEnemy.position <= 0) {
				if (previousEnemy != null) {
					previousEnemy.next = currentEnemy.next;
				} else {
					headEnemy = currentEnemy.next;
				}
			}

			previousEnemy = currentEnemy;
			currentEnemy = currentEnemy.next;
		}
	}

	protected override void Draw() {
		Game.Draw(background, 0, 0);
		Game.DrawString(new[] { (byte) (money / 10), (byte) (money % 10) }, 7, 81);
		
		for (byte row = 0; row < RowCount; row += 1) {
			for (byte col = 0; col < ColumnCount; col += 1) {
				Game.Draw(towers[tiles[row, col].tower].sprite, FieldOffset + col * TileWidth, FieldOffset + row * TileHeight);
			}
		}

		Projectile currentProjectile = headProjectile;
		while (currentProjectile != null) {
			Game.Draw(projectiles[currentProjectile.id].sprite, currentProjectile.position, 5);
			currentProjectile = currentProjectile.next;
		}

		Enemy currentEnemy = headEnemy;
		while (currentEnemy != null) {
			Game.Draw(enemies[currentEnemy.id].sprite, currentEnemy.position, 50);
			currentEnemy = currentEnemy.next;
		}

		Game.Draw(cursorTexture, FieldOffset + cursorColumn * TileWidth, FieldOffset + cursorRow * TileHeight);

		substate.Draw(this);
	}

	// The data for a type of tower
	internal readonly struct Tower {
		internal readonly byte[] name;
		internal readonly Sprite sprite;
		internal readonly byte cost;
		internal readonly ushort period;
		internal readonly Action<Battle> update;

		internal Tower(byte[] name, string sprite, byte cost, Action<Battle> update, ushort period) {
			this.name = name;
			this.sprite = new Sprite(sprite);
			this.cost = cost;
			this.update = update;
			this.period = period;
		}
	}

	// The data for a type of projectile
	internal readonly struct ProjectileData {
		internal readonly Sprite sprite;
		internal readonly byte damage;
		internal readonly byte speed;

		internal ProjectileData(string sprite, byte damage, byte speed) {
			this.sprite = new Sprite(sprite);
			this.damage = damage;
			this.speed = speed;
		}
	}

	// The data for a type of enemy
	internal readonly struct EnemyData {
		internal readonly Sprite sprite;
		internal readonly byte speed;
		internal readonly byte health;

		internal EnemyData(string sprite, byte health, byte speed) {
			this.sprite = new Sprite(sprite);
			this.health = health;
			this.speed = speed;
		}
	}

	// The data for a tile in the current battle
	internal readonly struct Tile {
		internal readonly byte tower;
		internal readonly Clock<Battle> clock;

		internal Tile(Battle battle, byte tower) {
			this.tower = tower;
			clock = new Clock<Battle>(battle.towers[tower].period, battle.towers[tower].update);
		}
	}

	// A linked list structure
	internal class Node<T> where T : Node<T> {
		internal T next;
	}

	// A living instance of a projectile
	internal sealed class Projectile : Node<Projectile> {
		internal byte id;
		internal ushort position = FieldOffset;

		internal Projectile(byte id) {
			this.id = id;
		}
	}

	// A living instance of an enemy
	internal sealed class Enemy : Node<Enemy> {
		internal byte id;
		internal byte health;
		internal ushort position = 160;

		internal Enemy(byte id) {
			this.id = id;
		}
	}

	// The state when the cursor can move
	private sealed class SelectState : Substate<Battle> {
		public override void OnConfirm(Battle superstate) {
			superstate.ChangeSubstate<BuildState>();
		}

		public override void OnMoveUp(Battle superstate) {
			if (superstate.cursorRow > 0) {
				superstate.cursorRow -= 1;
			}
		}

		public override void OnMoveDown(Battle superstate) {
			if (superstate.cursorRow < RowCount - 1) {
				superstate.cursorRow += 1;
			}
		}

		public override void OnMoveLeft(Battle superstate) {
			if (superstate.cursorColumn > 0) {
				superstate.cursorColumn -= 1;
			}
		}

		public override void OnMoveRight(Battle superstate) {
			if (superstate.cursorColumn < ColumnCount - 1) {
				superstate.cursorColumn += 1;
			}
		}
	}

	// The state when a tower is being built
	private sealed class BuildState : Substate<Battle> {
		private byte towerIndex;

		public override void OnConfirm(Battle superstate) {
			if (superstate.money >= superstate.towers[towerIndex].cost) {
				superstate.money -= superstate.towers[towerIndex].cost;
				superstate.tiles[superstate.cursorRow, superstate.cursorColumn] = new Tile(superstate, towerIndex);
				superstate.ChangeSubstate<SelectState>();
			}
		}

		public override void OnCancel(Battle superstate) {
			superstate.ChangeSubstate<SelectState>();
		}

		public override void OnMoveUp(Battle superstate) {
			if (towerIndex > 0) {
				towerIndex -= 1;
			}
		}

		public override void OnMoveDown(Battle superstate) {
			if (towerIndex < TowerCount) {
				towerIndex += 1;
			}
		}

		public override void Draw(Battle superstate) {
			if (towerIndex > 0) {
				Game.Draw(superstate.arrowUpTexture, 21, 81);
			}

			if (towerIndex < TowerCount) {
				Game.Draw(superstate.arrowDownTexture, 21, 86);
			}

			Game.DrawString(superstate.towers[towerIndex].name, 27, 81);
			Game.DrawString(new[] { superstate.towers[towerIndex].cost }, 154, 81);
		}
	}
}
