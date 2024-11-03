namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using WintabDN;

    public delegate void Action(params object[] parameters);
    public enum MouseButtons { LeftButton, RightButton, MiddleButton, XButton1, XButton2 }

    public class Keybind {
        public bool TriggerOnce;
        public bool OrderSensitive;
        public object[] Buttons;
        public object[] ExclusiveButtons;
        public Action Action;

        public Keybind(object[] buttons, object[] exclusiveButtons, Action action, bool triggerOnce, bool orderSensitive) {
            foreach (var button in buttons.Concat(exclusiveButtons)) {
                // Check that it's either a key or a valid mouse button input (Keys or MouseButton).
                if (button is not Keys && button is not MouseButtons) {
                    throw new Exception("Non-Key or MouseButton type passed to Key Combinations");
                }
            }

            Buttons = buttons;
            ExclusiveButtons = exclusiveButtons;
            Action = action;
            TriggerOnce = triggerOnce;
            OrderSensitive = orderSensitive;
        }
    }
    
    public static class InputManager {
        private static CWintabData wintabData;

        private static MouseState CurrentMouseState;
        public static int ScrollWheelDelta;
        private static Dictionary<Keys, KeyState> KeyStates = new();
        private static Dictionary<MouseButtons, KeyState> MouseButtonStates = new();
        public static List<Keybind> Keybinds = new();
        public static List<Action> PostKeyActions = new();

        public static bool[] PenButtons = new bool[3];
        public static float PenPressure = 1.0f;
        public static Vector2 PenTilt = Vector2.Zero;

        private class KeyState {
            public bool IsDown;
            public int ElapsedTicks; // int can sufficiently handle roughly 165 hours of 60 frames per second before overflowing
            public double ElapsedSeconds; // These record elapsed time since STATE CHANGE, including time since key released. 

            public KeyState() {
                IsDown = false;
                ElapsedTicks = 0;
                ElapsedSeconds = 0;
            }
        }

        public static void Initialize(GameWindow window) {
            foreach (Keys key in Enum.GetValues(typeof(Keys))) {
                KeyStates[key] = new KeyState();
            }

            foreach (MouseButtons button in Enum.GetValues(typeof(MouseButtons))) {
                MouseButtonStates[button] = new KeyState();
            }

            SDL2.SDL.SDL_SysWMinfo systemInfo = new SDL2.SDL.SDL_SysWMinfo();
            SDL2.SDL.SDL_VERSION(out systemInfo.version);
            SDL2.SDL.SDL_GetWindowWMInfo(window.Handle, ref systemInfo);

            CWintabContext logContext = CWintabInfo.GetDefaultSystemContext(ECTXOptionValues.CXO_MESSAGES);
            logContext.Open(systemInfo.info.win.window, true);
            wintabData = new CWintabData(logContext);

            DebugInfo.Subscribe(() => $"Pressed Keys: {PrintPressedKeys()}");
        }

        public static void Update() {
            UpdateKeyboardState();
            UpdateMouseState();
            UpdateTabletState();

            foreach (var keybind in Keybinds) {
                if (IsKeyCombinationPressed(keybind.TriggerOnce, keybind.OrderSensitive, keybind.Buttons, keybind.ExclusiveButtons)) {
                    keybind.Action.Invoke();
                }
            }

            foreach (var postKeyAction in PostKeyActions) {
                postKeyAction.Invoke();
            }
        }

        private static void UpdateKeyboardState() {
            var currentKeyboardState = Keyboard.GetState();

            foreach (Keys key in Enum.GetValues(typeof(Keys))) {
                bool isDown = currentKeyboardState.IsKeyDown(key);
                var keyState = KeyStates[key];

                if (isDown) {
                    if (!keyState.IsDown) {
                        keyState.IsDown = true;
                        keyState.ElapsedTicks = 0;
                        keyState.ElapsedSeconds = 0;
                    } else {
                        keyState.ElapsedTicks++;
                        keyState.ElapsedSeconds += SQ.GameTime.ElapsedGameTime.TotalSeconds;
                    }
                }
                else {
                    if (keyState.IsDown) {
                        keyState.IsDown = false;
                        keyState.ElapsedTicks = 0;
                        keyState.ElapsedSeconds = 0;
                    } else {
                        keyState.ElapsedTicks++;
                        keyState.ElapsedSeconds += SQ.GameTime.ElapsedGameTime.TotalSeconds;
                    }
                }
            }
        }

        private static void UpdateMouseState() {
            var previousScrollWheelValue = CurrentMouseState.ScrollWheelValue;
            CurrentMouseState = Mouse.GetState();
            ScrollWheelDelta = CurrentMouseState.ScrollWheelValue - previousScrollWheelValue;

            foreach (MouseButtons button in Enum.GetValues(typeof(MouseButtons))) {
                bool isDown = IsMouseButtonDown(button);
                var mouseState = MouseButtonStates[button];

                if (isDown) {
                    if (!mouseState.IsDown) {
                        mouseState.IsDown = true;
                        mouseState.ElapsedTicks = 0;
                        mouseState.ElapsedSeconds = 0;
                    } else {
                        mouseState.ElapsedTicks++;
                        mouseState.ElapsedSeconds += SQ.GameTime.ElapsedGameTime.TotalSeconds;
                    }
                } else {
                    if (mouseState.IsDown) {
                        mouseState.IsDown = false;
                        mouseState.ElapsedTicks = 0;
                        mouseState.ElapsedSeconds = 0;
                    } else {
                        mouseState.ElapsedTicks++;
                        mouseState.ElapsedSeconds += SQ.GameTime.ElapsedGameTime.TotalSeconds;
                    }
                }
            }
        }

        private static void UpdateTabletState() {
            float maxPressure = CWintabInfo.GetMaxPressure();
            uint count = 0; PenPressure = 0;
            WintabPacket[] packets = wintabData.GetDataPackets(10, true, ref count);
            for (int i = 0; i < count; i++) {
                PenPressure = packets[i].pkNormalPressure / maxPressure;
            }
        }

        private static string PrintPressedKeys() {
            string pressedKeys = "";
            foreach (var pair in KeyStates) {
                if (pair.Value.IsDown) pressedKeys += pair.Key.ToString() + " ";
            }

            return pressedKeys;
        }

        public static Keybind RegisterKeybind(object button, Action action, bool triggerOnce = true) {
            return RegisterKeybind(new object[] {button}, action, triggerOnce);
        }

        public static Keybind RegisterKeybind(object button, object exclusiveButton, Action action, bool triggerOnce = true) {
            return RegisterKeybind(new object[] {button}, new object[] {exclusiveButton}, action, triggerOnce);
        }

        public static Keybind RegisterKeybind(object button, object[] exclusiveButtons, Action action, bool triggerOnce = true) {
            return RegisterKeybind(new object[] {button}, exclusiveButtons, action, triggerOnce);
        }

        public static Keybind RegisterKeybind(object[] buttons, Action action, bool triggerOnce = true, bool orderSensitive = false) {
            return RegisterKeybind(buttons, new object[] {}, action, triggerOnce, orderSensitive);
        }

        public static Keybind RegisterKeybind(object[] buttons, object exclusiveButton, Action action, bool triggerOnce = true, bool orderSensitive = false) {
            return RegisterKeybind(buttons, new object[] {exclusiveButton}, action, triggerOnce, orderSensitive);
        }

        public static Keybind RegisterKeybind(object[] buttons, object[] exclusiveButtons, Action action, bool triggerOnce = true, bool orderSensitive = false) {
            foreach (var button in buttons.Concat(exclusiveButtons)) {
                if (button is Keys key) {
                    if (!KeyStates.ContainsKey(key))
                        KeyStates[key] = new KeyState();
                }
            }

            var keybind = new Keybind(buttons, exclusiveButtons, action, triggerOnce, orderSensitive);
            Keybinds.Add(keybind);
            return keybind;
        }

        public static void UnregisterKeybind(Keybind keybind) => Keybinds.Remove(keybind);

        public static void RegisterPostKeyAction(Action action) => PostKeyActions.Add(action);
        public static void UnregisterPostKeyAction(Action action) => PostKeyActions.Remove(action);

        public static bool IsKeyDown(Keys key) => KeyStates[key].IsDown;
        public static bool IsKeyPressed(Keys key) => KeyStates[key].IsDown && KeyStates[key].ElapsedTicks == 0;
        public static bool IsKeyReleased(Keys key) => !KeyStates[key].IsDown && KeyStates[key].ElapsedTicks == 0;

        public static int ElapsedTicksSinceKeyPressed(Keys key) => KeyStates[key].IsDown ? KeyStates[key].ElapsedTicks : -1;
        public static int ElapsedTicksSinceKeyReleased(Keys key) => !KeyStates[key].IsDown ? KeyStates[key].ElapsedTicks : -1;
        public static double ElapsedSecondsSinceKeyPressed(Keys key) => KeyStates[key].IsDown ? KeyStates[key].ElapsedSeconds : -1;
        public static double ElapsedSecondsSinceKeyReleased(Keys key) => !KeyStates[key].IsDown ? KeyStates[key].ElapsedSeconds : -1;

        public static Vector2 GetMousePosition() => CurrentMouseState.Position.ToVector2();
        public static bool IsMouseButtonDown(MouseButtons button) {
            return button switch {
                MouseButtons.LeftButton => CurrentMouseState.LeftButton == ButtonState.Pressed,
                MouseButtons.RightButton => CurrentMouseState.RightButton == ButtonState.Pressed,
                MouseButtons.MiddleButton => CurrentMouseState.MiddleButton == ButtonState.Pressed,
                MouseButtons.XButton1 => CurrentMouseState.XButton1 == ButtonState.Pressed,
                MouseButtons.XButton2 => CurrentMouseState.XButton2 == ButtonState.Pressed,
                _ => throw new ArgumentException("Invalid MouseButtons value"),
            };
        }
        public static bool IsMouseButtonPressed(MouseButtons mouseButton) {
            bool result = MouseButtonStates[mouseButton].IsDown && MouseButtonStates[mouseButton].ElapsedTicks == 0;
            return result;
        }
        public static bool IsMouseButtonReleased(MouseButtons mouseButton) => !MouseButtonStates[mouseButton].IsDown && MouseButtonStates[mouseButton].ElapsedTicks == 0;

        public static int ElapsedTicksSinceMouseButtonPressed(MouseButtons mouseButton) => MouseButtonStates[mouseButton].IsDown ? MouseButtonStates[mouseButton].ElapsedTicks : -1;
        public static int ElapsedTicksSinceKeyReleased(MouseButtons mouseButton) => !MouseButtonStates[mouseButton].IsDown ? MouseButtonStates[mouseButton].ElapsedTicks : -1;
        public static double ElapsedSecondsSinceMouseButtonPressed(MouseButtons mouseButton) => MouseButtonStates[mouseButton].IsDown ? MouseButtonStates[mouseButton].ElapsedSeconds : -1;
        public static double ElapsedSecondsSinceKeyReleased(MouseButtons mouseButton) => !MouseButtonStates[mouseButton].IsDown ? MouseButtonStates[mouseButton].ElapsedSeconds : -1;

        public static float GetPenPressure() => PenPressure;
        public static Vector2 GetPenTilt() => PenTilt;
        public static bool IsPenButtonPressed(int buttonIndex) {
            if (buttonIndex < 1 || buttonIndex > 3) {
                throw new ArgumentException("Invalid pen button index");
            }
            return PenButtons[buttonIndex - 1];
        }

        public static bool IsKeyCombinationPressed(bool triggerOnce, bool orderSensitive, object[] buttons, object[] exclusiveButtons) {
            foreach (var button in exclusiveButtons) {
                if (button is Keys key) {
                    if (IsKeyDown(key)) return false;
                } else if (button is MouseButtons mouseButton) {
                    if (IsMouseButtonDown(mouseButton)) return false;
                } else {
                    throw new Exception("Non-Key or MouseButton type passed to Key Combinations");
                }
            }
            
            if (triggerOnce) {
                if (buttons[^1] is Keys key) {
                    if (!IsKeyPressed(key)) return false;
                } else if (buttons[^1] is MouseButtons mouseButton) {
                    if (!IsMouseButtonPressed(mouseButton)) return false; 
                } else {
                    throw new Exception("Non-Key or MouseButton type passed to Key Combinations");
                }
            }

            foreach (object button in buttons) {
                if (button is Keys key) {
                    if (!IsKeyDown(key)) return false; // One of the keys isn't pressed
                } else if (button is MouseButtons mouseButton) {
                    if (!IsMouseButtonDown(mouseButton)) return false;
                } else {
                    throw new Exception("Non-Key or MouseButton type passed to Key Combinations");
                }
            }

            if (orderSensitive) {
                for (int i = 0; i < buttons.Length - 1; i++) {
                    // Every instance of keyOrMouseButtons are proved to be either Key or MouseButton thus usage of ternary will be ok
                    var currentKeyElapsedTicks = buttons[i] is Keys key1 ? KeyStates[key1].ElapsedTicks : MouseButtonStates[(MouseButtons)buttons[i]].ElapsedTicks;
                    var anteriorKeyElapsedTicks = buttons[i] is Keys key ? KeyStates[key].ElapsedTicks : MouseButtonStates[(MouseButtons)buttons[i]].ElapsedTicks;
                    if (currentKeyElapsedTicks < anteriorKeyElapsedTicks) return false; // Wrong order
                }
            }

            return true;
        }
    }
}