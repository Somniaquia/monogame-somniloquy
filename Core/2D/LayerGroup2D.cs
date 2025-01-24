namespace Somniloquy {
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class LayerGroup2D : Layer2D {        
        [JsonInclude] public List<Layer2D> Layers = new();

        public LayerGroup2D() { }
        public LayerGroup2D(string identifier) { Identifier = identifier; }

        public Layer2D AddLayer(Layer2D layer) {
            Layers.Add(layer);
            layer.Section = Section;
            return layer;
        }

        public Rectangle GetTextureBounds() {
            List<Rectangle> bounds = (from layer in Layers where layer is TextureLayer2D let textureLayer = (TextureLayer2D)layer select textureLayer.GetTextureBounds()).ToList();

            int xMin = bounds.Min(bound => bound.Left);
            int xMax = bounds.Max(bound => bound.Right);
            int yMin = bounds.Min(bound => bound.Top);
            int yMax = bounds.Max(bound => bound.Bottom);

            return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public void SaveTexture(string path) {
            Texture2D texture = GetTexture();
            // using var fileStream = new FileStream(path, FileMode.Create);
            // texture.SaveAsPng(fileStream, texture.Width, texture.Height);
            IOManager.SaveTextureDataAsPngAsync(texture, path);
        }

        public Texture2D GetTexture() {
            var bounds = GetTextureBounds();
            RenderTarget2D target = new(SQ.GD, bounds.Width, bounds.Height);

            SQ.GD.SetRenderTarget(target);
            SQ.GD.Clear(Color.Transparent);
            SQ.SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (var layer in Layers) {
                if (layer is TextureLayer2D textureLayer) {
                    textureLayer.Draw(bounds.TopLeft(), bounds.BottomRight());
                }
            }
            SQ.SB.End();
            SQ.GD.SetRenderTarget(null);
            return target;
        }

        public override void Update() {
            foreach (var layer in Layers) {
                layer.Update();
            }
        }

        public override void Draw(Camera2D camera, float opacity = 1f) {
            foreach (var layer in Layers) {
                layer.Draw(camera, opacity);
            }
        }
    }
}