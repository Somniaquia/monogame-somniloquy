namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class ColorPicker : BoxUI {
        public Section2DEditor Screen;
        public ColorOkHSL ColorOkHSL;

        public HuePicker HuePicker;
        public Texture2D ChartTexture;
        public Vector2I ChartDimensions;
        public Vector2 PositionOnChart = Vector2.Zero;
        public float Hue = 0;

        public bool Active = true;

        public ColorPicker(Rectangle boundaries, Section2DEditor screen) : base(boundaries) {
            ChartDimensions = (Vector2I)(boundaries.Size.ToVector2() / 2);
            Screen = screen; 

            HuePicker = new(new Rectangle(boundaries.X, boundaries.Y - 24, boundaries.Width, 10), this);
        }

        public override void LoadContent() {
            ChartTexture = new Texture2D(SQ.GD, ChartDimensions.X, ChartDimensions.Y); 
            CreateChartTexture();

            HuePicker.LoadContent();
        }

        public void CreateChartTexture() {
            Color[] chartData = new Color[ChartDimensions.X * ChartDimensions.Y];

            for (int y = 0; y < ChartDimensions.Y; y++) {
                for (int x = 0; x < ChartDimensions.X; x++) {
                    chartData[y * ChartDimensions.X + x] = FetchColor(new Vector2((float)x/ChartDimensions.X, (float)y/ChartDimensions.Y));
                }
            }
            ChartTexture.SetData(chartData);
        }

        private Color FetchColor(Vector2 positionOnChart) {
            return new ColorOkHSL((byte)Hue, (byte)(positionOnChart.X * 255), (byte)(255 - positionOnChart.Y * 255)).ToRGB();
        }

        public void SetColor(Color desiredColor) {
            var hsl = desiredColor.ToOkHSL();
            SetColor(hsl);
        }

        public void SetColor(ColorOkHSL hsl) {
            ColorOkHSL = hsl;
            PositionOnChart = new Vector2(hsl.S / 255f, (255 - hsl.L) / 255f);
            Hue = hsl.H;
            CreateChartTexture();
        }

        public override void Update() {
            base.Update();
            if (!Active) return;

            HuePicker.Update();

            bool updateChart = false;

            float updateSpeed = InputManager.IsMouseButtonDown(MouseButtons.LeftButton) ? 0.001f : 0.004f;
            
            if (InputManager.IsKeyDown(Keys.U)) { Hue -= updateSpeed * 255; updateChart = true; }
            if (InputManager.IsKeyDown(Keys.O)) { Hue += updateSpeed * 255; updateChart = true; }

            if (InputManager.IsKeyDown(Keys.I)) { PositionOnChart += new Vector2(0, -updateSpeed); }
            if (InputManager.IsKeyDown(Keys.K)) { PositionOnChart += new Vector2(0, updateSpeed); }
            if (InputManager.IsKeyDown(Keys.J)) { PositionOnChart += new Vector2(-updateSpeed, 0); }
            if (InputManager.IsKeyDown(Keys.L)) { PositionOnChart += new Vector2(updateSpeed, 0); }

            if (updateChart) CreateChartTexture();

            if (Focused) {
                if (InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                    PositionOnChart = Vector2.Transform(InputManager.GetMousePosition(), Transform);
                    Screen.SelectedColor = FetchColor(PositionOnChart);
                    ColorOkHSL = Screen.SelectedColor.ToOkHSL();
                    CreateChartTexture();
                }
            }

            PositionOnChart = new Vector2(MathF.Max(MathF.Min(1, PositionOnChart.X), 0), MathF.Max(MathF.Min(1, PositionOnChart.Y), 0));
            Hue = Util.PosMod(Hue, 255);
            Screen.SelectedColor = FetchColor(PositionOnChart);
            ColorOkHSL = Screen.SelectedColor.ToOkHSL();
        }

        public override void Draw() {
            if (Active && Screen.EditorMode == EditorMode.PaintMode) {
                // int borderLength = (Focused) ? 8 : 6;
                int borderLength = 4;
                SQ.SB.DrawFilledRectangle(new RectangleF(Boundaries.X - borderLength, Boundaries.Y - borderLength, Boundaries.Width + borderLength * 2, Boundaries.Height + borderLength * 2), Screen.SelectedColor);
                SQ.SB.Draw(ChartTexture, (Rectangle)Boundaries, Color.White);
                SQ.SB.DrawCircle((Vector2I)(new Vector2(Boundaries.X, Boundaries.Y) + new Vector2(PositionOnChart.X * Boundaries.Width, PositionOnChart.Y * Boundaries.Height)), 8, Util.InvertColor(Screen.SelectedColor), true);
            }
        }
    }

    public class HuePicker : BoxUI {
        public ColorPicker ColorPicker;
        public Texture2D BarTexture;

        public HuePicker(Rectangle boundaries, ColorPicker colorPicker) : base(boundaries) {
            ColorPicker = colorPicker;
            BarTexture = new(SQ.GD, ColorPicker.ChartDimensions.X, 1);
        }

        public override void LoadContent() {
            Color[] data = new Color[ColorPicker.ChartDimensions.X];
            for (int i = 0; i < ColorPicker.ChartDimensions.X; i++) {
                data[i] = new ColorOkHSL((byte)((float)i / ColorPicker.ChartDimensions.X * 255), 127, 127).ToRGB();
            }
            BarTexture.SetData(data);
        }

        public override void Update() {
            if (ColorPicker.Active && Focused) {
                if (InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                    var positionOnBar = Vector2.Transform(InputManager.GetMousePosition(), Transform).X;
                    positionOnBar = Util.PosMod(positionOnBar, 1);
                    // Mouse.SetPosition(Util.PosMod);
                    ColorPicker.SetColor(new ColorOkHSL((byte)(positionOnBar * 255), ColorPicker.ColorOkHSL.S, ColorPicker.ColorOkHSL.L));
                }
            }
            base.Update();
        }

        public override void Draw() {
            if (ColorPicker.Active && ColorPicker.Screen.EditorMode == EditorMode.PaintMode) {
                int borderLength = 4;
                SQ.SB.DrawFilledRectangle(new RectangleF(Boundaries.X - borderLength, Boundaries.Y - borderLength, Boundaries.Width + borderLength * 2, Boundaries.Height + borderLength * 2), ColorPicker.Screen.SelectedColor);
                SQ.SB.Draw(BarTexture, (Rectangle)Boundaries, Color.White);
                SQ.SB.DrawCircle((Vector2I)(new Vector2(Boundaries.X, Boundaries.Y) + new Vector2(ColorPicker.Hue / 255f * Boundaries.Width, Boundaries.Height / 2)), 8, Util.InvertColor(new ColorOkHSL((byte)ColorPicker.Hue, 255, 255).ToRGB()), true);
            }
        }
    }
}