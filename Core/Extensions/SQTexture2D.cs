namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.IO;
    using System.IO.Compression;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using SixLabors.ImageSharp.PixelFormats;

    public class SQTexture2D : Texture2D, ISpriteSheet {
        public static HashSet<SQTexture2D> ChangedTextures = new();
        
        public Color[] TextureData;

        public SQTexture2D(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height) {
            TextureData = new Color[width * height];
            Array.Fill(TextureData, Color.Transparent);
            SetData(TextureData);
        }

        public void SetPixel(Vector2I position, Color color, CommandChain chain = null) {
            chain?.AddCommand(new PixelChangeCommand(this, position, TextureData[position.Unwrap(Width)], color));
            TextureData[position.Unwrap(Width)] = color;
            ChangedTextures.Add(this);
        }

        public void PaintPixel(Vector2I position, Color color, float opacity = 1f, CommandChain chain = null) {
            // opacity = color.A * opacity;
            if (opacity == 1f) {
                SetPixel(position, color, chain);
            } else {
                var blendedColor = BlendColor(position, color, opacity);
                SetPixel(position, blendedColor, chain);
            }
        }

    public Rectangle GetNonTransparentBounds() {
        int width = Width; int height = Height;
        int xMin = width; int yMin = height; int xMax = 0; int yMax = 0;
        bool hasNonTransparentPixels = false;

        for (int i = 0; i < TextureData.Length; i++) {
            if (TextureData[i].A > 0) {
                int x = i % width;
                int y = i / width;

                if (x < xMin) xMin = x;
                if (x > xMax) xMax = x;
                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;

                hasNonTransparentPixels = true;
            }
        }

        if (!hasNonTransparentPixels) return Rectangle.Empty;
        return new Rectangle(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1);
    }

        public Color GetColor(Vector2I position) {
            return TextureData[position.Unwrap(Width)];
        }

        private Color BlendColor(Vector2I position, Color paintingColor, float opacity) {
            Color canvasColor = TextureData[position.Unwrap(Width)];
            return canvasColor.BlendWith(paintingColor, opacity);
        }

        public void Draw(Camera2D camera, Rectangle destination, Rectangle source, Color color, SpriteEffects effects = SpriteEffects.None) {
            camera.Draw(this, destination, source, color);
        }

        public static void ApplyTextureChanges() {
            foreach (var texture in ChangedTextures) {
                texture.SetData(texture.TextureData);
            }
            ChangedTextures.Clear();
        }

        public string Serialize() {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new SQTexture2DConverter());
            return JsonSerializer.Serialize(this, options);
        }

        public static SQTexture2D Deserialize(string json) {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new SQTexture2DConverter());
            return JsonSerializer.Deserialize<SQTexture2D>(json, options);
        }
    }

    public class SQTexture2DConverter : JsonConverter<SQTexture2D> {
        public override SQTexture2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            reader.Read();

            int width = 0;
            int height = 0;
            byte[] compressedTextureDataBytes = null;

            while (reader.TokenType == JsonTokenType.PropertyName) {
                string propertyName = reader.GetString();
                reader.Read();

                if (propertyName == "Width") {
                    width = reader.GetInt32();
                } else if (propertyName == "Height") {
                    height = reader.GetInt32();
                } else if (propertyName == "TextureData") {
                    compressedTextureDataBytes = reader.GetBytesFromBase64();
                }
                reader.Read();
            }

            if (compressedTextureDataBytes == null)
                throw new JsonException("Missing TextureData in JSON.");

            byte[] textureDataBytes = DecompressPng(compressedTextureDataBytes, out int decodedWidth, out int decodedHeight);

            var texture = new SQTexture2D(SQ.GD, decodedWidth, decodedHeight);
            texture.SetData(textureDataBytes);
            texture.GetData(texture.TextureData);
            return texture;
        }

        public override void Write(Utf8JsonWriter writer, SQTexture2D value, JsonSerializerOptions options) {
            writer.WriteStartObject();

            writer.WriteNumber("Width", value.Width);
            writer.WriteNumber("Height", value.Height);

            byte[] rawData = new byte[value.Width * value.Height * 4];
            value.GetData(rawData);

            byte[] compressedTextureDataBytes = CompressToPng(rawData, value.Width, value.Height);

            writer.WriteBase64String("TextureData", compressedTextureDataBytes);
            writer.WriteEndObject();
        }

        private byte[] DecompressPng(byte[] compressedData, out int width, out int height) {
            using var ms = new MemoryStream(compressedData);
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(ms);
            width = image.Width;
            height = image.Height;

            byte[] rawData = new byte[width * height * 4];
            image.CopyPixelDataTo(rawData);
            return rawData;
        }

        private byte[] CompressToPng(byte[] rawData, int width, int height) {
            using var ms = new MemoryStream();
            using var image = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(rawData, width, height);
            SixLabors.ImageSharp.ImageExtensions.SaveAsPng(image, ms);
            return ms.ToArray();
        }
    }

}