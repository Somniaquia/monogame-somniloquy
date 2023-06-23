namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using MonoGame.Extended;

    public class ColorChart : Screen {
        public ColorChart(Rectangle boundaries) : base(boundaries) { }
        public Texture2D Chart { get; set; }
        public int Hue { get; set; } = 0;
        public Vector2 PositionOnChart { get; set; } = Vector2.Zero;
        private bool updatedChart = false;

        public static Color ColorFromHSV(float hue, float saturation, float value) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

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

        public void UpdateChart() {
            PositionOnChart = new Vector2((MathF.Max(MathF.Min(1, PositionOnChart.X), 0)), MathF.Max(MathF.Min(1, PositionOnChart.Y), 0));

            Hue = Commons.Modulo(Hue, 255);
            if (Chart == null)
                Chart = new Texture2D(GameManager.GraphicsDeviceManager.GraphicsDevice, Boundaries.Width, Boundaries.Height);

            Color[] chartData = new Color[Boundaries.Width * Boundaries.Height];

            for (int y = 0; y < Boundaries.Height; y++) {
                for (int x = 0; x < Boundaries.Width; x++) {
                    chartData[y * Boundaries.Width + x] = FetchColor(new Vector2((float)x/Boundaries.Width, (float)y/Boundaries.Height));
                }
            }

            Chart.SetData(chartData);
            EditorScreen.EditorColor = FetchColor(PositionOnChart);
        }

        public override void OnFocus() {
            if (InputManager.IsLeftButtonDown()) {
                System.Console.Write(InputManager.GetMousePosition());
                System.Console.WriteLine(Vector2.Transform(InputManager.GetMousePosition(), TransformMatrix));
                PositionOnChart = Vector2.Transform(InputManager.GetMousePosition(), TransformMatrix);
                EditorScreen.EditorColor = FetchColor(PositionOnChart);
                UpdateChart();
            }
        }

        private Color FetchColor(Vector2 positionOnChart) {
            return ColorFromHSV(Hue, (float)positionOnChart.X, 1f - (float)positionOnChart.Y);
        }

        public void FetchPositionAndHueFromColor(Color desiredColor) {
            PositionOnChart = Vector2.Zero;

            for (int y = 0; y < Boundaries.Height; y++)
            {
                for (int x = 0; x < Boundaries.Width; x++)
                {
                    Vector2 currentPosition = new Vector2((float)x / Boundaries.Width, (float)y / Boundaries.Height);
                    Color currentColor = FetchColor(currentPosition);

                    if (currentColor == desiredColor)
                    {
                        PositionOnChart = currentPosition;
                        Hue = Commons.Modulo(Hue, 255);
                    }
                }
            }
        }

        public override void Update() {
            base.Update();

            updatedChart = false;
            if (InputManager.IsKeyDown(Keys.U)) { Hue--; updatedChart = true; }
            if (InputManager.IsKeyDown(Keys.O)) { Hue++; updatedChart = true; }
            if (InputManager.IsKeyDown(Keys.I)) { PositionOnChart += new Vector2(0, -0.01f); updatedChart = true; }
            if (InputManager.IsKeyDown(Keys.K)) { PositionOnChart += new Vector2(0, 0.01f); updatedChart = true; }
            if (InputManager.IsKeyDown(Keys.J)) { PositionOnChart += new Vector2(-0.01f, 0); updatedChart = true; }
            if (InputManager.IsKeyDown(Keys.L)) { PositionOnChart += new Vector2(0.01f, 0); updatedChart = true; }

            if (updatedChart) UpdateChart();
        }

        public override void Draw() {
            GameManager.DrawFilledRectangle(new Rectangle(Boundaries.X - 8, Boundaries.Y - 8, Boundaries.Width + 16, Boundaries.Height + 16), EditorScreen.EditorColor);
            GameManager.SpriteBatch.Draw(Chart, Boundaries, Color.White);
            GameManager.SpriteBatch.DrawPoint(new Vector2(Boundaries.X, Boundaries.Y) + PositionOnChart * Boundaries.Width, Commons.InvertColor(EditorScreen.EditorColor), 8);
            base.Draw();
        }
    }
}