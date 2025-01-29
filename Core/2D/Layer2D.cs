namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Layer2D {
        [JsonIgnore] public Section2D Section;
        [JsonIgnore] public Layer2D Parent;
        [JsonInclude] public List<Layer2D> Layers;
        [JsonInclude] public string Identifier = "";
        [JsonIgnore] public bool Enabled = true;
        [JsonIgnore] public float Opacity = 1f;
        
        public Layer2D() { }
        public Layer2D(string identifier) { Identifier = identifier; }

        public Layer2D AddLayer(Layer2D layer) {
            Layers ??= new();
            Layers.Add(layer);
            layer.Section = Section;
            layer.Parent = this;
            return layer;
        }

        public Layer2D InsertLayer(int index, Layer2D layer) {
        Layers ??= new();
            Layers.Insert(index, layer);
            layer.Section = Section;
            layer.Parent = this;
            return layer;
        }

        public void ToggleHide() {
            if (Enabled) Hide();
            else Show();
        }

        public void Hide() {
            Enabled = false;
            Layers?.ForEach(layer => layer.Hide());
        }

        public void Show() {
            Enabled = true;
            Layers?.ForEach(layer => layer.Show());
        }

        public bool HasChildren() => Layers is not null && Layers.Count > 0;

        public virtual void Update() {
            if (Layers is null) return;
            foreach (var layer in Layers) {
                layer.Update();
            }
        }

        public virtual void Draw(Camera2D camera) {
            if (Layers is null) return;
            foreach (var layer in Layers) {
                layer.Draw(camera);
            }
        }
    }

    public class PaintableLayer2D : Layer2D {
        public PaintableLayer2D() : base() { }
        public PaintableLayer2D(string identifier) : base(identifier) { }

        public void PaintRectangle(Vector2I startPosition, Vector2I endPosition, Color color, float opacity, bool filled, CommandChain chain = null) {
            PixelActions.ApplyRectangleAction(startPosition, endPosition, filled, (Vector2I position) => {
                PaintPixel(position, color, opacity, chain);
            });
        }

        public void PaintLine(Vector2I start, Vector2I end, Color color, float opacity, int width = 0, CommandChain chain = null) {
            PixelActions.ApplyLineAction(start, end, width, (Vector2I position) => {
                PaintPixel(position, color, opacity, chain);
            });
        }
        
        public void PaintSnappedLine(Vector2I start, Vector2I end, Color color, float opacity, int width = 0, CommandChain chain = null) {
            PixelActions.ApplySnappedLineAction(start, end, width, (Vector2I position) => {
                PaintPixel(position, color, opacity, chain);
            });
        }

        public void PaintCircle(Vector2I center, int radius, Color color, float opacity, bool filled = true, CommandChain chain = null) {
            PixelActions.ApplyCircleAction(center, radius, filled, (Vector2I position) => {
                PaintPixel(position, color, opacity, chain);
            });
        }

        public void Fill(Vector2I startPos, Color color) {
            Color? startColor = GetColor(startPos);

            var stack = new Stack<Vector2I>();
            var lookedPositions = new HashSet<Vector2I>();
            var chain = new CommandChain();

            stack.Push(startPos);
            lookedPositions.Add(startPos);

            while (stack.Count > 0 && lookedPositions.Count < 1000000) {
                var pos = stack.Pop();
                PaintPixel(pos, color, 1f, chain);

                foreach (var checkPos in new Vector2I[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) }) {
                    var newPos = pos + checkPos;
                    if (!lookedPositions.Contains(newPos) && GetColor(newPos) == startColor) {
                        lookedPositions.Add(newPos);
                        stack.Push(newPos);
                    }
                }
            }

            if (lookedPositions.Count >= 1000000) {
                chain.Undo();
                DebugInfo.AddTempLine(() => "Overload: Cancelled fill operation.", 5);
            } else {
                CommandManager.AddCommandChain(chain);
            }
        }
        
        public virtual Color? GetColor(Vector2I position) { return null; }

        public virtual void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain = null) { }

        /// <summary>
        /// TODOO:::: Complete re-write of image export function
        /// </summary>
        public virtual Rectangle GetTextureBounds() {
            List<Rectangle> bounds = (from layer in Layers where layer is TextureLayer2D let textureLayer = (TextureLayer2D)layer select textureLayer.GetTextureBounds()).ToList();

            int xMin = bounds.Min(bound => bound.Left);
            int xMax = bounds.Max(bound => bound.Right);
            int yMin = bounds.Min(bound => bound.Top);
            int yMax = bounds.Max(bound => bound.Bottom);

            return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        /// <summary>
        /// TODOO:::: Complete re-write of image export function
        /// </summary>
        public void SaveTexture(string path) {
            Texture2D texture = GetTexture();
            // using var fileStream = new FileStream(path, FileMode.Create);
            // texture.SaveAsPng(fileStream, texture.Width, texture.Height);
            IOManager.SaveTextureDataAsPngAsync(texture, path);
        }

        /// <summary>
        /// TODOO:::: Complete re-write of image export function
        /// </summary>
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
    }

    public class Layer2DConverter : JsonConverter<Layer2D> {
        public override Layer2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader)) {
                var root = document.RootElement;

                if (!root.TryGetProperty("Type", out var typeProp))
                    throw new JsonException("Missing type discriminator in JSON.");

                string type = typeProp.GetString();

                return type switch {
                    "TextureLayer2D" => JsonSerializer.Deserialize<TextureLayer2D>(root.GetRawText(), options),
                    "TileLayer2D" => JsonSerializer.Deserialize<TileLayer2D>(root.GetRawText(), options),
                    _ => throw new JsonException($"Unsupported type: {type}")
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, Layer2D value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            writer.WriteString("Type", value.GetType().Name);
            foreach (var property in JsonSerializer.SerializeToElement(value, value.GetType(), options).EnumerateObject()) {
                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }
    }
}