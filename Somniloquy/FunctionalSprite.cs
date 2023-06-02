namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
    /// thus the sprite does not contain the drawing functionality neither - Entity class will do that instead
    /// I only intend to use the FunctionalSprite class to efficiently control between animations.
    /// </summary>
    public class FunctionalSprite {
        public string SpriteName { get; set; }
        public Dictionary<string, Animation> Animations { get; set; }

        public Animation CurrentAnimation { get; set; }
        public int FrameInCurrentAnimation { get; private set; }

        public void AdvanceFrames(int frames) {
            FrameInCurrentAnimation = (FrameInCurrentAnimation + frames) % CurrentAnimation.spriteBoundaries.Count;
        }

        public static void Serialize(FunctionalSprite fSprite) {
            string serialized = "";

            foreach (KeyValuePair<string, Animation> entry in fSprite.Animations) {
                serialized += $"{entry.Key} + || + {entry.Value.SpriteName} \n";
            }

            SerializationManager.WriteToFile(typeof(FunctionalSprite), fSprite.SpriteName, serialized);
        }

        public static FunctionalSprite Deserialize(string serialized) {
            var fSprite = new FunctionalSprite();

            // Set fSprite properties

            return fSprite;
        }
    }
}