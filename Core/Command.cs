namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public static class CommandManager {
        public static Stack<ICommand> UndoHistory = new();
        public static Stack<ICommand> RedoHistory = new();

        public static CommandChain AddCommandChain(CommandChain command) {
            UndoHistory.Push(command);
            RedoHistory.Clear();
            return command;
        }

        public static void Undo() {
            if (UndoHistory.Count > 0) {
                ICommand command = UndoHistory.Pop();
                command.Undo();
                RedoHistory.Push(command);
            }
        }

        public static void Redo() {
            if (RedoHistory.Count > 0) {
                ICommand command = RedoHistory.Pop();
                command.Redo();
                UndoHistory.Push(command);
            }
        }

        public static void Clear() {
            UndoHistory.Clear();
            RedoHistory.Clear();
        }
    }

    public interface ICommand {
        public abstract void Redo();
        public abstract void Undo();
    }

    public class CommandChain : ICommand {
        private List<ICommand> commands = new();
        public Dictionary<Vector2I, (Color, float)> AffectedPixels = new();

        public void AddCommand(ICommand command) {
            commands.Add(command);
        }

        public void Undo() { for (int i = commands.Count - 1; i >= 0; i--) commands[i].Undo(); }
        public void Redo() => commands.ForEach(command => command.Redo());
    }

    // public class LayerCommand : ICommand {
    //     private Layer2D parent;

    //     public LayerAddCommand(Layer2D parent, Layer2D child, int index) {
    //         this.parent = parent;
    //         this.child = child;
    //         this.index = index;
    //     }

    //     public void Undo() => parent.Layers.Remove(child);
    //     public void Redo() => parent.Layers.Insert(index, child);
    // }

    public class PixelChangeCommand : ICommand {
        private SQTexture2D target;
        private Vector2I position;
        private Color colorBefore, colorAfter;

        public PixelChangeCommand(SQTexture2D target, Vector2I position, Color colorBefore, Color colorAfter) {
            this.target = target;
            this.position = position;
            this.colorBefore = colorBefore;
            this.colorAfter = colorAfter;
        }

        public void Undo() => target.SetPixel(position, colorBefore);
        public void Redo() => target.SetPixel(position, colorAfter);
    }

    public class ChunkRegionChangeCommand : ICommand {
        private SQTexture2D target;
        private Rectangle region;
        private Color[] textureDataBefore, textureDataAfter;

        public ChunkRegionChangeCommand(SQTexture2D target, Rectangle region) {
            this.target = target;
            this.region = region;
            target.GetData(textureDataBefore, region.TopLeft().Unwrap(region.Width), region.Width * region.Height);
        }

        public void Undo() {
            target.GetData(textureDataAfter, region.TopLeft().Unwrap(region.Width), region.Width * region.Height);
            target.SetData(textureDataBefore, region.TopLeft().Unwrap(region.Width), region.Width * region.Height);
        }

        public void Redo() {
            target.SetData(textureDataAfter, region.TopLeft().Unwrap(region.Width), region.Width * region.Height);
        }
    }

    // public class ChunkChangeCommand : ICommand {
    //     private SQTexture2D target;
    //     private Texture2D textureBefore, textureAfter;

    //     public ChunkChangeCommand() {

    //     }
    // }

    public class TextureChunkSetCommand : ICommand {
        private TextureLayer2D target;
        private Vector2I chunkPosition;
        private TextureChunk2D chunkBefore, chunkAfter;

        public TextureChunkSetCommand(TextureLayer2D target, Vector2I chunkPosition, TextureChunk2D chunkBefore, TextureChunk2D chunkAfter) {
            this.target = target;
            this.chunkPosition = chunkPosition;
            this.chunkBefore = chunkBefore;
            this.chunkAfter = chunkAfter;
        }

        public void Undo() {
            if (chunkBefore == null) {
                target.Chunks.Remove(chunkPosition);
            } else {
                target.Chunks[chunkPosition] = chunkBefore;
            }
        }

        public void Redo() {
            if (chunkAfter == null) {
                target.Chunks.Remove(chunkPosition);
            } else {
                target.Chunks[chunkPosition] = chunkAfter;
            }
        }
    }

    public class TileSetCommand : ICommand {
        private TileChunk2D target;
        private Vector2I position;
        private Tile2D tileBefore, tileAfter;

        public TileSetCommand(TileChunk2D target, Vector2I position, Tile2D tileBefore, Tile2D tileAfter) {
            this.target = target;
            this.position = position;
            this.tileBefore = tileBefore;
            this.tileAfter = tileAfter;
        }

        public void Undo() => target.SetTile(position, tileBefore);
        public void Redo() => target.SetTile(position, tileAfter);
    }

    public class TileChunkSetCommand : ICommand {
        private TileLayer2D target;
        private Vector2I chunkPosition;
        private TileChunk2D chunkBefore, chunkAfter;

        public TileChunkSetCommand(TileLayer2D target, Vector2I chunkPosition, TileChunk2D chunkBefore, TileChunk2D chunkAfter) {
            this.target = target;
            this.chunkPosition = chunkPosition;
            this.chunkBefore = chunkBefore;
            this.chunkAfter = chunkAfter;
        }

        public void Undo() {
            if (chunkBefore == null) {
                target.Chunks.Remove(chunkPosition);
            } else {
                target.Chunks[chunkPosition] = chunkBefore;
            }
        }

        public void Redo() {
            if (chunkAfter == null) {
                target.Chunks.Remove(chunkPosition);
            } else {
                target.Chunks[chunkPosition] = chunkAfter;
            }
        }
    }
}