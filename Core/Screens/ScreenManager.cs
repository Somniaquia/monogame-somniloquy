namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public static class ScreenManager {
        public static List<Screen> Screens = new();
        public static List<List<Screen>> SelectableScreenCollections = new();

        public static Screen FocusedScreen;
        public static List<Screen> SelectedScreenCollection;
        public static Screen SelectedScreen;

        public static Screen AddScreen(Screen screen) {
            Screens.Add(screen);
            return screen;
        }

        public static T GetFirstScreenOfType<T>() where T : BoxScreen {
            return Screens.OfType<T>().FirstOrDefault();
        }

        public static void ShiftScreenCollection(int amount) {
            int index = SelectableScreenCollections.FindIndex(i => i == SelectedScreenCollection) + amount;
            SelectedScreenCollection = SelectableScreenCollections[Util.PosMod(index, SelectableScreenCollections.Count)];
        }
        
        public static void ShiftSelectedScreen(int amount) {
            int index = SelectedScreenCollection.FindIndex(i => i == SelectedScreen) + amount;
            SelectedScreenCollection = SelectableScreenCollections[Util.PosMod(index, SelectedScreenCollection.Count)];
        }

        public static void LoadContent() {
            foreach (var screen in Screens) {
                screen.LoadContent();
            }
        }

        public static void Update() {
            foreach (var screen in Screens) {
                screen.Update();
            }
        }

        public static void Draw() {
            foreach (var screen in Screens) {
                screen.Draw();
            }
        }
    }
}