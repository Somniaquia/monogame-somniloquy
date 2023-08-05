namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;

    public static class ScreenManager {
        public static List<Screen> ActiveScreens { get; set; } = new();

        public static void AddScreen(Screen screen) {
            ActiveScreens.Add(screen);
        }

        public static void ToGameScreen(EditorScreen editorScreen) {
            GameScreen gameScreen = new(GameManager.WindowSize) {
                WorldScreen = editorScreen.WorldScreen
            };
            gameScreen.LoadedWorlds.Add(editorScreen.LoadedWorld);
            gameScreen.AddPlayer();

            ActiveScreens.Remove(editorScreen);
            ActiveScreens.Add(gameScreen);
        }

        public static void ToEditorScreen(GameScreen gameScreen) {            
            EditorScreen editorScreen = new(GameManager.WindowSize) {
                WorldScreen = gameScreen.WorldScreen,
                LoadedWorld = gameScreen.LoadedWorlds[0]
            };

            ActiveScreens.Remove(gameScreen);
            ActiveScreens.Add(editorScreen);
        }

        public static void Update() {
            foreach (var screen in ActiveScreens.ToArray()) {
                screen.Update();
            }
        }

        public static void Draw() {
            foreach (var screen in ActiveScreens.ToArray()) {
                screen.Draw();
            }
        }
    }
}