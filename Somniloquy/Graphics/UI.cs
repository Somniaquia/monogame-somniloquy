namespace Somniloquy
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public abstract class UIFrame {
        public Rectangle Boundaries { get; set; }
        public Matrix TransformMatrix { get; protected set; }
        public List<UIFrame> ChildUIFrames { get; set; } = new();

        public UIFrame(Rectangle boundaries) {
            Boundaries = boundaries;
            TransformMatrix = Matrix.CreateScale(
                1f / ResourceManager.GraphicsDeviceManager.PreferredBackBufferWidth, 
                1f / ResourceManager.GraphicsDeviceManager.PreferredBackBufferHeight, 
                1f) * Matrix.CreateTranslation(boundaries.X, boundaries.Y, 0f);
        }

        public abstract void OnFocus();

        public void Update() {
            if (Commons.IsWithinBoundaries(InputManager.GetMousePosition().ToPoint(), Boundaries)) {
                if (InputManager.IsLeftButtonClicked()) {
                    InputManager.Focus = this;
                } else if (InputManager.IsLeftButtonReleased()) {
                    InputManager.Focus = null;
                }
            }

            foreach (var child in ChildUIFrames) {
                child.Update();
            }

            if (InputManager.Focus == this) {
                OnFocus();
            }
        }

        public virtual void Draw() {
            foreach (var child in ChildUIFrames) {
                child.Draw();
            }
        }
    }

    public class ColorChart : UIFrame {
        public ColorChart(Rectangle boundaries) : base(boundaries) { }
        public Texture2D Chart { get; set; }
        public int Hue { get; set; } = 0;

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
            Hue = Commons.Modulo(Hue, 255);
            if (Chart == null)
                Chart = new Texture2D(ResourceManager.GraphicsDeviceManager.GraphicsDevice, Boundaries.Width, Boundaries.Height);

            Color[] chartData = new Color[Boundaries.Width * Boundaries.Height];

            for (int y = 0; y < Boundaries.Height; y++) {
                for (int x = 0; x < Boundaries.Width; x++) {
                    chartData[y * Boundaries.Width + x] = FetchColor(new Vector2((float)x/Boundaries.Width, (float)y/Boundaries.Height));
                }
            }

            Chart.SetData(chartData);
        }

        public override void OnFocus() {
            if (InputManager.IsLeftButtonDown()) {
                FetchColor(Vector2.Transform(InputManager.GetMousePosition(), TransformMatrix));
            }

            base.Update();
        }

        public Color FetchColor(Vector2 positionOnChart) {
            return ColorFromHSV(Hue, (float)positionOnChart.X, 1f - (float)positionOnChart.Y);
        }

        public Point FetchPositionOnChart(Color color) {
            return Point.Zero;
        }

        public override void Draw() {
            ResourceManager.SpriteBatch.Draw(Chart, Boundaries, Color.White);
            base.Draw();
        }
    }
}