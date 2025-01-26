namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public struct Sides {
        public float Left, Right, Up, Down;
        
        public static Sides None => new Sides(0, 0, 0, 0);

        public Sides(float all) : this(all, all, all, all) { }
        public Sides(float horizontal, float vertical) : this(horizontal, horizontal, vertical, vertical) { }
        
        public Sides(float left, float right, float up, float down) {
            Left = left;
            Right = right;
            Up = up;
            Down = down;
        }

        public Vector2 TopLeft() => new Vector2(Left, Up);
        public Vector2 BottomRight() => new Vector2(Right, Down);

        public float GetSide(Axis axis, bool start) {
            if (axis == Axis.Horizontal) {
                if (start) return Left;
                else return Right;
            } else {
                if (start) return Up;
                else return Down;
            }
        }

        public float GetSideSum(Axis axis) {
            if (axis == Axis.Horizontal) {
                return Left + Right;
            } else {
                return Up + Down;
            }
        }

        public Sides GetPart(Axis axis, bool perpendicular = false) {
            if (axis == Axis.Horizontal ^ perpendicular) {
                return new Sides(Left, Right, 0, 0);
            } else {
                return new Sides(0, 0, Up, Down);
            }
        }
    }

    public abstract class Screen {
        public string Identifier;
        public List<Screen> Children = new();
        public bool Focusable = true;
        public bool Selectable = false;

        public bool Focused => ScreenManager.FocusedScreen == this;
        public bool Selected => ScreenManager.SelectedScreen == this;

        public virtual void LoadContent() { }

        public virtual void Update() {
            if (Focusable) {
                if (MouseWithinBoundaries() && !InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                    ScreenManager.FocusedScreen = this;
                }
            }
            
            foreach (var child in Children) {
                child.Update();
            }
        }

        public abstract bool MouseWithinBoundaries();

        public virtual void Draw() {
            foreach (var child in Children) {
                child.Draw();
            }
        }

        public Screen GetChildByID(string identifier) {
            return GetAllChildren().Find(child => child.Identifier == identifier);
        }

        public List<Screen> GetAllChildren() {
            List<Screen> allChildren = new();

            foreach (var child in Children) {
                allChildren.Add(child); // Add the current child
                allChildren.AddRange(child.GetAllChildren()); // Add all descendants of the current child
            }

            return allChildren;
        }

        public override string ToString() {
            if (this is null) return "";
            return $"{base.ToString()} {Identifier}";
        }

        public void Destroy() {
            ScreenManager.Screens.Remove(this);
            foreach (var child in Children) {
                child.Destroy();
            }
        }
    }
}