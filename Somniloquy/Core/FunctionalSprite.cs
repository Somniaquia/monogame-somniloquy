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
    public class Animation {
        public string AnimationName { get; private set; }
        public Texture2D SpriteSheet { get; private set; }
        public List<Rectangle> FrameBoundaries { get; private set; } = new();
        public List<Point> FrameAnchors { get; private set; } = new();

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

            Texture2D mergedTexture = new Texture2D(GameManager.GraphicsDeviceManager.GraphicsDevice, width, height);
            mergedTexture.SetData(data);

            return mergedTexture;
        }

        public void MergeTextures(Texture2D texture1, Texture2D texture2, Rectangle boundaries, bool ignoreTransparency) {
            if (boundaries.X + texture2.Width > texture1.Width || boundaries.Y + texture2.Height > texture1.Height) {
                // Handle error or return if the smaller texture doesn't fit
                return;
            }

            Color[] textureData1 = new Color[texture1.Width * texture1.Height];
            texture1.GetData(textureData1);

            Color[] textureData2 = new Color[texture2.Width * texture2.Height];
            texture2.GetData(textureData2);

            for (int y = 0; y < boundaries.Right; y++) {
                for (int x = 0; x < boundaries.Bottom; x++) {
                    if (ignoreTransparency && textureData2[y * texture2.Width + x] == Color.Transparent) {
                        continue;
                    }
                    textureData1[(boundaries.Y + y) * texture1.Width + (boundaries.X + x)] = textureData2[y * texture2.Width + x];
                }
            }

            texture1.SetData(textureData1);
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
            FrameAnchors.Add(new Point(0, 0));
        }

        public void PaintOnFrame(Texture2D texture, int frameIndex, bool ignoreTransparency = true) {
            MergeTextures(SpriteSheet, texture, FrameBoundaries[frameIndex], ignoreTransparency);
        }

        public Texture2D GetFrameTexture(int frameIndex) {
            Rectangle boundaries = FrameBoundaries[frameIndex];

            Color[] data = new Color[boundaries.Width * boundaries.Height];
            SpriteSheet.GetData(0, new Rectangle(boundaries.X, boundaries.Y, boundaries.Width, boundaries.Height), data, 0, boundaries.Width * boundaries.Height);
            //Array.Fill(data, Color.Black);

            Texture2D frameTexture = new Texture2D(GameManager.GraphicsDeviceManager.GraphicsDevice, boundaries.Width, boundaries.Height);
            frameTexture.SetData(data);

            return frameTexture;
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

        public Animation CurrentAnimation { get; set; }
        public int FrameInCurrentAnimation { get; private set; } = 0;

        public Animation AddAnimation(string name) {
            var animation = new Animation(name);
            Animations.Add(name, animation);
            CurrentAnimation = animation;
            return animation;
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