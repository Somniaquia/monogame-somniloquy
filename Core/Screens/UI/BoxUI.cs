namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public enum Axis { Horizontal, Vertical }
    public enum Align { Begin, Center, End, Even, Custom }

    /// <summary>
    /// Boxy Screen elements
    /// </summary>
    public class BoxUI : Screen {
        public BoxUI Root; public bool RepositioningNeeded = false;
        public BoxUI Parent;
        public RectangleF Boundaries;
        public Matrix Transform;
        public BoxUIRenderer Renderer = new BoxUIDefaultRenderer();

        public Sides Margin;
        public Sides Padding;

        public Axis MainAxis;
        public Axis PerpendicularAxis => Util.Perpendicular(MainAxis);
        
        public Align MainAxisAlign, MainAxisAlignOverflow; // switches to MainAxisAlignOverflow when overflowed
        public List<float> DivisionLocations = new();
        public Align PerpendicularAxisAlign;
        
        public bool Overflowed;
        public RenderTarget2D ContentRenderTarget;
        public float ScrollValue;
        public float SmoothScrollValue;

        // Contrary to other positioning flags, fill and shrink flags apply to themselves instead of children
        public bool MainAxisFill, PerpendicularAxisFill; // Fill: Fits to the parent. The parent should have minimum length or should be fitting to parent
        public float MainAxisMin = 10, PerpendicularAxisMin = 10; 
        
        public bool MainAxisShrink, PerpendicularAxisShrink; 
        public float MainAxisMax = float.MaxValue, PerpendicularAxisMax = float.MaxValue;

        public BoxUI() {}

        public BoxUI(BoxUI parent, float margin = 0, float padding = 0) : this(parent, new Sides(margin), new Sides(padding), Axis.Horizontal, Align.Begin, Align.Begin) { }
        public BoxUI(BoxUI parent, Sides margin, Sides padding, Axis axis, Align mainAxisAlign, Align perpendicularAxisAlign) {
            Root = parent.Root;
            Root.RepositioningNeeded = true;
            Parent = parent;
            Parent.Children.Add(this);
            Margin = margin;
            Padding = padding;
            MainAxis = axis;
            MainAxisAlign = mainAxisAlign;
            PerpendicularAxisAlign = perpendicularAxisAlign;
        }

        public BoxUI(RectangleF boundaries) : this(boundaries, Sides.None, Axis.Vertical, Align.Begin, Align.Begin) { }
        public BoxUI(RectangleF boundaries, Sides padding, Axis axis, Align mainAxisAlign, Align perpendicularAxisAlign) {
            Root = this;
            Boundaries = boundaries;
            Transform = Matrix.CreateTranslation(-boundaries.X, -boundaries.Y, 0f) * 
                        Matrix.CreateScale(1f / boundaries.Width, 1f / boundaries.Height, 1f);

            Padding = padding;
            MainAxis = axis;
            MainAxisAlign = mainAxisAlign;
            PerpendicularAxisAlign = perpendicularAxisAlign;

            ScreenManager.AddScreen(this);
        }

        public override bool MouseWithinBoundaries() {
            Vector2 displacement = Vector2.Zero;
            if (Parent is not null) {
                if (Parent.MainAxis == Axis.Horizontal) displacement = new(Parent.SmoothScrollValue, 0); 
                if (Parent.MainAxis == Axis.Vertical) displacement = new(0, Parent.SmoothScrollValue); 
            }
            return Util.IsWithinBoundaries(InputManager.GetMousePosition(), Boundaries.Displace(displacement));
        }

        public void PositionChildren() {
            var children = Children.OfType<BoxUI>().ToList();
            if (children.Count == 0) return;
            PositionMain(children);
            PositionPerpendicular(children);

            foreach (var child in children) {
                child.PositionChildren();
            }

            RepositioningNeeded = false;
        }

        private void PositionMain(List<BoxUI> children) {
            if (MainAxisAlign == Align.Custom) {

            } else {
                float availableSpace = GetAvailableSpace(MainAxis);
                var fixedItems = children.Where(ui => !ui.MainAxisFill).ToList();
                var fillItems = children.Where(ui => ui.MainAxisFill).ToList();

                float fixedSpace = Enumerable.Range(0, children.Count + 1).Select(i => GetSeperationByIndex(MainAxis, i)).Sum();
                fixedSpace += fixedItems.Sum(ui => ui.GetContentLength(MainAxis));
                var remainingSpace = availableSpace - fixedSpace;

                Overflowed = remainingSpace < 0 && MainAxisShrink;
                var alignMode = Overflowed ? MainAxisAlignOverflow : MainAxisAlign;

                if (alignMode == Align.Begin) {
                    float position = MainAxis == Axis.Horizontal ? Boundaries.X : Boundaries.Y;

                    for (int i = 0; i < children.Count; i++) {
                        position += GetSeperationByIndex(MainAxis, i);
                        var thisLength = children[i].GetContentLength(MainAxis);
                        if (children[i].MainAxisFill) thisLength = remainingSpace / fillItems.Count;
                        var end = Overflowed? position + thisLength : Math.Min(position + thisLength, Boundaries.GetSide(MainAxis, false));
                        children[i].SetBoundariesAxis(MainAxis, position, end);
                        position += thisLength;
                    }
                } else if (alignMode == Align.End) {
                    float position = MainAxis == Axis.Horizontal ? Boundaries.Right : Boundaries.Bottom;

                    for (int i = 0; i < children.Count; i++) {
                        position -= GetSeperationByIndex(MainAxis, i);
                        var thisLength = children[i].GetContentLength(MainAxis);
                        if (children[i].MainAxisFill) thisLength = remainingSpace / fillItems.Count;
                        var start = Overflowed? position - thisLength : Math.Max(position - thisLength, Boundaries.GetSide(MainAxis, true));
                        children[i].SetBoundariesAxis(MainAxis, start, position);
                        position -= thisLength;
                    }
                } else if (alignMode == Align.Even && fillItems.Count == 0) {
                    float position = (MainAxis == Axis.Horizontal ? Boundaries.X : Boundaries.Y) + remainingSpace / fixedItems.Count;

                    for (int i = 0; i < children.Count; i++) {
                        position += remainingSpace / fixedItems.Count;
                        var thisLength = children[i].GetContentLength(MainAxis);
                        children[i].SetBoundariesAxis(MainAxis, position, position += thisLength);
                    }
                } else { // alignMode: center
                    float position = (MainAxis == Axis.Horizontal ? Boundaries.X : Boundaries.Y) + remainingSpace / 2;

                    for (int i = 0; i < children.Count; i++) {
                        position += GetSeperationByIndex(MainAxis, i);
                        var thisLength = children[i].GetContentLength(MainAxis);
                        if (children[i].MainAxisFill) thisLength = remainingSpace / fillItems.Count;
                        children[i].SetBoundariesAxis(MainAxis, position, position += thisLength);
                    }
                }

                if (Overflowed) {
                    ScrollValue = Math.Clamp(ScrollValue, 0, fixedSpace - availableSpace);
                    if (ContentRenderTarget == null || ContentRenderTarget.Bounds != (Rectangle)Boundaries) {
                        ContentRenderTarget?.Dispose();
                        ContentRenderTarget = MainAxis == Axis.Horizontal ? new RenderTarget2D(SQ.GD, (int)availableSpace, (int)Boundaries.Height) : new RenderTarget2D(SQ.GD, (int)Boundaries.Width, (int)availableSpace);
                    }
                } else {
                    ScrollValue = 0;
                    ContentRenderTarget?.Dispose();
                    ContentRenderTarget = null;
                }
            }
        }

        private void PositionPerpendicular(List<BoxUI> children) { // TODO: Perpendicular overflowing maybe later
            float availableSpace = GetAvailableSpace(PerpendicularAxis);
            
            foreach (var child in children) {
                var contentLength = child.GetContentLength(PerpendicularAxis);
                
                if (child.PerpendicularAxisFill || availableSpace < contentLength) {
                    float begin = Boundaries.GetSide(PerpendicularAxis, true) + GetSeperationByIndex(PerpendicularAxis, 0);
                    float end = Boundaries.GetSide(PerpendicularAxis, false) - GetSeperationByIndex(PerpendicularAxis, 1);
                    child.SetBoundariesAxis(PerpendicularAxis, begin, end);
                } else if (PerpendicularAxisAlign == Align.Begin) {
                    float begin = Boundaries.GetSide(PerpendicularAxis, true) + GetSeperationByIndex(PerpendicularAxis, 0);
                    float end = begin + contentLength;
                    child.SetBoundariesAxis(PerpendicularAxis, begin, end);
                } else if (PerpendicularAxisAlign == Align.End) {
                    float end = Boundaries.GetSide(PerpendicularAxis, false) - GetSeperationByIndex(PerpendicularAxis, 1);
                    float begin = end - contentLength;
                    child.SetBoundariesAxis(PerpendicularAxis, begin, end);
                } else if (PerpendicularAxisAlign is Align.Center or Align.Even) {
                    var leftOver = (Boundaries.GetAxisLength(PerpendicularAxis) - contentLength) / 2;
                    var begin = Boundaries.GetSide(PerpendicularAxis, true) + leftOver;
                    var end = Boundaries.GetSide(PerpendicularAxis, false) - leftOver;
                    
                    child.SetBoundariesAxis(PerpendicularAxis, begin, end);
                }
            }
        }

        public virtual float GetContentLength(Axis axis) {
            var children = Children.OfType<BoxUI>().ToList();
            if (children.Count == 0) return Padding.GetSideSum(axis);

            if (MainAxis == axis) {
                float length = Enumerable.Range(0, children.Count + 1).Select(i => GetSeperationByIndex(MainAxis, i)).Sum() + Children.Sum(ui => ((BoxUI)ui).GetContentLength(MainAxis));
                return Util.Min(Util.Max(MainAxisMin, length), MainAxisMax);
            } else {
                float length = children.Max(child => Util.Max(child.Margin.GetSide(axis, true), Padding.GetSide(axis, true)) + child.GetContentLength(axis) + Util.Max(child.Margin.GetSide(axis, false), Padding.GetSide(axis, false)));
                return Util.Min(Util.Max(PerpendicularAxisMin, length), PerpendicularAxisMax);
            }
        }

        public void SetBoundariesAxis(Axis axis, float begin, float end) {
            if (axis == Axis.Horizontal) {
                Boundaries = new(begin, Boundaries.Y, end - begin, Boundaries.Height); 
            } else {
                Boundaries = new(Boundaries.X, begin, Boundaries.Width, end - begin); 
            }
        }

        public float GetSeperationByIndex(Axis axis, int i) {
            float start = i == 0 ? Padding.GetSide(axis, false) : ((BoxUI)Children[i - 1]).Margin.GetSide(axis, false);
            float end = i == Children.Count ? Padding.GetSide(axis, true) : ((BoxUI)Children[i]).Margin.GetSide(axis, true);

            return Util.Max(start, end);
        }

        public float GetAvailableSpace(Axis axis) {
            return axis == Axis.Horizontal ? Boundaries.Width : Boundaries.Height;
        }

        public Align GetAxisAlign(Axis axis) {
            return axis == MainAxis ? MainAxisAlign : PerpendicularAxisAlign;
        }

        public override void Update() { // TODO: Dynamic resizing
            base.Update();

            if (Overflowed) {
                if (MouseWithinBoundaries()) ScrollValue = Math.Clamp(ScrollValue - InputManager.ScrollWheelDelta /10f, 0, GetContentLength(MainAxis) - Boundaries.GetAxisLength(MainAxis));
                SmoothScrollValue = Util.Lerp(SmoothScrollValue, ScrollValue, 0.075f);
            }
        }

        public override void Draw() => Draw(Vector2.Zero);
        public virtual void Draw(Vector2 displacement) {
            Renderer?.Draw(Boundaries.Displace(displacement));
            if (ContentRenderTarget is null) {
                foreach (var child in Children.OfType<BoxUI>()) {
                    child.Draw(displacement);
                } 
            } else {
                SQ.SB.End();
                SQ.GD.SetRenderTarget(ContentRenderTarget);
                SQ.GD.Clear(Color.Transparent);
                SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

                var scrollDisplacement = MainAxis == Axis.Horizontal ? new Vector2(-SmoothScrollValue, 0) : new Vector2(0, -SmoothScrollValue);
                
                // TODO: Optimize performance in directory with tons of files inside
                foreach (var child in Children.OfType<BoxUI>()) {
                    if (Util.IntersectsOrAdjacent(child.Boundaries.Displace(scrollDisplacement), Boundaries)) {
                        child.Draw(-Boundaries.TopLeft() + scrollDisplacement);
                    }
                }

                SQ.SB.End();
                SQ.GD.SetRenderTarget(null);
                SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                SQ.SB.Draw(ContentRenderTarget, (Rectangle)Boundaries, Color.White);
            }
        }
    }
}