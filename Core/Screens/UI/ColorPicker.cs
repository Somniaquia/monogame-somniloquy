namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    

    public class ColorPicker : Screen {
        public Section2DEditor Screen { get; set; }
        
        public Texture2D Chart { get; set; }
        public int Hue { get; set; } = 0;
        public Vector2 PositionOnChart { get; set; } = Vector2.Zero;
        private bool updatedChart = false;

        public ColorPicker(Rectangle boundaries, Section2DEditor screen) : base(boundaries) {
            Screen = screen;
        }

        public static Color ColorFromHSV(float hue, float saturation, float value) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            hue = hue / 60 - (float)Math.Floor(hue / 60);

            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - hue * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - hue) * saturation));

            if (hi == 0)
                return new Color(v, t, p);
            else if (hi == 1)
                return new Color(q, v, p);
            else if (hi == 2)
                return new Color(p, v, t);
            else if (hi == 3)
                return new Color(p, q, v);
            else if (hi == 4)
                return new Color(t, p, v);
            else
                return new Color(v, p, q);
        }

        public static (float hue, float saturation, float value) HSVFromColor(Color color) {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float hue = 0f;
            float saturation = 0f;
            float value = max;

            if (max > 0f) {
                saturation = delta / max;

                if (r >= max) {
                    hue = (g - b) / delta;
                } else if (g >= max) {
                    hue = 2f + (b - r) / delta;
                } else {
                    hue = 4f + (r - g) / delta;
                }

                hue *= 60f;
                if (hue < 0f) {
                    hue += 360f;
                }
            }

            return (hue, saturation, value);
        }

        public void UpdateChart() {
            PositionOnChart = new Vector2(MathF.Max(MathF.Min(1, PositionOnChart.X), 0), MathF.Max(MathF.Min(1, PositionOnChart.Y), 0));
            Hue = Util.PosMod(Hue, 360);

            Chart ??= new Texture2D(SQ.GD, Boundaries.Width, Boundaries.Height);

            Color[] chartData = new Color[Boundaries.Width * Boundaries.Height];

            for (int y = 0; y < Boundaries.Height; y++) {
                for (int x = 0; x < Boundaries.Width; x++) {
                    chartData[y * Boundaries.Width + x] = FetchColor(new Vector2((float)x/Boundaries.Width, (float)y/Boundaries.Height));
                }
            }

            Chart.SetData(chartData);
            Screen.SelectedColor = FetchColor(PositionOnChart);
        }

        public override void OnFocus() {
            if (InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                PositionOnChart = Vector2.Transform(InputManager.GetMousePosition(), TransformMatrix);
                Screen.SelectedColor = FetchColor(PositionOnChart);
                UpdateChart();
            }

            base.OnFocus();
        }

        private Color FetchColor(Vector2 positionOnChart) {
            return ColorFromHSV(Hue, (float)positionOnChart.X, 1f - (float)positionOnChart.Y);
        }

        public void FetchPositionAndHueFromColor(Color desiredColor) {
            var (hue, saturation, value) = HSVFromColor(desiredColor);
            PositionOnChart = new Vector2(saturation, 1 - value);
            Hue = (int) hue;
            UpdateChart();
        }

        public override void Update() {
            base.Update();

            updatedChart = false;
            if (InputManager.IsKeyDown(Keys.U)) { Hue--; updatedChart = true; }
            if (InputManager.IsKeyDown(Keys.O)) { Hue++; updatedChart = true; }

            float updateSpeed = InputManager.IsMouseButtonDown(MouseButtons.LeftButton) ? 0.001f : 0.01f;

            if (InputManager.IsKeyDown(Keys.I)) { PositionOnChart += new Vector2(0, -updateSpeed); updatedChart = true; }
            if (InputManager.IsKeyDown(Keys.K)) { PositionOnChart += new Vector2(0, updateSpeed); updatedChart = true; }
            if (InputManager.IsKeyDown(Keys.J)) { PositionOnChart += new Vector2(-updateSpeed, 0); updatedChart = true; }
            if (InputManager.IsKeyDown(Keys.L)) { PositionOnChart += new Vector2(updateSpeed, 0); updatedChart = true; }

            if (updatedChart) UpdateChart();
        }

        public override void Draw() {
            SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (Screen.EditorState == EditorState.PaintMode) {
                SQ.SB.DrawFilledRectangle(new Rectangle(Boundaries.X - 8, Boundaries.Y - 8, Boundaries.Width + 16, Boundaries.Height + 16), Screen.SelectedColor);
                SQ.SB.Draw(Chart, Boundaries, Color.White);
                SQ.SB.DrawPoint(new Vector2(Boundaries.X, Boundaries.Y) + new Vector2(PositionOnChart.X * Boundaries.Width, PositionOnChart.Y * Boundaries.Height), Util.InvertColor(Screen.SelectedColor), 8);
            }
            base.Draw();
            SQ.SB.End();
        }
    }
}