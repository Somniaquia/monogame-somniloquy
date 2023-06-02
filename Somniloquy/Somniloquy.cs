namespace Somniloquy {
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class Somniloquy : Game {
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;

        public Somniloquy() {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            ResourceManager.GraphicsDeviceManager = graphicsDeviceManager;
            ResourceManager.ContentManager = Content;
        }

        protected override void Initialize() {
            SerializationManager.InitializeDirectories(
                (typeof(World), "Resources/Worlds"), 
                (typeof(FunctionalSprite), "Resources/SpriteInformations")
            );
            base.Initialize();
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ResourceManager.SpriteBatch = spriteBatch;
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            ResourceManager.GameTime = gameTime;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.AliceBlue);

            

            base.Draw(gameTime);
        }
    }
}