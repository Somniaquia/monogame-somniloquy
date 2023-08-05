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
            UndoHistory.Clear();
            RedoHistory.Clear();
        }
    }

    public interface ICommand {
        public abstract void Redo();
        public abstract void Undo();
    }

    public class WorldEditCommand : ICommand {
        private List<(Animation, int, Color?[,], Color?[,])> textureChanges = new(); 
        private List<(Layer, Point, Tile, Tile)> tileReferenceChanges = new();

        public void AppendFrameTextureChanges(Animation animation, int frame, Color?[,] previousColors, Color?[,] subsequentColors) {
            for (int i = 0; i < textureChanges.Count; i++) {
                if (ReferenceEquals(textureChanges[i].Item1, animation) && textureChanges[i].Item2 == frame) {
                    textureChanges[i] = (animation, frame, textureChanges[i].Item3, subsequentColors);
                    return;
                }
            }

            textureChanges.Add((animation, frame, previousColors, subsequentColors));
        }

        public void AppendTileReferenceChanges(Layer layer, Point position, Tile previousTile, Tile subsequentTile) {
            for (int i = 0; i < tileReferenceChanges.Count; i++) {
                if (tileReferenceChanges[i].Item1 == layer && tileReferenceChanges[i].Item2 == position) {
                    tileReferenceChanges[i] = (layer, position, tileReferenceChanges[i].Item3, subsequentTile);
                    return;
                }
            }
            
            tileReferenceChanges.Add((layer, position, previousTile, subsequentTile));
        }

        public void Undo() {
            for (int i = textureChanges.Count - 1; i >= 0; i--) {
                textureChanges[i].Item1.ParentSprite.PaintOnFrame(textureChanges[i].Item3);
            }

            foreach (var pair in tileReferenceChanges) {
                pair.Item1.SetTile(pair.Item2, pair.Item3);
            }
        }

        public void Redo() {
            foreach (var pair in textureChanges) {
                pair.Item1.ParentSprite.PaintOnFrame(pair.Item3);
            }

            foreach (var pair in tileReferenceChanges) {
                pair.Item1.SetTile(pair.Item2, pair.Item4);
            }
        }
    }
}