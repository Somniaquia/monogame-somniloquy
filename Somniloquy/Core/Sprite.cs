namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Linq;
    
    public class Animation {
        public Sprite ParentSprite { get; set; }
        public List<int> FrameIndices { get; set; } = new();
        public List<Point> FrameOffsets { get; set; } = new();

        public Animation(Sprite parent) {
            ParentSprite = parent;
        }
    }    

    public class Sprite {
        public SpriteSheet SpriteSheet { get; set; }
        public Dictionary<string, Animation> Animations { get; set; } = new();

        public Animation CurrentAnimation { get; set; }
        public int CurrentAnimationFrame { get; set; }

        public Sprite(SpriteSheet spriteSheet) {
            SpriteSheet = spriteSheet;
        }

        public void AddAnimation(string name) {
            var animation = new Animation(this);
            Animations.Add(name, animation);
            CurrentAnimation = animation;
        }

        public void AddFrame(string animationName, int frameIndex, Point frameOffset) {
            Animations[animationName].FrameIndices.Add(frameIndex);
            Animations[animationName].FrameOffsets.Add(frameOffset);
        }

        public void PaintOnFrame(Color?[,] colors) {
            SpriteSheet.PaintOnFrame(colors, CurrentAnimation.FrameIndices[CurrentAnimationFrame]);
        }

        public Color?[,] GetFrameColors() {
            return SpriteSheet.GetFrameColors(CurrentAnimation.FrameIndices[CurrentAnimationFrame]);
        }

        public void Draw(Rectangle destination, float opacity = 1f) {
            GameManager.SpriteBatch.Draw(SpriteSheet.RawSpriteSheet, destination, new Rectangle(new Point(0, CurrentAnimation.FrameIndices[CurrentAnimationFrame] * SpriteSheet.FrameSize.Y), SpriteSheet.FrameSize), Color.White * opacity);
        }
    }
}