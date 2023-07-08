namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public static class CommandManager {
        public static Stack<ICommand> UndoHistory = new();
        public static Stack<ICommand> RedoHistory = new();

        public static void Push(ICommand command) {
            UndoHistory.Push(command);
            RedoHistory.Clear();
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
            foreach (var command in UndoHistory) {
                command.Clear();
            } foreach (var command in RedoHistory) {
                command.Clear();
            }

            UndoHistory.Clear();
            RedoHistory.Clear();
        }
    }

    public interface ICommand {
        public abstract void Redo();
        public abstract void Undo();
        public abstract void Clear();
    }

    public class PaintCommand : ICommand {
        private int animationFrame;
        private List<(Tile, Color?[,], Color?[,])> affectedTiles = new();

        public PaintCommand(int animationFrame) {
            this.animationFrame = animationFrame;
        }

        public void Append(Tile tile, Color?[,] previousColors, Color?[,] subsequentColors) {
            for (int i = 0; i < affectedTiles.Count; i++) {
                if (ReferenceEquals(affectedTiles[i].Item1, tile)) {
                    affectedTiles[i] = (tile, affectedTiles[i].Item2, subsequentColors);
                    return;
                }
            }

            affectedTiles.Add((tile, previousColors, subsequentColors));
        }

        public void Redo() {
            foreach (var pair in affectedTiles) {
                pair.Item1.FSprite.GetCurrentAnimation().PaintOnFrame(pair.Item3, animationFrame);
            }
        }

        public void Undo() {
            for (int i = affectedTiles.Count - 1; i >= 0; i--) {
                affectedTiles[i].Item1.FSprite.GetCurrentAnimation().PaintOnFrame(affectedTiles[i].Item2, animationFrame);
            }
        }

        public void Clear() {
            affectedTiles.Clear();
        }
    }

    public class SetCommand : ICommand {
        private Layer layer;
        private List<(Point, Tile, Tile)> affectedPositions = new();

        public SetCommand(Layer layer) {
            this.layer = layer;
        }

        public void Append(Point point, Tile previousTile, Tile subsequentTile) {
            for (int i = 0; i < affectedPositions.Count; i++) {
                if (affectedPositions[i].Item1.Equals(point)) {
                    affectedPositions[i] = (point, affectedPositions[i].Item2, subsequentTile);
                    return;
                }
            }

            affectedPositions.Add((point, previousTile, subsequentTile));
        }

        public void Redo() {
            foreach (var pair in affectedPositions) {
                layer.SetTile(pair.Item1, pair.Item3);
            }
        }

        public void Undo() {
            for (int i = affectedPositions.Count - 1; i >= 0; i--) {
                layer.SetTile(affectedPositions[i].Item1, affectedPositions[i].Item2);
            }
        }

        public void Clear() {
            affectedPositions.Clear();
        }
    }
}