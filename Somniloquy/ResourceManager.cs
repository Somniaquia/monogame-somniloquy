namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Content;

    public static class ResourceManager {
        public static GraphicsDeviceManager GraphicsDeviceManager { get; set; }
        public static ContentManager ContentManager { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static GameTime GameTime { get; set; }

        public static Dictionary<string, Texture2D> SpriteSheets { get; set; }

        public static void DrawFunctionalSprite(FunctionalSprite fSprite, Rectangle boundaries, Effect spriteEffect) {
            SpriteBatch.Draw(
                SpriteSheets[fSprite.CurrentAnimation.SpriteSheetName],
                boundaries,
                fSprite.CurrentAnimation.spriteBoundaries[fSprite.FrameInCurrentAnimation],
                Color.White);
        }
    }
}