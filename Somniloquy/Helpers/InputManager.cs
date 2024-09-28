namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using WintabDN;

    public static class InputManager {
        public static object Focus { get; set; }
        private static KeyboardState currentKeyboardState;
        private static KeyboardState previousKeyboardState;

        private static MouseState currentMouseState;
        private static float penPressure;
        private static MouseState previousMouseState;
        private static CWintabData wintabData;

        public static void Initialize(GameWindow window) {
            SDL2.SDL.SDL_SysWMinfo systemInfo = new SDL2.SDL.SDL_SysWMinfo();
            SDL2.SDL.SDL_VERSION(out systemInfo.version);
            SDL2.SDL.SDL_GetWindowWMInfo(window.Handle, ref systemInfo);

            CWintabContext logContext = CWintabInfo.GetDefaultSystemContext(ECTXOptionValues.CXO_MESSAGES);
            logContext.Open(systemInfo.info.win.window, true);
            wintabData = new CWintabData(logContext);
        }

        public static void Update() {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            // Thank you Apos for sharing the code!
            float maxPressure = CWintabInfo.GetMaxPressure();

            uint count = 0; penPressure = 0;
            // WintabPacket[] results = wintabData.GetDataPackets(1, true, ref count);
            // for (int i = 0; i < count; i++) {
            //     penPressure = results[i].pkNormalPressure / maxPressure;
            // }

            // Keys[] pressedKeys = currentKeyboardState.GetPressedKeys();
            // if (pressedKeys.Length > 0)
            //     System.Console.WriteLine(pressedKeys[0]);
        }

        public static bool IsKeyDown(Keys key) => currentKeyboardState.IsKeyDown(key);
        public static bool IsKeyPressed(Keys key) => currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
        public static bool IsKeyReleasesd(Keys key) => currentKeyboardState.IsKeyUp(key) && previousKeyboardState.IsKeyDown(key);

        public static int? GetNumberKeyPress() {
            foreach (var key in new Keys[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 }) {
                if (IsKeyDown(key)) {
                    return (int)(key - Keys.D1) + 1;
                }
            }
            return null;
        }

        public static Vector2 GetPreviousMousePosition() => new Vector2(previousMouseState.Position.X, previousMouseState.Position.Y);
        public static Vector2 GetMousePosition() => new Vector2(currentMouseState.Position.X, currentMouseState.Position.Y);

        public static bool IsLeftButtonDown() => currentMouseState.LeftButton == ButtonState.Pressed;
        public static bool IsLeftButtonClicked() => currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
        public static bool IsLeftButtonReleased() => currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed;
    
        public static bool IsRightButtonDown() => currentMouseState.RightButton == ButtonState.Pressed;
        public static bool IsRightButtonClicked() => currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
        public static bool IsRightButtonReleased() => currentMouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed;
        
        public static bool IsMiddleButtonDown() => currentMouseState.MiddleButton == ButtonState.Pressed;
        public static bool IsMiddleButtonClicked() => currentMouseState.MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton == ButtonState.Released;
        public static int GetMiddleButtonDelta() => currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;

        public static float GetPenPressure() => 5 * penPressure;
    }
}