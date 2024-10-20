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

        public void AddCommand(ICommand command) {
            commands.Add(command);
        }

        public void Undo() { for (int i = commands.Count - 1; i >= 0; i--) commands[i].Undo(); }
        public void Redo() => commands.ForEach(command => command.Redo());
    }

    public class TextureEditCommand : ICommand {
        private SQTexture2D target;
        private Vector2I position;
        private Color colorBefore, colorAfter;

        public TextureEditCommand(SQTexture2D target, Vector2I position, Color colorBefore, Color colorAfter) {
            this.target = target;
            this.position = position;
            this.colorBefore = colorBefore;
            this.colorAfter = colorAfter;
        }

        public void Undo() => target.SetPixel(position, colorBefore);
        public void Redo() => target.SetPixel(position, colorAfter);
    }

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
        private TileLayer2D target;
        private Vector2I position;
        private Tile2D tileBefore, tileAfter;

        public TileSetCommand(TileLayer2D target, Vector2I position, Tile2D tileBefore, Tile2D tileAfter) {
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
        private Tile2D[,] chunkBefore, chunkAfter;

        public TileChunkSetCommand(TileLayer2D target, Vector2I chunkPosition, Tile2D[,] chunkBefore, Tile2D[,] chunkAfter) {
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