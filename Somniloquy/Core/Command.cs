namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public interface ICommand {
        public abstract void Execute();
        public abstract void Undo();
    }

    public class PaintTrajectoryCommand : ICommand {
        public void Execute() {
            throw new NotImplementedException();
        }

        public void Undo() {
            throw new NotImplementedException();
        }
    }

    public class PaintDropCommand : ICommand {
        private List<Tile> affectedTiles;

        public void Execute() {
            
        }

        public void Undo() {
            
        }
    }
}