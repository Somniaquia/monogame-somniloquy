namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public static class CommandManager {
        public static Stack<ICommand> UndoHistory;
        public static Stack<ICommand> RedoHistory;

        public static void Undo() {
            ICommand command = UndoHistory.Pop();
            command.Undo();
            RedoHistory.Push(command);
        }

        public static void Redo() {
            ICommand command = RedoHistory.Pop();
            command.Execute();
            UndoHistory.Push(command);
        }
    }

    public interface ICommand {
        public abstract void Execute();
        public abstract void Undo();
    }

    public class PaintCommand : ICommand {
        private List<(Tile, Tile)> affectedTiles;

        public PaintCommand() {
            CommandManager.UndoHistory.Push(this);
            CommandManager.RedoHistory.Clear();
        }

        public void AddAffectedTiles() {
            
        }

        public void Execute() {

        }

        public void Undo() {
            foreach (var affectedTile in affectedTiles) {

            }
        }
    }
}