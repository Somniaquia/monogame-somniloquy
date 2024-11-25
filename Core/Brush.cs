namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public abstract class Brush {
        public CommandChain CurrentCommandChain;
        public static List<Brush> BrushTypes = new();
        static Brush() {
            var brushTypes = Assembly.GetAssembly(typeof(Brush)).GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Brush)) && !t.IsAbstract);

            foreach (var type in brushTypes) {
                // Create an instance of each brush type and add it to BrushTypes
                Brush brushInstance = (Brush)Activator.CreateInstance(type);
                BrushTypes.Add(brushInstance);
            }
        }
        public abstract void Paint(IPaintableLayer2D layer, bool initializingPress, Color color, Camera2D camera);
    }

    public class OilPaintBrush : Brush {
        public override void Paint(IPaintableLayer2D paintableLayer, bool initializingPress, Color color, Camera2D camera) {
            int penWidth = (int)(InputManager.GetPenPressure() * 16 / camera.ZoomInverse);
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
            int penWidth = 0;
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

    public class PatternBrush : Brush {
        public override void Paint(IPaintableLayer2D paintableLayer, bool initializingPress, Color color, Camera2D camera) {
            int penWidth = (int)(InputManager.GetPenPressure() * 16 / camera.ZoomInverse);
            float penOpacity = InputManager.GetPenPressure() != 0 ? InputManager.GetPenPressure() : 1;
            penOpacity *= 64 / camera.ZoomInverse;
            penOpacity = Util.Max(1, penOpacity);

            if (initializingPress) {   
                if (CurrentCommandChain is not null) CurrentCommandChain.AffectedPixels = null;
                CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
                // (int)(InputManager.GetPenTilt().Length() * 5)
                paintableLayer.PaintCircle((Vector2I)camera.GlobalMousePos.Value, penWidth, color, penOpacity, true, CurrentCommandChain);
            } else {
                paintableLayer.PaintLine((Vector2I)camera.PreviousGlobalMousePos.Value, (Vector2I)camera.GlobalMousePos.Value, color, penOpacity, penWidth, CurrentCommandChain);
            }
            CurrentCommandChain.AffectedPixels.Clear();
        }
    }
}