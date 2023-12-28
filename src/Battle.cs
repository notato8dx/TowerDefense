using MGLib;
using System;
using System.Collections.Generic;

internal sealed class Battle : Superstate<Battle> {
	private const byte BattlefieldRowCount = 5;
	private const byte BattlefieldColumnCount = 9;
	private const byte BattlefieldPixelOffset = 4;

	private static readonly TowerDatum[] TowerData = [
		new([], "null", 0, _ => { }, 0),
		new([1], "tower_1", 2, data => data.battle.MoneyClock.Tick(data.battle), 1),
		new([2], "tower_2", 5, data => { }, 0),
		new([3], "tower_3", 4, data => data.battle.Projectiles[data.row].Add(new(0, data.column)), 90),
		new([4], "tower_2", 2, data => { }, 0)
	];

	private static readonly ProjectileDatum[] ProjectileData = [
		new("tower_1", 1, 2)
	];

	private static readonly EnemyDatum[] EnemyData = [
		new("tower_2", 10, 1),
		new("tower_2", 28, 1),
		new("tower_2", 17, 2),
		new("tower_2", 65, 1)
	];

	private static readonly Sprite Background = new("frame");
	private static readonly Sprite ArrowUpTexture = new("arrow_up");
	private static readonly Sprite ArrowDownTexture = new("arrow_down");
	private static readonly Sprite CursorTexture = new("cursor");

	private static ushort ColumnToPosition(byte column) {
		return (ushort)(BattlefieldPixelOffset + (column * 17));
	}

	private static ushort RowToPosition(byte row) {
		return (ushort)(BattlefieldPixelOffset + (row * 15));
	}

	private void ForEachItem(Action<Tower, byte, byte> towerAction, Action<Projectile, byte> projectileAction, Action<Enemy, byte> enemyAction) {
		Utility.ForEach(BattlefieldRowCount, row => {
			Utility.ForEach(BattlefieldColumnCount, column => towerAction(Towers[row, column], row, column));

			for (var i = Projectiles[row].Count - 1; i >= 0; i -= 1) {
				projectileAction(Projectiles[row][i], row);
			}

			for (var i = Enemies[row].Count - 1; i >= 0; i -= 1) {
				enemyAction(Enemies[row][i], row);
			}
		});
	}

	private readonly Clock<Battle> MoneyClock = new(600, (Battle battle) => Utility.IncrementToMax(ref battle.Money, 300));

	private readonly Tower[,] Towers = new Tower[BattlefieldRowCount, BattlefieldColumnCount] {
		{ new(0), new(0), new(0), new(0), new(0), new(0), new(0), new(0), new(0) },
		{ new(0), new(0), new(0), new(0), new(0), new(0), new(0), new(0), new(0) },
		{ new(0), new(0), new(0), new(0), new(0), new(0), new(0), new(0), new(0) },
		{ new(0), new(0), new(0), new(0), new(0), new(0), new(0), new(0), new(0) },
		{ new(0), new(0), new(0), new(0), new(0), new(0), new(0), new(0), new(0) }
	};

	private readonly List<Projectile>[] Projectiles = new List<Projectile>[BattlefieldRowCount] {
		[], [], [], [], []
	};

	private readonly List<Enemy>[] Enemies = new List<Enemy>[BattlefieldRowCount] {
		[], [], [], [], []
	};

	private byte Money = 20;
	private byte CursorRow;
	private byte CursorColumn;

	public Battle() : base(new SelectState()) {
		Enemies[0].Add(new(0));
	}

	protected override void Update() {
		MoneyClock.Tick(this);

		ForEachItem(
			(tower, row, column) => tower.clock.Tick(new(this, row, column)),
			(projectile, row) => {
				Enemy? enemy = Enemies[row].Find(enemy => {
					var difference = enemy.position - projectile.position;
					return difference >= 0 && difference < ProjectileData[projectile.datumId].speed;
				});

				if (enemy != null) {
					_ = Projectiles[row].Remove(projectile);

					enemy.health -= 1;
					if (enemy.health == 0) {
						_ = Enemies[row].Remove(enemy);
					}
				} else {
					projectile.position += ProjectileData[projectile.datumId].speed;

					if (projectile.position >= ColumnToPosition(BattlefieldColumnCount)) {
						_ = Projectiles[row].Remove(projectile);
					}
				}
			},
			(enemy, row) => enemy.clock.Tick(new(enemy, Enemies[row].Remove))
		);
	}

	protected override void Draw() {
		Game.Draw(Background);
		Game.DrawString([(byte)(Money / 10), (byte)(Money % 10)], 7, 81);

		ForEachItem(
			(tower, row, column) => Game.Draw(TowerData[tower.datumId].sprite, ColumnToPosition(column), RowToPosition(row)),
			(projectile, row) => Game.Draw(ProjectileData[projectile.datumId].sprite, projectile.position, RowToPosition(row)),
			(enemy, row) => Game.Draw(EnemyData[enemy.datumId].sprite, enemy.position, RowToPosition(row))
		);

		Game.Draw(CursorTexture, ColumnToPosition(CursorColumn), RowToPosition(CursorRow));

		base.Draw();
	}

	private readonly struct TowerClockData {
		internal readonly Battle battle;
		internal readonly byte row;
		internal readonly byte column;

		internal TowerClockData(Battle battle, byte row, byte column) {
			this.battle = battle;
			this.row = row;
			this.column = column;
		}
	}

	private readonly struct EnemyClockData {
		internal readonly Enemy enemy;
		internal readonly Func<Enemy, bool> removeFromRow;

		internal EnemyClockData(Enemy enemy, Func<Enemy, bool> removeFromRow) {
			this.enemy = enemy;
			this.removeFromRow = removeFromRow;
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

		internal Tower(int datumId) {
			this.datumId = datumId;
			clock = new(TowerData[datumId].period, TowerData[datumId].update);
		}
	}

	private sealed class Projectile {
		internal readonly int datumId;
		
		internal int position;

		internal Projectile(int datumId, byte column) {
			this.datumId = datumId;
			position = ColumnToPosition(column);
		}
	}

	private sealed class Enemy {
		internal readonly int datumId;

		internal readonly Clock<EnemyClockData> clock = new(20, (EnemyClockData data) => {
			data.enemy.position -= 1;
			if (data.enemy.position <= 0) {
				_ = data.removeFromRow(data.enemy);
			}
		});

		internal byte health;
		internal int position;

		internal Enemy(int datumId) {
			this.datumId = datumId;
			health = EnemyData[datumId].health;
			position = ColumnToPosition(BattlefieldColumnCount);
		}
	}

	private sealed class SelectState : Substate<Battle> {
		protected override void OnConfirm(Battle battle) {
			battle.ChangeSubstate<BuildState>();
		}

		protected override void OnMoveUp(Battle battle) {
			Utility.DecrementToMin(ref battle.CursorRow, 0);
		}

		protected override void OnMoveDown(Battle battle) {
			Utility.IncrementToMax(ref battle.CursorRow, BattlefieldRowCount);
		}

		protected override void OnMoveLeft(Battle battle) {
			Utility.DecrementToMin(ref battle.CursorColumn, 0);
		}

		protected override void OnMoveRight(Battle battle) {
			Utility.IncrementToMax(ref battle.CursorColumn, BattlefieldColumnCount);
		}
	}

	private sealed class BuildState : Substate<Battle> {
		private int TowerIndex;

		protected override void OnConfirm(Battle battle) {
			if (battle.Money >= TowerData[TowerIndex].cost) {
				battle.Money -= TowerData[TowerIndex].cost;
				battle.Towers[battle.CursorRow, battle.CursorColumn] = new(TowerIndex);
				battle.ChangeSubstate<SelectState>();
			}
		}

		protected override void OnCancel(Battle battle) {
			battle.ChangeSubstate<SelectState>();
		}

		protected override void OnMoveUp(Battle battle) {
			Utility.DecrementToMin(ref TowerIndex, 0);
		}

		protected override void OnMoveDown(Battle battle) {
			Utility.IncrementToMax(ref TowerIndex, TowerData.Length);
		}

		protected override void Draw(Battle battle) {
			if (TowerIndex > 0) {
				Game.Draw(ArrowUpTexture, 21, 81);
			}

			if (TowerIndex < TowerData.Length - 1) {
				Game.Draw(ArrowDownTexture, 21, 86);
			}

			TowerDatum towerDatum = TowerData[TowerIndex];
			Game.DrawString(towerDatum.name, 27, 81);
			Game.DrawString([towerDatum.cost], 154, 81);
		}
	}
}