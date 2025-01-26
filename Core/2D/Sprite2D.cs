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
        public void Draw(Camera2D camera, Rectangle destination, Rectangle source, Color color, SpriteEffects effects = SpriteEffects.None);
    }

    public class TileSpriteSheet : ISpriteSheet {
        [JsonInclude] public int SheetChunkLength;
        [JsonInclude] public int TileLength;

        [JsonInclude] public List<Tile2D> Tiles = new();
        [JsonInclude] public List<SQTexture2D> SheetChunks = new();
        [JsonInclude] public HashSet<Vector2I> UnoccupiedTileSlots = new();

        public TileSpriteSheet(int tileLength, int sheetChunkLength) {
            SheetChunkLength = sheetChunkLength;
            TileLength = tileLength;
        
            DebugInfo.Subscribe(() => $"Section Tiles count: {SheetChunkLength * SheetChunkLength * SheetChunks.Count - UnoccupiedTileSlots.Count}");
        }
        
        public void AddSheetChunk() {
            SheetChunks.Add(new SQTexture2D(SQ.GD, TileLength * SheetChunkLength, TileLength * SheetChunkLength));

            for (int y  = 0; y < SheetChunkLength; y++) {
                for (int x = 0; x < SheetChunkLength; x++) {
                    UnoccupiedTileSlots.Add(new Vector2I(x, y + SheetChunkLength * (SheetChunks.Count - 1)));
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
            return SheetChunks[position.Y / (SheetChunkLength * TileLength)].GetColor(new Vector2I(position.X, position.Y % (SheetChunkLength * TileLength)));
        }

        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) {
            SheetChunks[position.Y / (SheetChunkLength * TileLength)].PaintPixel(new Vector2I(position.X, position.Y % (SheetChunkLength * TileLength)), color, opacity, chain);
        }

        public void SetPixel(Vector2I position, Color color, CommandChain chain) {
            SheetChunks[position.Y / (SheetChunkLength * TileLength)].SetPixel(new Vector2I(position.X, position.Y % (SheetChunkLength * TileLength)), color, chain);
        }

        public void Draw(Camera2D camera, Rectangle destination, Rectangle source, Color color, SpriteEffects effects = SpriteEffects.None) {
            camera.Draw(SheetChunks[source.Y / (SheetChunkLength * TileLength)], destination, new Rectangle(source.X, source.Y % (SheetChunkLength * TileLength), source.Width, source.Height), color);
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
        public void Draw(Camera2D camera, Rectangle destination, Color color, SpriteEffects effects = SpriteEffects.None);
    }

    public class SheetAnimationFrame2D : IAnimationFrame2D {
        [JsonInclude] public ISpriteSheet SpriteSheet;
        [JsonInclude] public Rectangle SourceRect;

        public Color GetColor(Vector2I position) => SpriteSheet.GetColor(GetSheetPosition(position));
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) => SpriteSheet.PaintPixel(GetSheetPosition(position), color, opacity, chain);
        public void SetPixel(Vector2I position, Color color, CommandChain chain) => SpriteSheet.SetPixel(GetSheetPosition(position), color, chain);

        private Vector2I GetSheetPosition(Vector2I position) {
            var dimensions = SourceRect.Size;
            if (position.X < 0 || position.Y < 0 || position.X > dimensions.X || position.Y > dimensions.Y) throw new Exception("Queried Position outside of frame bounds ");

            return position + SourceRect.TopLeft();
        }

        public void Draw(Camera2D camera, Rectangle destination, Color color, SpriteEffects effects) {
            SpriteSheet.Draw(camera, destination, SourceRect, color, effects);
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

        public void Draw(Camera2D camera, Rectangle destination, Color color, SpriteEffects effects = SpriteEffects.None) {
            CurrentFrame.Draw(camera, destination, color, effects);
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

        public void Draw(Camera2D camera, Rectangle destination, Color color, SpriteEffects effects = SpriteEffects.None) {
            CurrentAnimation?.Draw(camera, destination, color, effects);
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