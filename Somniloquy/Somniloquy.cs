namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using MonoGame.Extended;

    // dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained

    public class Somniloquy : Game {
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;
        private List<Screen> activeScreens = new();

        public Somniloquy() {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphicsDeviceManager.HardwareModeSwitch = false;
            graphicsDeviceManager.IsFullScreen = false;
            graphicsDeviceManager.ApplyChanges();

            Window.IsBorderless = true;
            Window.Position = new Point(0, 0);

            ResourceManager.GraphicsDeviceManager = graphicsDeviceManager;
            ResourceManager.ContentManager = Content;
        }

        protected override void Initialize() {
            base.Initialize();
            activeScreens.Add(new WorldScreen());
            activeScreens.Add(new UIScreen());
        }

        protected override void LoadContent() {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ResourceManager.SpriteBatch = spriteBatch;
            Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            ResourceManager.Pixel = pixel;

            ResourceManager.Misaki = Content.Load<SpriteFont>("MisakiGothic2nd");
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            ResourceManager.GameTime = gameTime;
            InputManager.Update();
            foreach (var screen in activeScreens) {
                screen.Update();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            foreach (var screen in activeScreens) {
                screen.Draw();
            }
            base.Draw(gameTime);
        }
    }
}