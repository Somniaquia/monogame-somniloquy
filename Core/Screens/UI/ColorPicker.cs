namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class ColorPicker : Screen {
        public Section2DEditor Screen;
        
        public Texture2D Chart;
        public Vector2 PositionOnChart = Vector2.Zero;
        public int Hue = 0;

        public ColorPicker(Rectangle boundaries, Section2DEditor screen) : base(boundaries) { 
            Screen = screen; 
        }

        public override void LoadContent() {
            Chart = new Texture2D(SQ.GD, Boundaries.Width, Boundaries.Height); 
            CreateChartTexture();
        }

        public void CreateChartTexture() {
            Color[] chartData = new Color[Boundaries.Width * Boundaries.Height];

            for (int y = 0; y < Boundaries.Height; y++) {
                for (int x = 0; x < Boundaries.Width; x++) {
                    chartData[y * Boundaries.Width + x] = FetchColor(new Vector2((float)x/Boundaries.Width, (float)y/Boundaries.Height));
                }
            }
            Chart.SetData(chartData);
        }

        private Color FetchColor(Vector2 positionOnChart) {
            return new ColorOkHSL((byte)Hue, (byte)(positionOnChart.X * 255), (byte)(255 - positionOnChart.Y * 255)).ToRGB();
        }

        public void FetchPositionAndHueFromColor(Color desiredColor) {
            var hsl = desiredColor.ToOkHSL();
            PositionOnChart = new Vector2(hsl.S, 1 - hsl.L);
            Hue = hsl.H;
            CreateChartTexture();
        }

        public override void Update() {
            base.Update();

            bool updateChart = false;
            if (InputManager.IsKeyDown(Keys.U)) { Hue--; updateChart = true; }
            if (InputManager.IsKeyDown(Keys.O)) { Hue++; updateChart = true; }

            float updateSpeed = InputManager.IsMouseButtonDown(MouseButtons.LeftButton) ? 0.001f : 0.01f;

            if (InputManager.IsKeyDown(Keys.I)) { PositionOnChart += new Vector2(0, -updateSpeed); }
            if (InputManager.IsKeyDown(Keys.K)) { PositionOnChart += new Vector2(0, updateSpeed); }
            if (InputManager.IsKeyDown(Keys.J)) { PositionOnChart += new Vector2(-updateSpeed, 0); }
            if (InputManager.IsKeyDown(Keys.L)) { PositionOnChart += new Vector2(updateSpeed, 0); }

            if (updateChart) CreateChartTexture();

            if (ScreenManager.FocusedScreen == this) {
                if (InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                    PositionOnChart = Vector2.Transform(InputManager.GetMousePosition(), Transform);
                    Screen.SelectedColor = FetchColor(PositionOnChart);
                    CreateChartTexture();
                }
            }

            PositionOnChart = new Vector2(MathF.Max(MathF.Min(1, PositionOnChart.X), 0), MathF.Max(MathF.Min(1, PositionOnChart.Y), 0));
            Hue = Util.PosMod(Hue, 255);
            Screen.SelectedColor = FetchColor(PositionOnChart);
        }

        public override void Draw() {
            if (Screen.EditorState == EditorState.PaintMode) {
                SQ.SB.DrawFilledRectangle(new Rectangle(Boundaries.X - 8, Boundaries.Y - 8, Boundaries.Width + 16, Boundaries.Height + 16), Screen.SelectedColor);
                SQ.SB.Draw(Chart, Boundaries, Color.White);
                SQ.SB.DrawCircle((Vector2I)(new Vector2(Boundaries.X, Boundaries.Y) + new Vector2(PositionOnChart.X * Boundaries.Width, PositionOnChart.Y * Boundaries.Height)), 8, Util.InvertColor(Screen.SelectedColor), true);
            }
        }
    }
}