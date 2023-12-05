using MGLib;
using System;

internal sealed class Battle : Superstate<Battle> {
	private const byte TowerDatumCount = 4;
	private const byte BattlefieldRowCount = 5;
	private const byte BattlefieldColumnCount = 9;
	private const byte TilePixelWidth = 17;
	private const byte TilePixelHeight = 15;
	private const byte BattlefieldPixelOffset = 4;

	// For the size, TowerDatumCount + 1 is used to accomodate the datum used by empty tileData.
	private readonly TowerDatum[] towerData = new TowerDatum[TowerDatumCount + 1] {
		new TowerDatum(new byte[] {}, "null", 0, (Battle battle) => {}, 0),
		new TowerDatum(new byte[] { 1 }, "tower_1", 2, (Battle battle) => {
			battle.moneyClock.Tick(battle);
		}, 1),
		new TowerDatum(new byte[] { 2 }, "tower_2", 5, (Battle battle) => {}, 0),
		new TowerDatum(new byte[] { 3 }, "tower_3", 4, (Battle battle) => {
			Projectile headProjectile = battle.headProjectile;
			battle.headProjectile = new Projectile(0);
			battle.headProjectile.next = headProjectile;
		}, 90),
		new TowerDatum(new byte[] { 4 }, "tower_2", 2, (Battle battle) => {}, 0)
	};

	private readonly ProjectileDatum[] projectileData = new ProjectileDatum[] {
		new ProjectileDatum("tower_1", 1, 2)
	};

	private readonly EnemyDatum[] enemyData = new EnemyDatum[] {
		new EnemyDatum("tower_2", 10, 1),
		new EnemyDatum("tower_2", 28, 1),
		new EnemyDatum("tower_2", 17, 2),
		new EnemyDatum("tower_2", 65, 1)
	};

	private readonly Tile[,] tileData = new Tile[BattlefieldRowCount, BattlefieldColumnCount];

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

		for (byte row = 0; row < BattlefieldRowCount; row += 1) {
			for (byte col = 0; col < BattlefieldColumnCount; col += 1) {
				tileData[row, col] = new Tile(this, 0);
			}
		}

		Enemy headEnemy = this.headEnemy;
		this.headEnemy = new Enemy(0);
		this.headEnemy.next = headEnemy;
	}

	protected override void Update() {
		moneyClock.Tick(this);

		foreach (Tile tile in tileData) {
			tile.clock.Tick(this);
		}

		Projectile previousProjectile = null;
		Projectile currentProjectile = headProjectile;
		while (currentProjectile != null) {
			currentProjectile.position += projectileData[currentProjectile.id].speed;

			if (currentProjectile.position >= TilePixelWidth * BattlefieldColumnCount + BattlefieldPixelOffset) {
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
			currentEnemy.position -= enemyData[currentEnemy.id].speed;

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
		
		for (byte row = 0; row < BattlefieldRowCount; row += 1) {
			for (byte col = 0; col < BattlefieldColumnCount; col += 1) {
				Game.Draw(towerData[tileData[row, col].tower].sprite, BattlefieldPixelOffset + col * TilePixelWidth, BattlefieldPixelOffset + row * TilePixelHeight);
			}
		}

		Projectile currentProjectile = headProjectile;
		while (currentProjectile != null) {
			Game.Draw(projectileData[currentProjectile.id].sprite, currentProjectile.position, 5);
			currentProjectile = currentProjectile.next;
		}

		Enemy currentEnemy = headEnemy;
		while (currentEnemy != null) {
			Game.Draw(enemyData[currentEnemy.id].sprite, currentEnemy.position, 50);
			currentEnemy = currentEnemy.next;
		}

		Game.Draw(cursorTexture, BattlefieldPixelOffset + cursorColumn * TilePixelWidth, BattlefieldPixelOffset + cursorRow * TilePixelHeight);

		substate.Draw(this);
	}

	internal readonly struct TowerDatum {
		internal readonly byte[] name;
		internal readonly Sprite sprite;
		internal readonly byte cost;
		internal readonly ushort period;
		internal readonly Action<Battle> update;

		internal TowerDatum(byte[] name, string sprite, byte cost, Action<Battle> update, ushort period) {
			this.name = name;
			this.sprite = new Sprite(sprite);
			this.cost = cost;
			this.update = update;
			this.period = period;
		}
	}

	internal readonly struct ProjectileDatum {
		internal readonly Sprite sprite;
		internal readonly byte damage;
		internal readonly byte speed;

		internal ProjectileDatum(string sprite, byte damage, byte speed) {
			this.sprite = new Sprite(sprite);
			this.damage = damage;
			this.speed = speed;
		}
	}

	internal readonly struct EnemyDatum {
		internal readonly Sprite sprite;
		internal readonly byte speed;
		internal readonly byte health;

		internal EnemyDatum(string sprite, byte health, byte speed) {
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
			clock = new Clock<Battle>(battle.towerData[tower].period, battle.towerData[tower].update);
		}
	}

	// A linked list structure
	internal class Node<T> where T : Node<T> {
		internal T next;
	}

	// A living instance of a projectile
	internal sealed class Projectile : Node<Projectile> {
		internal byte id;
		internal ushort position = BattlefieldPixelOffset;

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
			if (superstate.cursorRow < BattlefieldRowCount - 1) {
				superstate.cursorRow += 1;
			}
		}

		public override void OnMoveLeft(Battle superstate) {
			if (superstate.cursorColumn > 0) {
				superstate.cursorColumn -= 1;
			}
		}

		public override void OnMoveRight(Battle superstate) {
			if (superstate.cursorColumn < BattlefieldColumnCount - 1) {
				superstate.cursorColumn += 1;
			}
		}
	}

	// The state when a tower is being built
	private sealed class BuildState : Substate<Battle> {
		private byte towerIndex;

		public override void OnConfirm(Battle superstate) {
			if (superstate.money >= superstate.towerData[towerIndex].cost) {
				superstate.money -= superstate.towerData[towerIndex].cost;
				superstate.tileData[superstate.cursorRow, superstate.cursorColumn] = new Tile(superstate, towerIndex);
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
			if (towerIndex < TowerDatumCount) {
				towerIndex += 1;
			}
		}

		public override void Draw(Battle superstate) {
			if (towerIndex > 0) {
				Game.Draw(superstate.arrowUpTexture, 21, 81);
			}

			if (towerIndex < TowerDatumCount) {
				Game.Draw(superstate.arrowDownTexture, 21, 86);
			}

			Game.DrawString(superstate.towerData[towerIndex].name, 27, 81);
			Game.DrawString(new[] { superstate.towerData[towerIndex].cost }, 154, 81);
		}
	}
}
