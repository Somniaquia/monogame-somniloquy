namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Linq;

    /// <summary>
    /// An animation contains these information:
    /// - the name of the animation that will be used to assign same animation to different functional sprites easily
    /// - the name of the spritesheet that derives the animation
    /// - list of boundaries of sprites in a sprite sheet
    /// </summary> 
    public class Animation {
        public string AnimationName { get; set; }
        [JsonConverter(typeof(Texture2DConverter))]
        public Texture2D SpriteSheet { get; set; }
        public List<Rectangle> FrameBoundaries { get; set; } = new();
        public List<Point> FrameCenters { get; set; } = new();

        public Animation(string name) {
            AnimationName = name;
        }

        public static Texture2D AppendTextureOnRight(Texture2D texture1, Texture2D texture2) {
            int width = texture1.Width + texture2.Width;
            int height = Math.Max(texture1.Height, texture2.Height);

            Color[] data = new Color[width * height];

            Color[] texture1Data = new Color[texture1.Width * texture1.Height];
            texture1.GetData(texture1Data);
            for (int y = 0; y < texture1.Height; y++) {
                for (int x = 0; x < texture1.Width; x++) {
                    data[y * width + x] = texture1Data[y * texture1.Width + x];
                }
            }

            Color[] texture2Data = new Color[texture2.Width * texture2.Height];
            texture2.GetData(texture2Data);
            for (int y = 0; y < texture2.Height; y++) {
                for (int x = 0; x < texture2.Width; x++) {
                    data[y * width + texture1.Width + x] = texture2Data[y * texture2.Width + x];
                }
            }

            Texture2D mergedTexture = new(GameManager.GraphicsDeviceManager.GraphicsDevice, width, height);
            mergedTexture.SetData(data);

            texture1.Dispose();
            texture2.Dispose();
            return mergedTexture;
        }

        public static Texture2D MergeTextures(Texture2D texture, Color?[,] colors, Rectangle boundaries) {
            Color[] textureData = new Color[texture.Width * texture.Height];
            texture.GetData(textureData);

            for (int y = 0; y < boundaries.Bottom; y++) {
                for (int x = 0; x < boundaries.Right; x++) {
                    if (colors[x, y] is not null) {
                        textureData[(boundaries.Y + y) * texture.Width + (boundaries.X + x)] = colors[x, y].Value;
                    }
                }
            }

            var newTexture = new Texture2D(GameManager.GraphicsDeviceManager.GraphicsDevice, texture.Width, texture.Height);
            newTexture.SetData(textureData);

            return newTexture;
        }

        public void AddFrame(Texture2D frame) {
            if (SpriteSheet is null) {
                SpriteSheet = frame;
                FrameBoundaries.Add(new Rectangle(0, 0, frame.Width, frame.Height));
            } else {
                FrameBoundaries.Add(new Rectangle(SpriteSheet.Width, 0, frame.Width, frame.Height));
                SpriteSheet = AppendTextureOnRight(SpriteSheet, frame);
            }

            // TODO: Calculate  anchors
            FrameCenters.Add(new Point(0, 0));
        }

        public void PaintOnFrame(Color?[,] colors, int frameIndex) {
            SpriteSheet = MergeTextures(SpriteSheet, colors, FrameBoundaries[frameIndex]);
        }

        public Color?[,] GetFrameColors(int frameIndex) {
            Rectangle boundaries = FrameBoundaries[frameIndex];

            var data = new Color[boundaries.Width * boundaries.Height];
            SpriteSheet.GetData(0, new Rectangle(boundaries.X, boundaries.Y, boundaries.Width, boundaries.Height), data, 0, boundaries.Width * boundaries.Height);

            var colors = new Color?[boundaries.Width, boundaries.Height];
            for (int y = 0; y < boundaries.Height; y++) {
                for (int x = 0; x < boundaries.Width; x++) {
                    colors[x, y] = data[y * boundaries.Width + x];
                }
            }
            return colors;
        }
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
        public Dictionary<string, Animation> Animations { get; private set; } = new();

        private string CurrentAnimationName { get; set; } = null;
        public int FrameInCurrentAnimation { get; private set; } = 0;

        public Animation AddAnimation(string name) {
            var animation = new Animation(name);
            Animations.Add(name, animation);
            CurrentAnimationName = name;
            return animation;
        }

        public Animation GetCurrentAnimation() {
            CurrentAnimationName ??= Animations.Keys.First();
            return Animations[CurrentAnimationName];
        }

        public Rectangle GetSourceRectangle() {
            return GetCurrentAnimation().FrameBoundaries[FrameInCurrentAnimation];
        }

        public Rectangle GetDestinationRectangle(Point point) {
            var offset = GetCurrentAnimation().FrameCenters[FrameInCurrentAnimation];
            var boundaries = GetCurrentAnimation().FrameBoundaries[FrameInCurrentAnimation];
            return new Rectangle(point.X + offset.X, point.Y + offset.Y, boundaries.Width, boundaries.Height);
        }
        
        public void AdvanceFrames(int frames = 1) {
            FrameInCurrentAnimation = (FrameInCurrentAnimation + frames) % GetCurrentAnimation().FrameBoundaries.Count;
        }

        public void Dispose() {
            foreach (var animation in Animations) {
                animation.Value.SpriteSheet?.Dispose();
            }

            Animations.Clear();
        }
    }
}