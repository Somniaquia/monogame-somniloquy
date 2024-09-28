namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    using MonoGame.Extended;


    public partial class TextureLayer2D : Layer2D {
        public int ChunkLength = 2560;
        public Dictionary<Point, TextureChunk2D> Chunks = new();
        public HashSet<TextureChunk2D> UpdatedChunks = new();

        public Point GetChunkPosition(Point canvasPosition) {
            return Util.Floor(canvasPosition.ToVector2() / ChunkLength);
        }

        public Point GetPositionInChunk(Point canvasPosition) {
            return Util.PosMod(canvasPosition, new Point(ChunkLength, ChunkLength));
        }

        public TextureLayer2D() {
            ParentSection.DisplayedScreen.Editor.SelectedLayer = this;
        }

        public void PaintRectangle(Point startPosition, Point endPosition, Color color, float opacity) {
            for (int y = startPosition.X; y < endPosition.Y; y++) {
                for (int x = 0; x < endPosition.X; x++) {
                    PaintPixel(new Point(x, y), color, opacity);
                }
            }
        }

        public void PaintLine(Point start, Point end, Color color, float opacity, int width = 0) {
            Actions.LineAction(start, end, color, opacity, PaintPixel, width);
        }

        public void PaintCircle(Point center, int radius, Color color, float opacity, bool filled = true) {
            if (filled) {
                Actions.FilledCircleAction(center, radius, color, opacity, PaintPixel);
            } else {
                Actions.CircleAction(center, radius, color, opacity, PaintPixel);
            }
        }

        public void PaintPixel(Point canvasPosition, Color color, float opacity) {
            var (chunkPosition, positionInChunk) = (GetChunkPosition(canvasPosition), GetPositionInChunk(canvasPosition));
            if (!Chunks.ContainsKey(chunkPosition)) {
                Chunks.Add(chunkPosition, new TextureChunk2D(this, ChunkLength));
            }
            Chunks[chunkPosition].PaintPixel(positionInChunk, color, opacity);
        }

        public void UpdateCanvas() {
            // Called at the end of the frame to prevent unnecessary repetitive Image <-> ImageTexture conversion.
            foreach (var chunk in UpdatedChunks) {
                chunk.UpdateImageTexture();
            }

            UpdatedChunks.Clear();
        }

        public void Update() {

        }

        public void Draw() {
            var camera = ParentSection.DisplayedScreen.Camera;
            var viewportRect = camera.GetViewport();
            
            Vector2 cameraWorldPosition = camera.Position;// - camera.Offset;
            Vector2 halfViewportSize = viewportRect.Size / 2;

            Vector2 topLeft = cameraWorldPosition - halfViewportSize * camera.Zoom;
            Vector2 bottomRight = cameraWorldPosition + halfViewportSize * camera.Zoom;
            Point topLeftChunk = new((int)(topLeft.X / ChunkLength) - 1, (int)(topLeft.Y / ChunkLength) - 1);
            Point bottomRightChunk = new((int)(bottomRight.X / ChunkLength) + 1, (int)(bottomRight.Y / ChunkLength) + 1);


            for (int y = topLeftChunk.Y; y < bottomRightChunk.Y; y++) {
                for (int x = topLeftChunk.X; x < bottomRightChunk.X; x++) {
                    var chunkPosition = new Point(x, y);
                    if (!Chunks.ContainsKey(chunkPosition)) continue;
                    var xLeft = MathF.Min(MathF.Max(topLeft.X, x * ChunkLength), bottomRight.X);
                    var xRight = MathF.Max(MathF.Min(bottomRight.X, (x + 1) * ChunkLength), topLeft.X);
                    var yTop = MathF.Min(MathF.Max(topLeft.Y, y * ChunkLength), bottomRight.Y);
                    var yBottom = MathF.Max(MathF.Min(bottomRight.Y, (y + 1) * ChunkLength), topLeft.Y); 

                    SQ.SB.Draw(Chunks[chunkPosition].Texture, new RectangleF(xLeft, yTop, xRight - xLeft, yBottom - yTop).ToRectangle(), new RectangleF(xLeft - x * ChunkLength, yTop - y * ChunkLength, xRight - xLeft, yBottom - yTop).ToRectangle(), Color.White);
                }
            }
        }
    }

    public class TextureChunk2D {
        public TextureLayer2D ParentLayer;
        public Color[] TextureData;
        public Texture2D Texture;
        public int ChunkLength;
        
        public TextureChunk2D (TextureLayer2D parentLayer, int chunkLength) {
            ParentLayer = parentLayer;
            ChunkLength = chunkLength;

            TextureData = new Color[chunkLength * chunkLength];
            Array.Fill(TextureData, Color.Transparent);
            Texture = new Texture2D(SQ.GD, chunkLength, chunkLength);
            Texture.SetData(TextureData);
        }

        public void PaintPixel(Point positionInChunk, Color color, float opacity) {
            var blendedColor = BlendColor(positionInChunk, color, opacity);
            TextureData[positionInChunk.Unwrap(ChunkLength)] = blendedColor;
            ParentLayer.UpdatedChunks.Add(this);
        }

        // public void PaintTexture(Point centerPositionInChunk, Color[] texture, float opacity) {
        //     Point textureSize = new(texture.GetWidth(), texture.GetHeight());
            
        //     for (int y = 0; y < textureSize.Y; y++) {
        //         for (int x = 0; x < textureSize.X; x++) {
        //             Point positionInChunk = centerPositionInChunk + new Point(x, y);

        //             if (positionInChunk.X < 0 || positionInChunk.X >= Image.GetWidth() ||
        //                 positionInChunk.Y < 0 || positionInChunk.Y >= Image.GetHeight()) {
        //                 continue;
        //             }

        //             var blendedColor = BlendColor(positionInChunk, texture.GetPixel(x, y), opacity);
        //             Image.SetPixelv(positionInChunk, blendedColor);
        //         }
        //     }

        //     ParentLayer.UpdatedChunks.Add(this);
        // }

        private Color BlendColor(Point positionInChunk, Color paintingColor, float opacity) {
            if (opacity == 1f) return paintingColor;

            Color canvasColor = TextureData[positionInChunk.Unwrap(ChunkLength)];
            paintingColor.A = (byte)(255 * opacity);
            
            Color blendedColor = new(
                paintingColor.R * paintingColor.A + canvasColor.R * (1 - paintingColor.A),
                paintingColor.G * paintingColor.A + canvasColor.G * (1 - paintingColor.A),
                paintingColor.B * paintingColor.A + canvasColor.B * (1 - paintingColor.A),
                paintingColor.A + canvasColor.A * (1 - paintingColor.A)
            );

            return blendedColor;
        }


        public void UpdateImageTexture() {
            // Called once at the end of the frame if this chunk has been modified
            Texture.SetData(TextureData);
        }
    }
}