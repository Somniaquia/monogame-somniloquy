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
            CommandManager.UndoHistory.Push(command);
            CommandManager.RedoHistory.Clear();
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
    }

    public interface ICommand {
        public abstract void Redo();
        public abstract void Undo();
    }

    public class PaintCommand : ICommand {
        private int animationFrame;
        private List<(Tile, Texture2D, Texture2D)> affectedTiles = new();

        public PaintCommand(int animationFrame) {
            this.animationFrame = animationFrame;
        }

        public void Append(Tile tile, Texture2D previousTexture, Texture2D subsequentTexture) {
            affectedTiles.Add((tile, previousTexture, subsequentTexture));
        }

        public void Redo() {
            foreach (var pair in affectedTiles) {
                pair.Item1.FSprite.CurrentAnimation.PaintOnFrame(pair.Item3, animationFrame, true);
            }
        }

        public void Undo() {
            for (int i = affectedTiles.Count - 1; i >= 0; i--) {
                affectedTiles[i].Item1.FSprite.CurrentAnimation.PaintOnFrame(affectedTiles[i].Item2, animationFrame, false);
            }
        }
    }

    public class SetCommand : ICommand {
        private Layer layer;
        private List<(Point, Tile, Tile)> affectedPositions = new();

        public SetCommand(Layer layer) {
            this.layer = layer;
        }

        public void Append(Point point, Tile previousTile, Tile subsequentTile) {
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
    }
}