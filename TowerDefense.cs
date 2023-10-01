using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

new TowerDefense();

internal sealed class TowerDefense : Game {
	private enum Scene : byte {
		Title,
		Battle
	}

	private SpriteBatch spriteBatch;
	private Texture2D frameTexture;
	private Texture2D titleScreenTexture;
	private Texture2D sceneTexture;
	private Texture2D arrowUpTexture;
	private Texture2D arrowDownTexture;
	private Texture2D[] characters = new Texture2D[10];
	private Texture2D[] towers = new Texture2D[0];

	private float scaleFactor;
	private Vector2 offset;

	private byte timer = 0;
	private byte money = 0;

	private Scene scene = Scene.Title;

	internal TowerDefense() {
		int internalWidth = 160;
		int internalHeight = 90;

		int width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
		int height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
		scaleFactor = Math.Min(width / internalWidth, height / internalHeight);
		offset = new Vector2((width - internalWidth * scaleFactor) / 2, (height - internalHeight * scaleFactor) / 2);

		new GraphicsDeviceManager(this) {
			PreferredBackBufferWidth = width,
			PreferredBackBufferHeight = height,
			IsFullScreen = true
		};
		Run();
	}

	protected override void Initialize() {
		spriteBatch = new SpriteBatch(GraphicsDevice);

		Content.RootDirectory = "Content";
		frameTexture = Content.Load<Texture2D>("frame");
		titleScreenTexture = Content.Load<Texture2D>("title");

		characters[0] = Content.Load<Texture2D>("0");
		characters[1] = Content.Load<Texture2D>("1");
		characters[2] = Content.Load<Texture2D>("2");
		characters[3] = Content.Load<Texture2D>("3");
		characters[4] = Content.Load<Texture2D>("4");
		characters[5] = Content.Load<Texture2D>("5");
		characters[6] = Content.Load<Texture2D>("6");
		characters[7] = Content.Load<Texture2D>("7");
		characters[8] = Content.Load<Texture2D>("8");
		characters[9] = Content.Load<Texture2D>("9");

		arrowUpTexture = Content.Load<Texture2D>("arrow_up");
		arrowDownTexture = Content.Load<Texture2D>("arrow_down");

		sceneTexture = titleScreenTexture;

		base.Initialize();
	}

	protected override void Update(GameTime gameTime) {
		KeyboardState keyboard = Keyboard.GetState();

		if (scene == Scene.Title && keyboard[Keys.Z] == KeyState.Down) {
			sceneTexture = frameTexture;
			scene = Scene.Battle;
		} else if (scene == Scene.Battle) {
			if (timer < 60) {
				timer++;
			} else {
				timer = 0;
				if (money < 99) {
					money++;
				}
			}
		}
	}

	protected override void Draw(GameTime gameTime) {
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

		spriteBatch.Draw(sceneTexture, offset, null, Color.White, 0, Vector2.Zero, scaleFactor, SpriteEffects.None, 0);

		if (scene == Scene.Battle) {
			spriteBatch.Draw(characters[money / 10], new Vector2(7, 81) * scaleFactor + offset, null, Color.White, 0, Vector2.Zero, scaleFactor, SpriteEffects.None, 0);
			spriteBatch.Draw(characters[money % 10], new Vector2(13, 81) * scaleFactor + offset, null, Color.White, 0, Vector2.Zero, scaleFactor, SpriteEffects.None, 0);
			spriteBatch.Draw(arrowUpTexture, new Vector2(21, 81) * scaleFactor + offset, null, Color.White, 0, Vector2.Zero, scaleFactor, SpriteEffects.None, 0);
			spriteBatch.Draw(arrowDownTexture, new Vector2(21, 86) * scaleFactor + offset, null, Color.White, 0, Vector2.Zero, scaleFactor, SpriteEffects.None, 0);
		}

		spriteBatch.End();
	}
}
