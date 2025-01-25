namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    public class TextLabel : BoxUI {
        public string Text;
        public Color DefaultColor = Color.White;
        public bool Editable;
        public Axis LetterAxis; // is each letter standing upright are or lying 90 degrees

        public Keys[] UntypedKeys = new Keys[] { 
            Keys.LeftControl, Keys.LeftShift, Keys.LeftAlt, Keys.LeftWindows, 
            Keys.RightControl, Keys.RightShift, Keys.RightAlt, Keys.RightWindows, 
            Keys.CapsLock, Keys.Tab, Keys.Escape, 
            Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12,
            Keys.Home, Keys.End, Keys.PageUp, Keys.PageDown, 
            Keys.Up, Keys.Down, Keys.Left, Keys.Right, 
            Keys.Insert, Keys.Delete, Keys.PrintScreen, Keys.Scroll, Keys.Pause, 
            Keys.NumLock, Keys.VolumeDown, Keys.VolumeUp, Keys.VolumeMute,
        };

        public Dictionary<Keys, (string, string)> KeyMappings = new() {
            { Keys.OemPeriod, (".", ">") },
            { Keys.OemComma, (",", "<") },
            { Keys.OemMinus, ("-", "_") },
            { Keys.OemPlus, ("=", "+") },
            { Keys.OemQuestion, ("/", "?") },
            { Keys.OemSemicolon, (";", ":") },
            { Keys.OemQuotes, ("'", "\"") },
            { Keys.OemOpenBrackets, ("[", "{") },
            { Keys.OemCloseBrackets, ("]", "}") },
            { Keys.OemPipe, ("\\", "|") },
            { Keys.OemBackslash, ("\\", "|") },
            { Keys.OemTilde, ("`", "~") },
            { Keys.Space, (" ", " ") },
            { Keys.Tab, ("\t", "\t") },
            { Keys.Enter, ("\n", "\n") }, 
            { Keys.D0, ("0", ")") },
            { Keys.D1, ("1", "!") },
            { Keys.D2, ("2", "@") },
            { Keys.D3, ("3", "#") },
            { Keys.D4, ("4", "$") },
            { Keys.D5, ("5", "%") },
            { Keys.D6, ("6", "^") },
            { Keys.D7, ("7", "&") },
            { Keys.D8, ("8", "*") },
            { Keys.D9, ("9", "(") }
        };


        public TextLabel(BoxUI parent, float margin = 0, float padding = 0, string text = null, Axis letterAxis = Axis.Horizontal, bool editable = false) : base(parent, margin, padding) {
            Text = text;
            Editable = editable;
            LetterAxis = letterAxis;
        }

        public TextLabel(Rectangle boundaries, string text = null, Axis letterAxis = Axis.Horizontal, bool editable = false) : base(boundaries) {
            Text = text;
            Editable = editable;
            LetterAxis = letterAxis;
        }

        public override float GetContentLength(Axis axis, BoxUI caller) {
            // TODO: Flexible indentation (wrapping text inside max axisLength)
            float length = base.GetContentLength(axis, caller);
            if (Text is null) return length;

            if (LetterAxis == axis) {
                Vector2 dimensions = SQ.Misaki.MeasureString(Text);
                length += axis == MainAxis ? dimensions.X : dimensions.Y;
            } else {
                if (axis == MainAxis) {
                    length += Text.Split('\n').Max(line => line.Length) * SQ.Misaki.LineSpacing;
                } else {
                    length += (Text.Count(s => s == '\n') + 1) * SQ.Misaki.LineSpacing;
                }
            }

            return length;
        }

        public override void Update() {
            base.Update(); 
            
            if (Editable) {
                Keys lastKey = Keys.None;
                foreach (var key in InputManager.PressedKeys) {
                    if (!UntypedKeys.Contains(key)) lastKey = key;
                }

                if (InputManager.IsKeyPressed(lastKey) || InputManager.ElapsedTicksSinceKeyPressed(lastKey) >= 60 && InputManager.ElapsedTicksSinceKeyPressed(lastKey) % 5 == 0) {
                    if (lastKey == Keys.Back) {
                        if (InputManager.IsKeyDown(Keys.LeftControl)) {
                            Text = "";
                        } else {
                            if (Text.Length != 0) Text = Text.Remove(Text.Length - 1);
                        }
                    } else if (lastKey == Keys.Enter || InputManager.IsKeyDown(Keys.LeftControl)) {
                        
                    } else if (KeyMappings.TryGetValue(lastKey, out var mappedValue)) {
                        if (InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift)) {
                            Text += mappedValue.Item2;
                        } else {
                            Text += mappedValue.Item1;
                        }
                    } else {
                        if (InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift)) {
                            Text += lastKey;
                        } else {
                            Text += lastKey.ToString().ToLower();
                        }
                    }
                }
            }
        }

        public override void Draw() {
            base.Draw();
            SQ.SB.DrawString(SQ.Misaki, Text, Boundaries.TopLeft() + Padding.TopLeft(), DefaultColor);    
        }   

        public override void Draw(Vector2 displacement) {
            base.Draw(displacement);
            if (Text is not null) SQ.SB.DrawString(SQ.Misaki, Text, Boundaries.TopLeft() + Padding.TopLeft() + displacement, DefaultColor);    
        }
    }
}