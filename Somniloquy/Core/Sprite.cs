namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Linq;
    
    public struct Animation {
        public List<Point> FramePositions = new();
        public List<Point> FrameOffsets = new();

        public Animation() { }
    }    

    public class Sprite {
        public SpriteSheet SpriteSheet { get; set; }
        public Dictionary<string, Animation> Animations { get; set; } = new();

        public Animation CurrentAnimation;
        public int CurrentAnimationFrame;

        public Sprite(SpriteSheet spriteSheet) {
            SpriteSheet = spriteSheet;
        }

        public void AddAnimation(string name) {
            var animation = new Animation();
            Animations.Add(name, animation);
            CurrentAnimation = animation;
        }

        public void AddFrame(string animationName, Point framePosition, Point frameOffset) {
            Animations[animationName].FramePositions.Add(framePosition);
            Animations[animationName].FrameOffsets.Add(frameOffset);
        }

        public void PaintOnFrame(Color?[,] colors) {
            SpriteSheet.PaintOnFrame(colors, CurrentAnimation.FramePositions[CurrentAnimationFrame]);
        }

        public Color?[,] GetFrameColors() {
            return SpriteSheet.GetFrameColors(CurrentAnimation.FramePositions[CurrentAnimationFrame]);
        }

        public void Draw(Rectangle destination, float opacity = 1f) {
            GameManager.SpriteBatch.Draw(SpriteSheet.Texture, destination, new Rectangle(CurrentAnimation.FramePositions[CurrentAnimationFrame] * SpriteSheet.FrameSize, SpriteSheet.FrameSize), Color.White * opacity);
        }
    }
}