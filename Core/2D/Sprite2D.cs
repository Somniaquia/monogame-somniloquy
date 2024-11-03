namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public interface ISpriteSheet {
        public Color GetColor(Vector2I position);
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain);
        public void SetPixel(Vector2I position, Color color, CommandChain chain);
        public void Draw(Rectangle destination, Rectangle source, Color color, SpriteEffects effects = SpriteEffects.None);
    }

    public class ProceduralTileSpriteSheet : ISpriteSheet {
        public int SheetChunkLength;
        public int TileLength;
        public List<SQTexture2D> SheetChunks = new();
        private HashSet<Vector2I> UnoccupiedTileSlots = new();

        public ProceduralTileSpriteSheet(int tileLength, int sheetChunkLength) {
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

        public void AddTile() {
            if (UnoccupiedTileSlots.Count == 0) {
                AddSheetChunk();
            }
            var tilePos = UnoccupiedTileSlots.First();
            UnoccupiedTileSlots.Remove(tilePos);
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
        ISpriteSheet SpriteSheet;
        Rectangle SourceRect;

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
        public List<IAnimationFrame2D> Frames = new();
        public int CurrentFrameIndex;
        public IAnimationFrame2D CurrentFrame { get {return Frames[CurrentFrameIndex];} }
        
        public Color GetColor(Vector2I position) => CurrentFrame.GetColor(position);
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) => CurrentFrame.PaintPixel(position, color, opacity, chain);
        public void SetPixel(Vector2I position, Color color, CommandChain chain) => CurrentFrame.SetPixel(position, color, chain);

        public void Draw(Rectangle destination, Color color, SpriteEffects effects = SpriteEffects.None) {
            CurrentFrame.Draw(destination, color, effects);
        }
    }

    public class Sprite2D {
        public Dictionary<string, Animation2D> Animations = new();
        public Animation2D CurrentAnimation;

        public void ChangeAnimation() {
            
        }

        public Color GetColor(Vector2I position) => CurrentAnimation.GetColor(position);
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) => CurrentAnimation.PaintPixel(position, color, opacity, chain);
        public void SetPixel(Vector2I position, Color color, CommandChain chain) => CurrentAnimation.SetPixel(position, color, chain);

        public void Update() {

        }

        public void Draw(Rectangle destination, Color color, SpriteEffects effects = SpriteEffects.None) {
            CurrentAnimation?.Draw(destination, color, effects);
        }
    }
}