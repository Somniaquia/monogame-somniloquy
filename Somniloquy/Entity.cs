namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// An animation contains these information:
    /// - the name of the animation that will be used to assign same animation to different functional sprites easily
    /// - the name of the spritesheet that derives the animation
    /// - list of boundaries of sprites in a sprite sheet
    /// </summary> 
    public struct Animation {
        public string SpriteName { get; set; }
        public string SpriteSheetName { get; set; }
        public List<Rectangle> spriteBoundaries { get; set; }
        public List<Point> spriteCenters { get; set; }
    }

    /// <summary>
    /// A functional sprite contains these information:
    /// - the name of the sprite that will be used to identify them 
    /// - animations included in the functional sprite
    /// - current animation stats
    /// 
    /// A sprite does NOT contain these information:
    /// - position in the world
    /// - position in the screen
    /// I only intend to use the FunctionalSprite class to efficiently control between animations.
    /// </summary>
    public class FunctionalSprite {
        public string SpriteName { get; set; }
        public Dictionary<string, Animation> Animations { get; set; }

        private Animation currentAnimation;
        private int frameInCurrentAnimation;

        public static void Serialize(FunctionalSprite fSprite) {
            string serialized = "";

            foreach (KeyValuePair<string, Animation> entry in fSprite.Animations) {
                serialized += $"{entry.Key} + || + {entry.Value.SpriteName} \n";
            }

            SerializationManager.WriteFile(typeof(FunctionalSprite), fSprite.SpriteName, serialized);
        }

        public static void Deserialize(string serialized) {
            
        }
    }
}