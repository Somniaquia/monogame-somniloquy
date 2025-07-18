namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using WintabDN;

    public delegate void Action(params object[] parameters);
    public enum MouseButtons { LeftButton, RightButton, MiddleButton, XButton1, XButton2 }
    public enum TriggerOnce { True, False, Block }

    public class Keybind {
        public TriggerOnce TriggerOnce;
        public bool OrderSensitive;
        public object[] Buttons;
        public object[] ExclusiveButtons;
        public Action Action;
        public Action PostAction;
        public bool PreviouslySatisfied, Satisfied;

        public Keybind(object[] buttons, object[] exclusiveButtons, Action action, Action postAction, TriggerOnce triggerOnce, bool orderSensitive) {
            foreach (var button in buttons.Concat(exclusiveButtons)) {
                // Check that it's either a key or a valid mouse button input (Keys or MouseButton).
                if (button is not Keys && button is not MouseButtons) {
                    throw new Exception("Non-Key or MouseButton type passed to Key Combinations");
                }
            }

            Buttons = buttons;
            ExclusiveButtons = exclusiveButtons;
            Action = action;
            PostAction = postAction;
            TriggerOnce = triggerOnce;
            OrderSensitive = orderSensitive;
        }
    }
    
    public static class InputManager {
        private static CWintabData wintabData;

        public static MouseState PreviousMouseState;
        public static MouseState CurrentMouseState;
        public static int ScrollWheelDelta;
        public static Queue<float> MouseSpeedSamples = new();
        public static float AverageMouseSpeed;
        public static Dictionary<Keys, KeyState> KeyStates = new();
        public static List<Keys> PressedKeys = new();
        public static Dictionary<MouseButtons, KeyState> MouseButtonStates = new();
        public static List<Keybind> Keybinds = new();

        public static bool[] PenButtons = new bool[3];
        public static float AveragePenPressure;
        public static Queue<float> PenPressureSamples = new();
        public static Vector2 PenTilt = Vector2.Zero;

        public class KeyState {
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

            DebugInfo.Subscribe(() => $"Pressed Keys: {string.Join(" ", PressedKeys.Select(key => key.ToString()))}");
            // DebugInfo.Subscribe(() => $"Mouse Speed / 100: {AverageMouseSpeed / 100}");
        }

        public static void Update() {
            UpdateKeyboardState();
            UpdateMouseState();
            UpdateTabletState();

            foreach (var keybind in Keybinds.ToList()) {
                UpdateKeybindStatus(keybind);
                if (keybind.Satisfied) { 
                    keybind.Action?.Invoke(); 
                }
            }

            foreach (var keybind in Keybinds.ToList()) {
                if (!keybind.Satisfied && keybind.PreviouslySatisfied) {
                    keybind.PostAction?.Invoke();
                }
            }

            // PressedKeys = KeyStates.Where(pair => pair.Value.IsDown).Select(pair => pair.Key).ToList();
            foreach (var pair in KeyStates) {
                if (pair.Value.IsDown && !PressedKeys.Contains(pair.Key)) PressedKeys.Add(pair.Key);
                if (!pair.Value.IsDown && PressedKeys.Contains(pair.Key)) PressedKeys.Remove(pair.Key);
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
            PreviousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            ScrollWheelDelta = CurrentMouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue;
            float currentMouseSpeed = (float)((CurrentMouseState.Position - PreviousMouseState.Position).ToVector2().Length() / (float)SQ.GameTime.ElapsedGameTime.TotalSeconds);
            MouseSpeedSamples.Enqueue(currentMouseSpeed);

            if (MouseSpeedSamples.Count > 20) {
                MouseSpeedSamples.Dequeue();
                AverageMouseSpeed = MouseSpeedSamples.Average(i => i);
            } else {
                AverageMouseSpeed = currentMouseSpeed;
            }

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
            uint count = 10; float currentPressure = 0f;

            float maxPressure = CWintabInfo.GetMaxPressure();
            WintabPacket[] packets = wintabData.GetDataPackets(count, true, ref count);
            
            for (int i = 0; i < count; i++) {
                currentPressure = MathF.Max(currentPressure, packets[i].pkNormalPressure / maxPressure);
            }

            PenPressureSamples.Enqueue(currentPressure);

            if (PenPressureSamples.Count > 10) {
                PenPressureSamples.Dequeue();
                AveragePenPressure = PenPressureSamples.Average(i => i);
            } else {
                AveragePenPressure = currentPressure;
            }
        }

        public static Keybind RegisterKeybind(object button, Action action, TriggerOnce triggerOnce) => RegisterKeybind(new object[] { button }, action, triggerOnce);
        public static Keybind RegisterKeybind(object button, object exclusiveButton, Action action, TriggerOnce triggerOnce) => RegisterKeybind(new object[] { button }, new object[] { exclusiveButton }, action, triggerOnce);
        public static Keybind RegisterKeybind(object button, object[] exclusiveButtons, Action action, TriggerOnce triggerOnce) => RegisterKeybind(new object[] { button }, exclusiveButtons, action, triggerOnce);
        public static Keybind RegisterKeybind(object[] buttons, Action action, TriggerOnce triggerOnce, bool orderSensitive = false) => RegisterKeybind(buttons, new object[] { }, action, triggerOnce, orderSensitive);
        public static Keybind RegisterKeybind(object[] buttons, object exclusiveButton, Action action, TriggerOnce triggerOnce, bool orderSensitive = false) => RegisterKeybind(buttons, new object[] { exclusiveButton }, action, triggerOnce, orderSensitive);
        public static Keybind RegisterKeybind(object[] buttons, object[] exclusiveButtons, Action action, TriggerOnce triggerOnce, bool orderSensitive = false) => RegisterKeybind(buttons, exclusiveButtons, action, null, triggerOnce);

        public static Keybind RegisterKeybind(object[] buttons, object[] exclusiveButtons, Action action, Action postAction, TriggerOnce triggerOnce, bool orderSensitive = false) {
            foreach (var button in buttons.Concat(exclusiveButtons)) {
                if (button is Keys key) {
                    if (!KeyStates.ContainsKey(key))
                        KeyStates[key] = new KeyState();
                }
            }

            var keybind = new Keybind(buttons, exclusiveButtons, action, postAction, triggerOnce, orderSensitive);
            Keybinds.Add(keybind);
            return keybind;
        }

        public static void UnregisterKeybind(Keybind keybind) => Keybinds.Remove(keybind);

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

        public static float GetPenPressure() => AveragePenPressure;
        public static Vector2 GetPenTilt() => PenTilt;
        public static bool IsPenButtonPressed(int buttonIndex) {
            if (buttonIndex < 1 || buttonIndex > 3) {
                throw new ArgumentException("Invalid pen button index");
            }
            return PenButtons[buttonIndex - 1];
        }

        public static void UpdateKeybindStatus(Keybind keybind) {
            keybind.PreviouslySatisfied = keybind.Satisfied;

            foreach (var button in keybind.ExclusiveButtons) {
                if (button is Keys key) {
                    if (IsKeyDown(key)) { keybind.Satisfied = false; return; }
                } else if (button is MouseButtons mouseButton) {
                    if (IsMouseButtonDown(mouseButton)) { keybind.Satisfied = false; return; }
                } else {
                    throw new Exception("Non-Key or MouseButton type passed to Key Combinations");
                }
            }
            
            if (keybind.TriggerOnce == TriggerOnce.True) {
                if (keybind.Buttons[^1] is Keys key) {
                    if (!IsKeyPressed(key)) { keybind.Satisfied = false; return; }
                } else if (keybind.Buttons[^1] is MouseButtons mouseButton) {
                    if (!IsMouseButtonPressed(mouseButton)) { keybind.Satisfied = false; return; } 
                } else {
                    throw new Exception("Non-Key or MouseButton type passed to Key Combinations");
                }
            } else if (keybind.TriggerOnce == TriggerOnce.Block) {
                if (keybind.Buttons[^1] is Keys key) {
                    if ((!IsKeyPressed(key) && ElapsedTicksSinceKeyPressed(key) < 60) || ElapsedTicksSinceKeyPressed(key) % 5 != 0) { keybind.Satisfied = false; return; }
                } else if (keybind.Buttons[^1] is MouseButtons mouseButton) {
                    if ((!IsMouseButtonPressed(mouseButton) && ElapsedSecondsSinceMouseButtonPressed(mouseButton) < 60) || ElapsedTicksSinceMouseButtonPressed(mouseButton) % 5 != 0) { keybind.Satisfied = false; return; } 
                } else {
                    throw new Exception("Non-Key or MouseButton type passed to Key Combinations");
                }
            }

            foreach (object button in keybind.Buttons) {
                if (button is Keys key) {
                    if (!IsKeyDown(key)) { keybind.Satisfied = false; return; } // One of the keys isn't pressed
                } else if (button is MouseButtons mouseButton) {
                    if (!IsMouseButtonDown(mouseButton)) { keybind.Satisfied = false; return; }
                } else {
                    throw new Exception("Non-Key or MouseButton type passed to Key Combinations");
                }
            }

            if (keybind.OrderSensitive) {
                for (int i = 0; i < keybind.Buttons.Length - 1; i++) {
                    // Every instance of keyOrMouseButtons are proved to be either Key or MouseButton thus usage of ternary will be ok
                    var currentKeyElapsedTicks = keybind.Buttons[i] is Keys key1 ? KeyStates[key1].ElapsedTicks : MouseButtonStates[(MouseButtons)keybind.Buttons[i]].ElapsedTicks;
                    var anteriorKeyElapsedTicks = keybind.Buttons[i] is Keys key ? KeyStates[key].ElapsedTicks : MouseButtonStates[(MouseButtons)keybind.Buttons[i]].ElapsedTicks;
                    if (currentKeyElapsedTicks < anteriorKeyElapsedTicks) { keybind.Satisfied = false; return; } // Wrong order
                }
            }

            keybind.Satisfied = true;
        }
    }
}