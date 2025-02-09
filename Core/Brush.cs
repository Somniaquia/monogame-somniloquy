namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public static class BrushPicker {
        public static void BuildUI() {

        }

        public static void DestroyUI() {
            
        }
    }

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

        public void Blur(PaintableLayer2D layer, int radius, int kernelRadius, Camera2D camera) {
            if (radius == 0 || kernelRadius == 0) return;
            float[,] kernel = Util.GetGaussianKernel(kernelRadius);
            Vector2I mousePos = (Vector2I)layer.ToLayerPos(camera.GlobalMousePos.Value);

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
                    
                    // Debug.Assert(color[0] <= 256);
                    // Debug.Assert(color[1] <= 256);
                    // Debug.Assert(color[2] <= 256);
                    // Debug.Assert(color[3] <= 256);
                    
                    newColors[x + radius, y + radius] = new Color((byte)color[0], (byte)color[1], (byte)color[2], (byte)color[3]);
                }
            }

            for (int x = -radius; x <= radius; x++) {
                for (int y = -radius; y <= radius; y++) {
                    layer.PaintPixel(mousePos + new Vector2I(x, y), newColors[x + radius, y + radius], 1f, CurrentCommandChain);
                }
            }
        }

        public abstract void Paint(PaintableLayer2D layer, bool initializingPress, Color color, Camera2D camera);
    }

    public class OilPaint : Brush {
        public override void Paint(PaintableLayer2D paintableLayer, bool initializingPress, Color color, Camera2D camera) {
            int penWidth = InputManager.GetPenPressure() != 0 ? (int)(InputManager.GetPenPressure() * 16 / camera.Zoom) : (int)(camera.AverageMouseSpeed * camera.Zoom / 2000);
            float penOpacity = 1;

            if (initializingPress) {
                if (CurrentCommandChain is not null) CurrentCommandChain.AffectedPixels = null;
                CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
                // (int)(InputManager.GetPenTilt().Length() * 5)
                paintableLayer.PaintCircle((Vector2I)paintableLayer.ToLayerPos(camera.GlobalMousePos.Value), penWidth, color, penOpacity, true, CurrentCommandChain);
            } else {
                paintableLayer.PaintLine((Vector2I)paintableLayer.ToLayerPos(camera.PreviousGlobalMousePos.Value), (Vector2I)paintableLayer.ToLayerPos(camera.GlobalMousePos.Value), color, penOpacity, penWidth, CurrentCommandChain);
            }

            // Blur(paintableLayer, penWidth, penWidth, camera);
        }
    }

    public class WaterPaint : Brush {
        public override void Paint(PaintableLayer2D paintableLayer, bool initializingPress, Color color, Camera2D camera) {
            int penWidth = InputManager.GetPenPressure() != 0 ? (int)(InputManager.GetPenPressure() * 32 / camera.Zoom) : (int)(MathF.Min(1, camera.AverageMouseSpeed * 0.0004f) / MathF.Sqrt(camera.Zoom) * 64);
            // float penOpacity = 1;
            float penOpacity = InputManager.GetPenPressure() != 0 ? InputManager.GetPenPressure() / 2 : MathF.Min(1, camera.AverageMouseSpeed * camera.Zoom / 5000);

            if (initializingPress) {   
                if (CurrentCommandChain is not null) CurrentCommandChain.AffectedPixels = null;
                CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
                // (int)(InputManager.GetPenTilt().Length() * 5)
                paintableLayer.PaintCircle((Vector2I)paintableLayer.ToLayerPos(camera.GlobalMousePos.Value), penWidth, color, penOpacity, true, CurrentCommandChain);
            } else {
                paintableLayer.PaintLine((Vector2I)paintableLayer.ToLayerPos(camera.PreviousGlobalMousePos.Value), (Vector2I)paintableLayer.ToLayerPos(camera.GlobalMousePos.Value), color, penOpacity, penWidth, CurrentCommandChain);
            }

            // Blur(paintableLayer, penWidth, penWidth, camera);
        }
    }

    // public class PixelArtBrush : Brush {
    //     public override void Paint(IPaintableLayer2D paintableLayer, bool initializingPress, Color color, Camera2D camera) {
    //         int penWidth = 0;
    //         float penOpacity = 1;

    //         if (initializingPress) {
    //             if (CurrentCommandChain is not null) CurrentCommandChain.AffectedPixels = null;
    //             CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
    //             paintableLayer.PaintCircle((Vector2I)paintableLayer.ToLayerPos(camera.GlobalMousePos.Value), penWidth, color, penOpacity, true, CurrentCommandChain);
    //         } else {
    //             paintableLayer.PaintLine((Vector2I)paintableLayer.ToLayerPos(camera.PreviousGlobalMousePos.Value), (Vector2I)paintableLayer.ToLayerPos(camera.GlobalMousePos.Value), color, penOpacity, penWidth, CurrentCommandChain);
    //         }
    //     }
    // }

    // public class GradientBrush : Brush {
    //     public override void Paint(PaintableLayer2D layer, bool initializingPress, Color color, Camera2D camera) {
            
    //     }
    // }

    public class BlurBrush : Brush {
        public override void Paint(PaintableLayer2D layer, bool initializingPress, Color color, Camera2D camera) {
            if (initializingPress) {   
                if (CurrentCommandChain is not null) CurrentCommandChain.AffectedPixels = null;
                CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
            }

            int radius = (int)(InputManager.GetPenPressure() * 32 / camera.Zoom) + 1;
            int kernelRadius = (int)(InputManager.GetPenPressure() * 32 / camera.Zoom) + 1;
            Blur(layer, radius, kernelRadius, camera);
        }
    }

    // public class PatternBrush : Brush {
    //     public override void Paint(IPaintableLayer2D paintableLayer, bool initializingPress, Color color, Camera2D camera) {
    //         int penWidth = (int)(InputManager.GetPenPressure() * 16 / camera.Zoom);
    //         float penOpacity = InputManager.GetPenPressure() != 0 ? InputManager.GetPenPressure() : 1;
    //         penOpacity *= 64 / camera.Zoom;
    //         penOpacity = Util.Max(1, penOpacity);

    //         if (initializingPress) {   
    //             if (CurrentCommandChain is not null) CurrentCommandChain.AffectedPixels = null;
    //             CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
    //             // (int)(InputManager.GetPenTilt().Length() * 5)
    //             paintableLayer.PaintCircle((Vector2I)paintableLayer.ToLayerPos(camera.GlobalMousePos.Value), penWidth, color, penOpacity, true, CurrentCommandChain);
    //         } else {
    //             paintableLayer.PaintLine((Vector2I)paintableLayer.ToLayerPos(camera.PreviousGlobalMousePos.Value), (Vector2I)paintableLayer.ToLayerPos(camera.GlobalMousePos.Value), color, penOpacity, penWidth, CurrentCommandChain);
    //         }
    //         CurrentCommandChain.AffectedPixels.Clear();
    //     }
    // }
}