namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public static class ScreenManager {
        public static List<Screen> ActiveScreens { get; set; } = new();
        public static Screen FocusedScreen;

        public static void AddScreen(Screen screen) {
            ActiveScreens.Add(screen);
        }

        public static void Update() {
            foreach (var screen in ActiveScreens) {
                screen.Update();
            }
        }

        public static void Draw() {
            foreach (var screen in ActiveScreens) {
                screen.Draw();
            }
        }
    }
}