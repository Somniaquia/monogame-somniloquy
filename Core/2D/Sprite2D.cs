namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public interface ISpriteSheet {
        public Color GetColor(Vector2I position);
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain);
        public void SetPixel(Vector2I position, Color color, CommandChain chain);
        public void Draw(Rectangle destination, Rectangle source, Color color, SpriteEffects effects = SpriteEffects.None);
    }

    public class TileSpriteSheet : ISpriteSheet {
        [JsonInclude] public int SheetChunkLength;
        [JsonInclude] public int TileLength;
        [JsonInclude] public List<SQTexture2D> SheetChunks = new();
        [JsonInclude] private HashSet<Vector2I> UnoccupiedTileSlots = new();

        public TileSpriteSheet(int tileLength, int sheetChunkLength) {
            SheetChunkLength = sheetChunkLength;
            TileLength = tileLength;
            AddSheetChunk();
        }

        public void AddSheetChunk() {
            for (int y  = 0; y < SheetChunkLength; y++) {
                for (int x = 0; x < SheetChunkLength; x++) {
                    UnoccupiedTileSlots.Add(new Vector2I(x, y));
                }
            }
        }

        public Rectangle AllocateSpace() {
            if (UnoccupiedTileSlots.Count == 0) {
                AddSheetChunk();
            }
            var tilePos = UnoccupiedTileSlots.First();
            UnoccupiedTileSlots.Remove(tilePos);
            return new Rectangle(tilePos * TileLength, new(TileLength));
        }

        public void RemoveTile(Vector2I tilePosition) {
            UnoccupiedTileSlots.Add(tilePosition);
        }

        public Color GetColor(Vector2I position) {
            return Color.Purple;
        }

        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) {
        }

        public void SetPixel(Vector2I position, Color color, CommandChain chain) {
        }

        public void Draw(Rectangle destination, Rectangle source, Color color, SpriteEffects effects = SpriteEffects.None) {
            
        }
    }

    public class PackedSpriteSheet : SQTexture2D, ISpriteSheet {
        // TODO: Implement either the MaxRects algorithm or Guillotine algorithm to pack frames into one sheet
        public PackedSpriteSheet(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height) { }
    }

    public interface IAnimationFrame2D {
        public Color GetColor(Vector2I position);
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain);
        public void SetPixel(Vector2I position, Color color, CommandChain chain);
        public void Draw(Rectangle destination, Color color, SpriteEffects effects = SpriteEffects.None);
    }

    public class SheetAnimationFrame2D : IAnimationFrame2D {
        [JsonInclude] public ISpriteSheet SpriteSheet;
        [JsonInclude] public Rectangle SourceRect;

        public Color GetColor(Vector2I position) {
            return SpriteSheet.GetColor(GetSheetPosition(position));
        }

        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) {
            SpriteSheet.PaintPixel(GetSheetPosition(position), color, opacity, chain);
        }

        public void SetPixel(Vector2I position, Color color, CommandChain chain) {
            SpriteSheet.SetPixel(GetSheetPosition(position), color, chain);
        }

        private Vector2I GetSheetPosition(Vector2I position) {
            var dimensions = SourceRect.Size;
            if (position.X < 0 || position.Y < 0 || position.X > dimensions.X || position.Y > dimensions.Y) throw new Exception("Queried Position outside of frame bounds ");

            return position + (Vector2I)dimensions;
        }

        public void Draw(Rectangle destination, Color color, SpriteEffects effects) {
            SpriteSheet.Draw(destination, SourceRect, color, effects);
        }
    }

    public class Animation2D {
        [JsonInclude] public List<IAnimationFrame2D> Frames = new();
        [JsonInclude] public float FrameDuration = 0.2f;
        [JsonIgnore] public int CurrentFrameIndex;
        [JsonIgnore] public IAnimationFrame2D CurrentFrame { get {return Frames[CurrentFrameIndex];} }
        
        public Color GetColor(Vector2I position) => CurrentFrame.GetColor(position);
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) => CurrentFrame.PaintPixel(position, color, opacity, chain);
        public void SetPixel(Vector2I position, Color color, CommandChain chain) => CurrentFrame.SetPixel(position, color, chain);

        public Animation2D AddFrame(ISpriteSheet spriteSheet, Rectangle sourceRect) {
            Frames.Add(new SheetAnimationFrame2D { SpriteSheet = spriteSheet, SourceRect = sourceRect });
            return this;
        }

        public Animation2D SetFrameDuration(float duration) {
            FrameDuration = duration;
            return this;
        }

        public Animation2D SetStartFrame(int index) {
            if (index >= 0 && index < Frames.Count) CurrentFrameIndex = index;
            return this;
        }

        public void Update() {

        }

        public void Draw(Rectangle destination, Color color, SpriteEffects effects = SpriteEffects.None) {
            CurrentFrame.Draw(destination, color, effects);
        }
    }

    public class Sprite2D {
        [JsonInclude] public Dictionary<string, Animation2D> Animations = new();
        [JsonIgnore] public Animation2D CurrentAnimation;

        public Sprite2D AddAnimation(string name, Animation2D animation) {
            Animations[name] = animation;
            return this;
        }

        public Sprite2D SetCurrentAnimation(string name) {
            if (Animations.TryGetValue(name, out var animation)) {
                CurrentAnimation = animation;
            }
            return this;
        }

        public Color GetColor(Vector2I position) => CurrentAnimation.GetColor(position);
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) => CurrentAnimation.PaintPixel(position, color, opacity, chain);
        public void SetPixel(Vector2I position, Color color, CommandChain chain) => CurrentAnimation.SetPixel(position, color, chain);

        public void Update() {
            CurrentAnimation?.Update();
        }

        public void Draw(Rectangle destination, Color color, SpriteEffects effects = SpriteEffects.None) {
            CurrentAnimation?.Draw(destination, color, effects);
        }

        public string Serialize() {
            var options = new JsonSerializerOptions {
                WriteIndented = true, 
                ReferenceHandler = ReferenceHandler.Preserve,
            };

            return JsonSerializer.Serialize(this, options);
        }

        public static Section2D Deserialize(string json) {
            var options = new JsonSerializerOptions {
                WriteIndented = true, 
                ReferenceHandler = ReferenceHandler.Preserve,
            };

            return JsonSerializer.Deserialize<Section2D>(json, options);
        }
    }
}