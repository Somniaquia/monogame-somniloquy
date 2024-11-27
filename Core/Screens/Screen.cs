namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public enum Orientation { Horizontal, Vertical }

    public class Screen {
        public Rectangle Boundaries;
        public Matrix Transform;

        public Orientation DividingDirection = Orientation.Horizontal;
        public Dictionary<int, Screen> ChildScreens = new();
        public bool Focusable = true;

        public Screen(Rectangle boundaries) {
            Boundaries = boundaries;
            Transform = 
                Matrix.CreateTranslation(-boundaries.X, -boundaries.Y, 0f) * 
                Matrix.CreateScale(1f / boundaries.Width, 1f / boundaries.Height, 1f);
        }

        public virtual void LoadContent() { }

        /// <summary>
        /// The base update function focuses the screen if the mouse is inside it. Call base.Update() at top!
        /// </summary>
        public virtual void Update() {
            if (Focusable) {
                if (Util.IsWithinBoundaries((Vector2I)InputManager.GetMousePosition(), Boundaries)) {
                    if (!InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                        ScreenManager.FocusedScreen = this;
                    }
                }
            }

            foreach (var child in ChildScreens) {
                child.Value.Update();
            }
        }

        public virtual void Draw() {
            foreach (var child in ChildScreens) {
                child.Value.Draw();
            }
        }
    }
}