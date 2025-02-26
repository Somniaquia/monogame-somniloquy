namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public static class DebugInfo {
        private static List<Func<string>> lines = new();
        private static List<(Func<string>, double)> tempLines = new();
        public static bool Active = true;

        public static void Initialize() {
            InputManager.RegisterKeybind(Keys.OemTilde, _ => Active = !Active, TriggerOnce.True);
        }

        public static Func<string> Subscribe(Func<string> lineGenerator) {
            lines.Add(lineGenerator);
            return lineGenerator;
        }

        public static void Unsubscribe(Func<string> lineGenerator) {
            lines.Remove(lineGenerator);
        }

        public static void AddTempLine(Func<string> lineGenerator, double time = -1) {
            tempLines.Add((lineGenerator, time));
        }

        public static void AddEmptyLine() {
            Subscribe(() => "");
        }

        public static void Draw(SpriteFont font) {
            var color = Util.InvertColor(ScreenManager.GetFirstOfType<Section2DScreen>().Section.BackgroundColor);

            Vector2 position = new Vector2(1, 1);
            if (Active) {
                foreach (var lineGenerator in lines) {
                    SQ.SB.DrawString(font, lineGenerator(), position, color * 0.5f);
                    position.Y += 18;
                }
            }

            position = new Vector2(1, SQ.WindowSize.Y - 17);
            foreach (var (lineGenerator, time) in tempLines) {
                if (time == -1) {
                    SQ.SB.DrawString(font, lineGenerator(), position, Color.LightBlue);
                }
                try {
                    SQ.SB.DrawString(font, lineGenerator(), position, color * 0.5f * MathF.Min((float)time, 1f));
                } catch (ArgumentException) {
                    string caughtString = lineGenerator();
                    Console.WriteLine($"Cannot display one of he characters of {caughtString}");
                }
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