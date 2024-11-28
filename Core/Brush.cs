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

        public void Blur(IPaintableLayer2D layer, int radius, int kernelRadius, Camera2D camera) {
            float[,] kernel = Util.GetGaussianKernel(kernelRadius);
            Vector2I mousePos = (Vector2I)camera.GlobalMousePos.Value;

            Color[,] newColors = new Color[2 * radius + 1, 2 * radius + 1];
            for (int x = -radius; x <= radius; x++) {
                for (int y = -radius; y <= radius; y++) {
                    float[] color = new float[4];
                    Vector2I pixelPos = mousePos + new Vector2I(x, y);

                    for (int i = -kernelRadius; i <= kernelRadius; i++) {
                        for (int j = -kernelRadius; j <= kernelRadius; j++) {
                            float weight = kernel[i + kernelRadius, j + kernelRadius];
                            Color? applyingColor = layer.GetColor(pixelPos + new Vector2I(i, j));
                            applyingColor = applyingColor is null ? new Color(0, 0, 0, 0) : applyingColor.Value;
                            color[0] += applyingColor.Value.R * weight;
                            color[1] += applyingColor.Value.G * weight;
                            color[2] += applyingColor.Value.B * weight;
                            color[3] += applyingColor.Value.A * weight;
                        }
                    }
                    
                    Debug.Assert(color[0] <= 255);
                    Debug.Assert(color[1] <= 255);
                    Debug.Assert(color[2] <= 255);
                    Debug.Assert(color[3] <= 255);
                    
                    newColors[x + radius, y + radius] = new Color((byte)color[0], (byte)color[1], (byte)color[2], (byte)color[3]);
                }
            }

            for (int x = -radius; x <= radius; x++) {
                for (int y = -radius; y <= radius; y++) {
                    layer.PaintPixel(mousePos + new Vector2I(x, y), newColors[x + radius, y + radius], 1f, CurrentCommandChain);
                }
            }
        }

        public abstract void Paint(IPaintableLayer2D layer, bool initializingPress, Color color, Camera2D camera);
    }

    public class OilPaintBrush : Brush {
        public override void Paint(IPaintableLayer2D paintableLayer, bool initializingPress, Color color, Camera2D camera) {
            int penWidth = (int)(InputManager.GetPenPressure() * 16 / camera.Zoom);
            // float penOpacity = 1;
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
            float penOpacity = 1;
            // float penOpacity = InputManager.GetPenPressure() != 0 ? InputManager.GetPenPressure() : 1;

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
            int penWidth = (int)(InputManager.GetPenPressure() * 16 / camera.Zoom);
            float penOpacity = InputManager.GetPenPressure() != 0 ? InputManager.GetPenPressure() : 1;
            penOpacity *= 64 / camera.Zoom;
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

    public class BlurBrush : Brush {
        public override void Paint(IPaintableLayer2D layer, bool initializingPress, Color color, Camera2D camera) {
            if (initializingPress) {   
                if (CurrentCommandChain is not null) CurrentCommandChain.AffectedPixels = null;
                CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
            }

            int radius = (int)(InputManager.GetPenPressure() * 16 / camera.Zoom) + 1;
            int kernelRadius = (int)(InputManager.GetPenPressure() * 16 / camera.Zoom) + 1;
            Blur(layer, radius, kernelRadius, camera);
        }
    }
}