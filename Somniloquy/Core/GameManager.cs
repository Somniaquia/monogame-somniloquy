namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Content;
    
    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;

    public static class GameManager
    {
        public static GraphicsDeviceManager GraphicsDeviceManager { get; set; }
        public static ContentManager ContentManager { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static GameTime GameTime { get; set; }

        public static Rectangle WindowSize { get; set; }
        public static Dictionary<Type, string> Directories { get; private set; } = new();
        public static Dictionary<string, Texture2D> SpriteSheets { get; set; }
        public static Texture2D Pixel { get; set; }
        public static FunctionalSprite MonotextureSprite { get; set; }
        public static BitmapFont Misaki { get; set; }

        public static void InitializeDirectories(params (Type, string)[] directories) {
            string baseDirectory = Directory.GetCurrentDirectory();

            foreach (var entry in directories) {
                Directories.Add(entry.Item1, $"{baseDirectory}/{entry.Item2}");
            }
        }

        public static void WriteTextToFile(Type type, string fileName, string serialized) {
            string directory = $"{Directories[type]}/{fileName}";

            using (FileStream compressedFileStream = File.Create(directory)) {
                using (GZipStream gzipStream = new GZipStream(compressedFileStream, CompressionMode.Compress)) {
                    using (StreamWriter writer = new StreamWriter(gzipStream)) {
                        writer.Write(serialized);
                    }
                }
            }
        }

        public static string ReadTextFromFile(Type type, string fileName) {
            string directory = $"{Directories[type]}/{fileName}";

            try {
                using (FileStream compressedFileStream = File.OpenRead(directory)) {
                    using (GZipStream gzipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress)) {
                        using (StreamReader reader = new StreamReader(gzipStream)) {
                            return reader.ReadToEnd();
                        }
                    }
                }
            } catch (Exception e) {
                System.Console.Out.WriteLine($"Failed to read file: {directory} \n {e.Message}");
                return "null";
            }
        }

        public static void SaveImage(Texture2D texture, string filePath) {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create)) {
                texture.SaveAsPng(fileStream, texture.Width, texture.Height);
            }
        }

        public static Texture2D LoadImage(string filePath) {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open)) {
                return Texture2D.FromStream(GraphicsDeviceManager.GraphicsDevice, fileStream);
            }
        }

        public static void DrawFunctionalSprite(FunctionalSprite fSprite, Rectangle destination, Effect spriteEffect) {
            SpriteBatch.Draw(
                fSprite.CurrentAnimation.SpriteSheet,
                destination,
                fSprite.CurrentAnimation.FrameBoundaries[fSprite.FrameInCurrentAnimation],
                Color.White);
        }

        public static void DrawFilledRectangle(Rectangle destination, Color color) {
            SpriteBatch.Draw(Pixel, destination, null, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth:0);
        }

        public static void DrawPixelizedLine(Point position1, Point position2, Color color) {
            int dx = Math.Abs(position2.X - position1.X);
            int dy = Math.Abs(position2.Y - position1.Y);
            int sx = (position1.X < position2.X) ? 1 : -1;
            int sy = (position1.Y < position2.Y) ? 1 : -1;
            int err = dx - dy;

            while (true) {
                SpriteBatch.DrawPoint(new Vector2(position1.X, position1.Y), color);

                if (position1.X == position2.X && position1.Y == position2.Y)
                    break;

                int err2 = 2 * err;

                if (err2 > -dy)
                {
                    err -= dy;
                    position1.X += sx;
                }

                if (err2 < dx)
                {
                    err += dx;
                    position1.Y += sy;
                }
            }
        }
    }
}