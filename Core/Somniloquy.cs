using var somniloquy = new Somniloquy.SQ();
somniloquy.Run();

namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    // dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained

    public class SQ : Game {
        public static GraphicsDeviceManager GDM;
        public static GraphicsDevice GD;
        public static ContentManager CM;
        public static SQSpriteBatch SB;
        public static GameTime GameTime;
        public long GameTick = 0;
        public int TargetFPS = 120;
        public float FPS;
        private Queue<float> timeSamples;

        public static Vector2I WindowSize;
        public static bool IsWindowActive;
        public static Dictionary<string, Texture2D> Textures;
        public static SpriteFont Misaki;

        public SQ() {
            GDM = new GraphicsDeviceManager(this);
            base.Content.RootDirectory = "Content";
            IsMouseVisible = true;

            GDM.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            GDM.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 48;

            GDM.HardwareModeSwitch = false;
            GDM.IsFullScreen = false;
            GDM.SynchronizeWithVerticalRetrace = true;
            GDM.ApplyChanges();

            TargetElapsedTime = TimeSpan.FromSeconds(1d / TargetFPS);
            timeSamples = new(10);

            Window.IsBorderless = true;
            Window.Position = new Vector2I(0, 0);

            WindowSize = new Vector2I(GDM.PreferredBackBufferWidth, GDM.PreferredBackBufferHeight);
            FocusWindow();
        }

        protected override void Initialize() {
            base.Initialize();
            // SerializationManager.InitializeDirectories((typeof(World), "Worlds"), (typeof(Texture2D), "Textures"));
            InputManager.Initialize(Window);
            SoundManager.Initialize("C:\\Somnia\\Projects\\monogame-somniloquy\\Assets\\Loops");
            FileBrowser.Initialize();
        }

        protected override void LoadContent() {
            base.LoadContent();
            GD = GraphicsDevice;
            SB = new SQSpriteBatch(GD);
            CM = Content;

            Misaki = Content.Load<SpriteFont>("Fonts/Misaki");

            ScreenManager.AddScreen(new Section2DScreen(new Rectangle(new(), WindowSize)));
            ScreenManager.LoadContent();

            DebugInfo.Subscribe(() => $"FPS: {FPS:n1}");
            DebugInfo.Subscribe(() => $"Pen Pressure: {InputManager.PenPressure}");
            DebugInfo.Subscribe(() => $"Pen Tilt: {InputManager.PenTilt}");
            DebugInfo.Subscribe(() => $"Focused Screen: {ScreenManager.FocusedScreen}");
            
            DebugInfo.Subscribe(() => $"Undo History: {CommandManager.UndoHistory.Count}");
            DebugInfo.Subscribe(() => $"Redo History: {CommandManager.RedoHistory.Count}");
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void FocusWindow() {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            SetForegroundWindow(process.MainWindowHandle);
        }

        public static void SaveImage(Texture2D texture, string filePath) {
            using FileStream fileStream = new(filePath, FileMode.Create);
            texture.SaveAsPng(fileStream, texture.Width, texture.Height);
        }

        public static Texture2D LoadImage(string filePath) {
            using FileStream fileStream = new(filePath, FileMode.Open);
            return Texture2D.FromStream(GD, fileStream);
        }

        protected override void Update(GameTime gameTime) {
            GameTime = gameTime;
            GameTick++;

            if (IsActive) {
                InputManager.Update();
                ScreenManager.Update();
            } else {
                // InputManager.ResetKeyboardState();
            }

            SoundManager.Update();
            SQTexture2D.ApplyTextureChanges();
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            if (IsActive) {
                UpdateFPS(gameTime);

                GD.Clear(Color.Black);
                SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                    ScreenManager.Draw();
                    DebugInfo.Draw(Misaki);
                    FileBrowser.Draw(Misaki);
                SB.End();
                base.Draw(gameTime);
            }
        }

        private void UpdateFPS(GameTime gameTime) {
            float CurrentFramesPerSecond = (float)(1.0d / gameTime.ElapsedGameTime.TotalSeconds);
            timeSamples.Enqueue(CurrentFramesPerSecond);

            if (timeSamples.Count > 10) {
                timeSamples.Dequeue();
                FPS = timeSamples.Average(i => i);
            } else {
                FPS = CurrentFramesPerSecond;
            }
        }
    }
}