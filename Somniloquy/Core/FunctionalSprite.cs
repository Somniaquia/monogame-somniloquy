namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// An animation contains these information:
    /// - the name of the animation that will be used to assign same animation to different functional sprites easily
    /// - the name of the spritesheet that derives the animation
    /// - list of boundaries of sprites in a sprite sheet
    /// </summary> 
    public struct Animation {
        public string AnimationName { get; set; }
        public Texture2D SpriteSheet { get; set; }
        public List<Rectangle> FrameBoundaries { get; set; }
        public List<Point> FrameAnchors { get; set; }
    }

    /// <summary>
    /// A functional sprite contains these information:
    /// - the name of the sprite that will be used to identify them 
    /// - animations included in the functional sprite
    /// - current animation states
    /// 
    /// A sprite does NOT contain these information:
    /// - position in the world
    /// - position in the screen
    /// thus the sprite does not contain the drawing functionality neither - Entity class will do that instead
    /// I only intend to use the FunctionalSprite class to efficiently control between animations.
    /// </summary>
    public class FunctionalSprite {
        public string SpriteName { get; set; }
        public Dictionary<string, Animation> Animations { get; private set; }

        public Animation CurrentAnimation { get; set; }
        public int FrameInCurrentAnimation { get; private set; } = 0;

        public void AddAnimation(string name, Animation animation) {
            Animations.Add(name, animation);
            animation.AnimationName = name;
        }

        public Rectangle GetSourceRectangle() {
            return CurrentAnimation.FrameBoundaries[FrameInCurrentAnimation];
        }

        public Point GetDestinationRectangleOffset() {
            return CurrentAnimation.FrameAnchors[FrameInCurrentAnimation];
        }
        
        public void AdvanceFrames(int frames = 1) {
            FrameInCurrentAnimation = (FrameInCurrentAnimation + frames) % CurrentAnimation.FrameBoundaries.Count;
        }

        public static void Serialize(FunctionalSprite fSprite) {
            string serialized = "";

            foreach (KeyValuePair<string, Animation> entry in fSprite.Animations) {
                serialized += $"{entry.Key} + || + {entry.Value.AnimationName} \n";
            }

            GameManager.WriteTextToFile(typeof(FunctionalSprite), fSprite.SpriteName, serialized);
        }

        public static FunctionalSprite Deserialize(string serialized) {
            var fSprite = new FunctionalSprite();

            // Set fSprite properties
            
            return fSprite;
        }
    }
}