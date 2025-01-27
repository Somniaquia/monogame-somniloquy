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
        public static BasicEffect BasicEffect;
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

        [DllImport("user32.dll")] private static extern bool SetProcessDpiAwarenessContext(int dpiFlag);
        [DllImport("user32.dll")] private static extern bool SetProcessDPIAware();
        [DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);

        public SQ() {
            base.Content.RootDirectory = "Content";
            IsMouseVisible = true;

            SetProcessDPIAware();
            SetProcessDpiAwarenessContext(-4); // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2

            GDM = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = GetSystemMetrics(0), // SM_CXSCREEN
                PreferredBackBufferHeight = GetSystemMetrics(1) - 72, // SM_CYSCREEN
                // PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, // SM_CXSCREEN
                // PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 48, // SM_CYSCREEN

                HardwareModeSwitch = false,
                IsFullScreen = false,
                SynchronizeWithVerticalRetrace = true
            };
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
            FileExplorer.Initialize();
            ShaderManager.Initialize();
            DebugInfo.Initialize();
        }

        protected override void LoadContent() {
            base.LoadContent();
            GD = GraphicsDevice;
            GD.RasterizerState = new RasterizerState { CullMode = CullMode.None };
            GD.BlendState = BlendState.AlphaBlend;
            GD.DepthStencilState = DepthStencilState.None;
            GD.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            InactiveSleepTime = TimeSpan.FromSeconds(1/30.0);
            
            BasicEffect = new BasicEffect(SQ.GD) {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(0, WindowSize.X, WindowSize.Y, 0, 0, 1),
                View = Matrix.Identity,
                World = Matrix.Identity
            };

            SB = new SQSpriteBatch(GD);
            CM = Content;

            Misaki = Content.Load<SpriteFont>("Fonts/Misaki");

            DebugInfo.Subscribe(() => $"FPS: {FPS:n1}");
            DebugInfo.Subscribe(() => $"Focused Screen: {ScreenManager.FocusedScreen}");
            
            var sectionScreen = new Section2DScreen(new Rectangle(new(), WindowSize));
            LayerTable.BuildUI();
            ScreenManager.LoadContent();
            ShaderManager.LoadContent(null);
        }

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

            SQTexture2D.ApplyTextureChanges();
            ScreenManager.RepositionChildren();
            SoundManager.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GD.Clear(Color.Black);
            UpdateFPS(gameTime);

            SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                ScreenManager.Draw();
                DebugInfo.Draw(Misaki);
            SB.End();
            base.Draw(gameTime);
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