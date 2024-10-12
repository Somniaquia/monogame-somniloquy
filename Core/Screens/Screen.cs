namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public enum Direction { Horizontal, Vertical }

    public class Screen {
        public Rectangle Boundaries { get; set; }
        public Matrix TransformMatrix { get; protected set; }

        public Direction DividingDirection = Direction.Horizontal;
        public Dictionary<int, Screen> ChildScreens { get; set; } = new();
        public bool Focusable { get; protected set; } = true;
        public bool Focused;

        public Screen(Rectangle boundaries) {
            Boundaries = boundaries;
            TransformMatrix = 
                Matrix.CreateTranslation(-boundaries.X, -boundaries.Y, 0f) * 
                Matrix.CreateScale(1f / boundaries.Width, 1f / boundaries.Height, 1f);
        }

        public virtual void OnFocus() {}

        /// <summary>
        /// Base updates if the screen is focused. Call base.Update() at top!
        /// </summary>
        public virtual void Update() {
            if (Focusable) {
                if (Util.IsWithinBoundaries((Vector2I)InputManager.GetMousePosition(), Boundaries)) {
                    if (!InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                        ScreenManager.FocusedScreen = this;
                        Focused = true;
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