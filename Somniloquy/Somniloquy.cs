namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class Somniloquy : Game {
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;
        private List<Screen> activeScreens = new();

        public Somniloquy() {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            ResourceManager.GraphicsDeviceManager = graphicsDeviceManager;
            ResourceManager.ContentManager = Content;
            
        }

        protected override void Initialize() {
            base.Initialize();
            activeScreens.Add(new WorldScreen());
        }

        protected override void LoadContent() {
            base.LoadContent();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ResourceManager.SpriteBatch = spriteBatch;
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
            GraphicsDevice.Clear(Color.AliceBlue);

            foreach (var screen in activeScreens) {
                screen.Draw();
            }
            base.Draw(gameTime);
        }
    }
}