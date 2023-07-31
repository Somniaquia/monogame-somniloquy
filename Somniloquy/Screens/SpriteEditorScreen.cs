namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;

    public class SpriteEditorScreen : Screen {
        public EditorScreen EditorScreen { get; set; }
        public List<Tile> Tiles { get; set; }

        public int ScreenTileWidth { get; set; }
        public int PixelSize { get; set; } = 2;

        private Point previousMouseFramePosition;
        private Point mouseFramePosition;

        public SpriteEditorScreen(Rectangle boundaries, EditorScreen editorScreen) : base(boundaries) {
            EditorScreen = editorScreen;
            Tiles = EditorScreen.LoadedWorld.Tiles;

            ScreenTileWidth = boundaries.Width / Layer.TileLength / PixelSize;
        }

        public void CreateTilesFromRawSpriteSheet(Texture2D rawSpriteSheet, World targetWorld) {
            int sheetWidth = rawSpriteSheet.Width / Layer.TileLength;
            int sheetHeight = rawSpriteSheet.Height / Layer.TileLength;

            for (int y = 0; y < sheetHeight; y++) {
                for (int x = 0; x < sheetWidth; x++) {
                    var margin = new Rectangle(x * Layer.TileLength, y * Layer.TileLength, Layer.TileLength, Layer.TileLength);
                    Color[] retrievedColors = new Color[margin.Width * margin.Height];
                    rawSpriteSheet.GetData(0, margin, retrievedColors, 0, retrievedColors.Length);

                    var colors = Utils.ToNullableColors(Utils.ConvertTo2D(retrievedColors, Layer.TileLength));

                    int frame = targetWorld.SpriteSheet.NewFrame();
                    targetWorld.SpriteSheet.PaintOnFrame(colors, frame);
                    Console.WriteLine(frame);

                    targetWorld.NewTile(false);
                }
            }
        }

        public override void OnFocus() {
            Point mousePosition = Utils.ToPoint(InputManager.GetMousePosition()) - Boundaries.Location;

            previousMouseFramePosition = mouseFramePosition;
            mouseFramePosition = new Point(mousePosition.X / Layer.TileLength / PixelSize, mousePosition.Y / Layer.TileLength / PixelSize);

            if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                if (InputManager.IsLeftButtonDown()) {
                    if (Tiles.Count > mouseFramePosition.Y * ScreenTileWidth + mouseFramePosition.X) {
                        EditorScreen.WorldScreen.TilePattern = new Tile[1, 1];
                        EditorScreen.WorldScreen.TilePattern[0, 0] = Tiles[mouseFramePosition.Y * ScreenTileWidth + mouseFramePosition.X];
                    }
                }
            }
            
            base.OnFocus();
        }

        public override void Draw() {
            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            for (int i = 0; i < Tiles.Count; i++) {
                Tiles[i]?.Draw(new Rectangle(
                    Boundaries.X + Layer.TileLength * PixelSize * (i % ScreenTileWidth), 
                    Layer.TileLength * PixelSize * (i / ScreenTileWidth),
                    Layer.TileLength * PixelSize, Layer.TileLength * PixelSize));
            }
            
            GameManager.SpriteBatch.End();
            
            base.Draw();
        }
    }
}