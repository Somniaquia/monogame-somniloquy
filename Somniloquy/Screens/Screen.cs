namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;

    public enum Direction { Horizontal, Vertical }

    public class Screen {
        public Rectangle Boundaries { get; set; }
        public Matrix TransformMatrix { get; protected set; }

        public Direction DividingDirection = Direction.Horizontal;
        public Dictionary<int, Screen> ChildScreens { get; set; } = new();
        public bool Focusable { get; protected set; } = true;

        public Screen(Rectangle boundaries) {
            Boundaries = boundaries;
            TransformMatrix = 
                Matrix.CreateTranslation(-boundaries.X, -boundaries.Y, 0f) * 
                Matrix.CreateScale(1f / boundaries.Width, 1f / boundaries.Height, 1f);
        }

        public virtual void OnFocus() { }

        public virtual void Update() {
            if (Focusable) {
                if (Utils.IsWithinBoundaries(Utils.ToPoint(InputManager.GetMousePosition()), Boundaries)) {
                    if (!InputManager.IsLeftButtonDown()) {
                        InputManager.Focus = this;
                    }
                }
            }

            foreach (var child in ChildScreens) {
                child.Value.Update();
            }

            if (InputManager.Focus == this) {
                OnFocus();
            }
        }

        public virtual void Draw() {
            foreach (var child in ChildScreens) {
                child.Value.Draw();
            }
        }
    }
}