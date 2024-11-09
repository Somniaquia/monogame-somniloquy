namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public abstract class Brush {
        public CommandChain CurrentCommandChain;
        public abstract void Paint(IPaintableLayer2D layer, bool initializingPress, Color color, Camera2D camera);
    }

    public class DrawingBrush : Brush {
        public override void Paint(IPaintableLayer2D paintableLayer, bool initializingPress, Color color, Camera2D camera) {
            int penWidth = (int)(InputManager.GetPenPressure() * 16 / camera.Zoom);
            float penOpacity = InputManager.GetPenPressure() != 0 ? InputManager.GetPenPressure() : 1;

            if (initializingPress) {   
                if (CurrentCommandChain is not null) CurrentCommandChain.AffectedPixels = null;
                CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
                // (int)(InputManager.GetPenTilt().Length() * 5)
                paintableLayer.PaintCircle((Vector2I)camera.GlobalMousePos.Value, penWidth, color, penOpacity, true, CurrentCommandChain);
            } else {
                paintableLayer.PaintLine((Vector2I)camera.PreviousGlobalMousePos.Value, (Vector2I)camera.GlobalMousePos.Value, color, penOpacity, penWidth, CurrentCommandChain);
            }
        }
    }

    public class PixelArtBrush : Brush {
        public override void Paint(IPaintableLayer2D paintableLayer, bool initializingPress, Color color, Camera2D camera) {
            int penWidth = 1;
            float penOpacity = InputManager.GetPenPressure() != 0 ? InputManager.GetPenPressure() : 1;

            if (initializingPress) {
                if (CurrentCommandChain is not null) CurrentCommandChain.AffectedPixels = null;
                CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
                paintableLayer.PaintCircle((Vector2I)camera.GlobalMousePos.Value, penWidth, color, penOpacity, true, CurrentCommandChain);
            } else {
                paintableLayer.PaintLine((Vector2I)camera.PreviousGlobalMousePos.Value, (Vector2I)camera.GlobalMousePos.Value, color, penOpacity, penWidth, CurrentCommandChain);
            }
        }
    }

    public class CosmicGummyWormBrush : Brush {
        public override void Paint(IPaintableLayer2D paintableLayer, bool initializingPress, Color color, Camera2D camera) {
            int penWidth = (int)(InputManager.GetPenPressure() * 16 / camera.Zoom);
            float penOpacity = 3 * InputManager.GetPenPressure();
            // opacity greater than 1 for cosmic worms

            if (initializingPress) {   
                if (CurrentCommandChain is not null) CurrentCommandChain.AffectedPixels = null;
                CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
                // (int)(InputManager.GetPenTilt().Length() * 5)
                paintableLayer.PaintCircle((Vector2I)camera.GlobalMousePos.Value, penWidth, color, penOpacity, true, CurrentCommandChain);
            } else {
                paintableLayer.PaintLine((Vector2I)camera.PreviousGlobalMousePos.Value, (Vector2I)camera.GlobalMousePos.Value, color, penOpacity, penWidth, CurrentCommandChain);
            }
        }
    }
}