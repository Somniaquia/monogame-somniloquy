namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    [JsonDerivedType(typeof(TileSpriteSheet), "TileSpriteSheet")]
    [JsonDerivedType(typeof(PackedSpriteSheet), "PackedSpriteSheet")]
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
        [JsonIgnore] public HashSet<Vector2I> UnoccupiedTileSlots = new();

        public TileSpriteSheet() { }
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

    [JsonDerivedType(typeof(SheetSpriteFrame2D), "SheetSpriteFrame2D")]
    public interface ISpriteFrame2D {
        public Color GetColor(Vector2I position);
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain);
        public void SetPixel(Vector2I position, Color color, CommandChain chain);
        public void Draw(Camera2D camera, Rectangle destination, Color color, SpriteEffects effects = SpriteEffects.None);
    }

    public class SheetSpriteFrame2D : ISpriteFrame2D {
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

    public class SpriteFrameCollection2D {
        [JsonInclude] public List<(string Name, List<string> Values)> Conditions = new();
        [JsonInclude] public Dictionary<string, ISpriteFrame2D> Frames = new();

        public SpriteFrameCollection2D() { }

        public SpriteFrameCollection2D(params (string Name, string InitialValue)[] conditions) {
            foreach (var (name, value) in conditions) {
                Conditions.Add((name, new List<string> { value }));
            }
        }

        public bool AddCondition(string name, string initialValue) {
            if (Conditions.Any(c => c.Name == name)) return false;
            Conditions.Add((name, new List<string> { initialValue }));

            foreach (var pair in Frames.ToList()) {
                string newKey = $"{pair.Key}|{initialValue}";
                Frames.Remove(pair.Key);
                Frames[newKey] = pair.Value;
            }

            return true;
        }

        public bool RenameCondition(string name, string newName) {
            var condition = Conditions.FirstOrDefault(c => c.Name == name);
            if (condition.Name == null) return false;

            int index = Conditions.FindIndex(c => c.Name == name);
            Conditions[index] = (newName, Conditions[index].Values);

            foreach (var pair in Frames.ToList()) {
                string[] parts = pair.Key.Split('|');
                if (parts.Length != Conditions.Count) continue;

                string newKey = string.Join("|", parts.Take(index).Concat(new string[] { newName }).Concat(parts.Skip(index + 1)));
                Frames.Remove(pair.Key);
                Frames[newKey] = pair.Value;
            }

            return true;
        }

        public bool RemoveCondition(string name) {
            var condition = Conditions.FirstOrDefault(c => c.Name == name);
            if (condition.Name == null) return false;

            int index = Conditions.FindIndex(c => c.Name == name);
            Conditions.RemoveAt(index);

            foreach (var pair in Frames.ToList()) {
                string[] parts = pair.Key.Split('|');
                if (parts.Length != Conditions.Count + 1) continue;

                string newKey = string.Join("|", parts.Take(index).Concat(parts.Skip(index + 1)));
                Frames.Remove(pair.Key);
                Frames[newKey] = pair.Value;
            }

            return true;
        }

        public void AddFrame(string[] conditionValues, string conditionName, string conditionValue) {
            
        }

        public ISpriteFrame2D GetFrame(params string[] conditionValues) {
            if (conditionValues.Length != Conditions.Count) return null;

            string key = string.Join("|", conditionValues);
            return Frames.TryGetValue(key, out var frame) ? frame : null;
        }
    }
}