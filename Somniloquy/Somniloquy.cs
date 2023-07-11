namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using MathNet.Numerics.LinearAlgebra;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;

    // dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained

    public class Somniloquy : Game {
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;

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
            InputManager.Initialize(Window);
            SerializationManager.InitializeDirectories((typeof(World), "Worlds"), (typeof(Texture2D), "Textures"));
            SoundManager.Initialize("Content\\Loops");

            ScreenManager.AddScreen(new EditorScreen(GameManager.WindowSize));
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

            if (IsActive) {
                InputManager.Update();
                ScreenManager.Update();
            }

            SoundManager.Update();
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            if (IsActive) {
                GraphicsDevice.Clear(Color.Black);
                ScreenManager.Draw();
                base.Draw(gameTime);
            }
        }
    }
}