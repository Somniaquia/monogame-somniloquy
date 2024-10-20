namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    
    public enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }

    public static class DebugInfo {
        private static List<Func<string>> linesTopLeft = new List<Func<string>>();
        private static List<Func<string>> linesTopRight = new List<Func<string>>();
        private static List<Func<string>> linesBottomLeft = new List<Func<string>>();
        private static List<Func<string>> linesBottomRight = new List<Func<string>>();

        private static List<Func<string>> tempLines = new List<Func<string>>();

        public static void Subscribe(Func<string> lineGenerator, Corner corner = Corner.TopLeft) {
            var lines = corner == Corner.TopLeft ? linesTopLeft : corner == Corner.TopRight ? linesTopRight : corner == Corner.BottomLeft ? linesBottomLeft : linesBottomRight;
            lines.Add(lineGenerator);
        }

        public static void AddTempLine(Func<string> lineGenerator) {
            tempLines.Add(lineGenerator);
        }

        public static void AddEmptyLine(Corner corner = Corner.TopLeft) {
            Subscribe(() => "", corner);
        }

        public static void Draw(SpriteFont font) {
            Vector2 position = new Vector2(1, 1);
            foreach (var lineGenerator in linesTopLeft) {
                SQ.SB.DrawString(font, lineGenerator(), position, Color.White);
                position.Y += 18;
            }

            position = new Vector2(1, SQ.WindowSize.Y - 17);
            foreach (var lineGenerator in tempLines) {
                SQ.SB.DrawString(font, lineGenerator(), position, Color.White);
                position.Y -= 18;
            }

            tempLines.Clear();
        }
    }
}