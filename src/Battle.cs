using MGLib;
using System;
using System.Collections.Generic;

internal sealed class Battle : Superstate<Battle> {
	private const byte TilePixelWidth = 17;
	private const byte TilePixelHeight = 15;
	private const byte BattlefieldPixelOffset = 4;

	private readonly TowerDatum[] TowerData = [
		new([], "null", 0, (TowerClockData data) => {}, 0),
		new([ 1 ], "tower_1", 2, (TowerClockData data) => data.battle.MoneyClock.Tick(data.battle), 1),
		new([ 2 ], "tower_2", 5, (TowerClockData data) => {}, 0),
		new([ 3 ], "tower_3", 4, (TowerClockData data) => data.battle.Projectiles[data.row].Add(new(0, data.column)), 90),
		new([ 4 ], "tower_2", 2, (TowerClockData data) => {}, 0)
	];

	private readonly ProjectileDatum[] ProjectileData = [
		new("tower_1", 1, 2)
	];

	private readonly EnemyDatum[] EnemyData = [
		new("tower_2", 10, 1),
		new("tower_2", 28, 1),
		new("tower_2", 17, 2),
		new("tower_2", 65, 1)
	];

	private readonly Clock<Battle> MoneyClock = new(600, (Battle battle) => Clamper.IncrementToMax(ref battle.Money, 300));
	private readonly Tower[,] Towers = new Tower[5, 9];
	private readonly HashSet<Projectile>[] Projectiles;
	private readonly HashSet<Enemy>[] Enemies;
	private readonly Sprite Background = new("frame");
	private readonly Sprite ArrowUpTexture = new("arrow_up");
	private readonly Sprite ArrowDownTexture = new("arrow_down");
	private readonly Sprite CursorTexture = new("cursor");

	private byte Money = 20;
	private byte CursorRow;
	private byte CursorColumn;

	public Battle() : base(new SelectState()) {
		for (var row = 0; row < Towers.GetLength(0); row += 1) {
			for (var col = 0; col < Towers.GetLength(1); col += 1) {
				Towers[row, col] = new(0, TowerData);
			}
		};

		Projectiles = new HashSet<Projectile>[Towers.GetLength(0)];
		for (var i = 0; i < Towers.GetLength(0); i += 1) {
			Projectiles[i] = [];
		}

		Enemies = new HashSet<Enemy>[Towers.GetLength(0)];
		for (var i = 0; i < Towers.GetLength(0); i += 1) {
			Enemies[i] = [];
		}
	}

	protected override void Update() {
		MoneyClock.Tick(this);

		for (var row = 0; row < Towers.GetLength(0); row += 1) {
			for (var col = 0; col < Towers.GetLength(1); col += 1) {
				Towers[row, col].clock.Tick(new(this, row, col));
			}
		};

		foreach (var row in Projectiles) {
			foreach (var projectile in row) {
				projectile.position += ProjectileData[projectile.datumId].speed;

				if (projectile.position >= TilePixelWidth * Towers.GetLength(1) + BattlefieldPixelOffset) {
					row.Remove(projectile);
				}
			}
		}

		foreach (var row in Enemies) {
			foreach (var enemy in row) {
				enemy.position -= EnemyData[enemy.datumId].speed;

				if (enemy.position <= 0) {
					row.Remove(enemy);
				}
			}
		}
	}

	protected override void Draw() {
		Game.Draw(Background);
		Game.DrawString([ (byte) (Money / 10), (byte) (Money % 10) ], 7, 81);

		for (var row = 0; row < Towers.GetLength(0); row += 1) {
			for (var col = 0; col < Towers.GetLength(1); col += 1) {
				Game.Draw(TowerData[Towers[row, col].datumId].sprite, BattlefieldPixelOffset + col * TilePixelWidth, BattlefieldPixelOffset + row * TilePixelHeight);
			}
		}

		for (var row = 0; row < Enemies.Length; row += 1) {
			foreach (var enemy in Enemies[row]) {
				Game.Draw(EnemyData[enemy.datumId].sprite, enemy.position, BattlefieldPixelOffset + row * TilePixelHeight);
			}
		}

		for (var row = 0; row < Projectiles.Length; row += 1) {
			foreach (var projectile in Projectiles[row]) {
				Game.Draw(ProjectileData[projectile.datumId].sprite, projectile.position, BattlefieldPixelOffset + row * TilePixelHeight);
			}
		}

		Game.Draw(CursorTexture, BattlefieldPixelOffset + CursorColumn * TilePixelWidth, BattlefieldPixelOffset + CursorRow * TilePixelHeight);

		base.Draw();
	}

	private readonly struct TowerClockData {
		internal readonly Battle battle;
		internal readonly int row;
		internal readonly int column;

		internal TowerClockData(Battle battle, int row, int column) {
			this.battle = battle;
			this.row = row;
			this.column = column;
		}
	}

	private readonly struct TowerDatum {
		internal readonly byte[] name;
		internal readonly Sprite sprite;
		internal readonly byte cost;
		internal readonly ushort period;
		internal readonly Action<TowerClockData> update;

		internal TowerDatum(byte[] name, string sprite, byte cost, Action<TowerClockData> update, ushort period) {
			this.name = name;
			this.sprite = new(sprite);
			this.cost = cost;
			this.update = update;
			this.period = period;
		}
	}

	private readonly struct ProjectileDatum {
		internal readonly Sprite sprite;
		internal readonly byte damage;
		internal readonly byte speed;

		internal ProjectileDatum(string sprite, byte damage, byte speed) {
			this.sprite = new(sprite);
			this.damage = damage;
			this.speed = speed;
		}
	}

	private readonly struct EnemyDatum {
		internal readonly Sprite sprite;
		internal readonly byte health;
		internal readonly byte speed;

		internal EnemyDatum(string sprite, byte health, byte speed) {
			this.sprite = new(sprite);
			this.health = health;
			this.speed = speed;
		}
	}

	private readonly struct Tower {
		internal readonly int datumId;
		internal readonly Clock<TowerClockData> clock;

		internal Tower(int datumId, TowerDatum[] data) {
			this.datumId = datumId;
			clock = new(data[datumId].period, data[datumId].update);
		}
	}

	private sealed class Projectile {
		internal readonly int datumId;
		internal int position;

		internal Projectile(int datumId, int column) {
			this.datumId = datumId;
			position = BattlefieldPixelOffset + column * TilePixelWidth;
		}
	}

	private sealed class Enemy {
		internal readonly int datumId;
		internal byte health;
		internal ushort position = 160;

		internal Enemy(int datumId, EnemyDatum[] data) {
			this.datumId = datumId;
			health = data[datumId].health;
		}
	}

	private sealed class SelectState : Substate<Battle> {
		public override void OnConfirm(Battle battle) {
			battle.ChangeSubstate<BuildState>();
		}

		public override void OnMoveUp(Battle battle) {
			Clamper.DecrementToMin(ref battle.CursorRow, 0);
		}

		public override void OnMoveDown(Battle battle) {
			Clamper.IncrementToMax(ref battle.CursorRow, battle.Towers.GetLength(0));
		}

		public override void OnMoveLeft(Battle battle) {
			Clamper.DecrementToMin(ref battle.CursorColumn, 0);
		}

		public override void OnMoveRight(Battle battle) {
			Clamper.IncrementToMax(ref battle.CursorColumn, battle.Towers.GetLength(1));
		}
	}

	private sealed class BuildState : Substate<Battle> {
		private int TowerIndex;

		public override void OnConfirm(Battle battle) {
			if (battle.Money >= battle.TowerData[TowerIndex].cost) {
				battle.Money -= battle.TowerData[TowerIndex].cost;
				battle.Towers[battle.CursorRow, battle.CursorColumn] = new(TowerIndex, battle.TowerData);
				battle.ChangeSubstate<SelectState>();
			}
		}

		public override void OnCancel(Battle battle) {
			battle.ChangeSubstate<SelectState>();
		}

		public override void OnMoveUp(Battle battle) {
			Clamper.DecrementToMin(ref TowerIndex, 0);
		}

		public override void OnMoveDown(Battle battle) {
			Clamper.IncrementToMax(ref TowerIndex, battle.TowerData.Length);
		}

		public override void Draw(Battle battle) {
			if (TowerIndex > 0) {
				Game.Draw(battle.ArrowUpTexture, 21, 81);
			}

			if (TowerIndex < battle.TowerData.Length - 1) {
				Game.Draw(battle.ArrowDownTexture, 21, 86);
			}

			Game.DrawString(battle.TowerData[TowerIndex].name, 27, 81);
			Game.DrawString([ battle.TowerData[TowerIndex].cost ], 154, 81);
		}
	}
}