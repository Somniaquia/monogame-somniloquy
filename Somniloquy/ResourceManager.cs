namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Content;

    public static class ResourceManager {
        public static GraphicsDeviceManager GraphicsDeviceManager { get; set; }
        public static ContentManager ContentManager { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static GameTime GameTime { get; set; }
    }
}