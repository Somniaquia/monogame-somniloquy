namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    
    public static class DebugInfo {
        private static List<Func<string>> lines = new();
        private static List<(Func<string>, double)> tempLines = new();

        public static void Subscribe(Func<string> lineGenerator) {
            lines.Add(lineGenerator);
        }

        public static void AddTempLine(Func<string> lineGenerator, double time = -1) {
            tempLines.Add((lineGenerator, time));
        }

        public static void AddEmptyLine() {
            Subscribe(() => "");
        }

        public static void Draw(SpriteFont font) {
            Vector2 position = new Vector2(1, 1);
            foreach (var lineGenerator in lines) {
                SQ.SB.DrawString(font, lineGenerator(), position, Color.White);
                position.Y += 18;
            }

            position = new Vector2(1, SQ.WindowSize.Y - 17);
            foreach (var (lineGenerator, time) in tempLines) {
                if (time == -1) {
                    SQ.SB.DrawString(font, lineGenerator(), position, Color.LightBlue);
                }
                SQ.SB.DrawString(font, lineGenerator(), position, Color.White * MathF.Min((float)time, 1f));
                position.Y -= 18;
            }

            var delta = SQ.GameTime.ElapsedGameTime.TotalSeconds;
            for (int i = 0; i < tempLines.Count; i++) {
                var pair = tempLines[i];
                var updatedPair = (pair.Item1, pair.Item2 - delta);
                
                if (updatedPair.Item2 <= 0) {
                    tempLines.RemoveAt(i);
                    i--;
                } else {
                    tempLines[i] = updatedPair;
                }
            }
        }
    }
}