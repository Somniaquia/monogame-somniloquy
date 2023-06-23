namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    public static class InputManager {
        public static object Focus { get; set; }
        private static KeyboardState currentKeyboardState;
        private static KeyboardState previousKeyboardState;

        private static MouseState currentMouseState;
        private static MouseState previousMouseState;

        //public List<UIFrame> 

        public static void Update() {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
        }

        public static bool IsKeyDown(Keys key) => currentKeyboardState.IsKeyDown(key);
        public static bool IsKeyPressed(Keys key) => currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
        public static bool IsKeyReleasesd(Keys key) => currentKeyboardState.IsKeyUp(key) && previousKeyboardState.IsKeyDown(key);

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
        public static int GetMiddleButtonDelta() => currentMouseState.ScrollWheelValue;
    }
}