namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;

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

            GameManager.GraphicsDeviceManager = graphicsDeviceManager;
            GameManager.ContentManager = Content;
            GameManager.WindowSize = new Rectangle(0, 0, graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight);
        }

        protected override void Initialize() {
            base.Initialize();
            activeScreens.Add(new EditorScreen(GameManager.WindowSize));

            InputManager.Initialize(Window);
            SerializationManager.InitializeDirectories((typeof(World), "Worlds"));
        }

        protected override void LoadContent() {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);

            GameManager.SpriteBatch = spriteBatch;
            Texture2D pixel = new(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            GameManager.Pixel = pixel;

            //GameManager.Misaki = Content.Load<BitmapFont>("misaki");
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            GameManager.GameTime = gameTime;

            if (IsActive)
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