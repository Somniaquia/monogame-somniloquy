namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class ETFractalScreen : Screen {
        public Camera2D Camera = new();
        public List<Keybind> CameraKeybinds = new();
        private Effect FractalEffect;
        private RenderTarget2D RenderTarget;

        public ETFractalScreen(Rectangle boundaries) : base(boundaries) {
            CameraKeybinds.Add(InputManager.RegisterKeybind(Keys.W, (parameters) => MoveScreen(new Vector2(0, -0.001f)), false));
            CameraKeybinds.Add(InputManager.RegisterKeybind(Keys.A, (parameters) => MoveScreen(new Vector2(-0.001f, 0)), false));
			CameraKeybinds.Add(InputManager.RegisterKeybind(Keys.S, (parameters) => MoveScreen(new Vector2(0, 0.001f)), false));
            CameraKeybinds.Add(InputManager.RegisterKeybind(Keys.D, (parameters) => MoveScreen(new Vector2(0.001f, 0)), false));

            CameraKeybinds.Add(InputManager.RegisterKeybind(Keys.Q, (parameters) => ZoomScreen(-0.05f), false));
            CameraKeybinds.Add(InputManager.RegisterKeybind(Keys.E, (parameters) => ZoomScreen(0.05f), false));
            CameraKeybinds.Add(InputManager.RegisterKeybind(Keys.OemPipe, (parameters) => Camera.TargetRotation = 0, true));
            CameraKeybinds.Add(InputManager.RegisterKeybind(Keys.OemOpenBrackets, (parameters) => RotateScreen(-0.05f), false));
            CameraKeybinds.Add(InputManager.RegisterKeybind(Keys.OemCloseBrackets, (parameters) => RotateScreen(0.05f), false));

            ShaderManager.ShaderUpdated += OnShaderUpdated;
        }

        private void OnShaderUpdated(string shaderName, Effect newShader) {
            if (shaderName == "ETFractal") {
                FractalEffect = newShader;
            }

            DebugInfo.AddTempLine(() => $"Shader updated: {shaderName}", 5);
        }

        public override void LoadContent() {
            Camera.LoadContent();
            RenderTarget = new RenderTarget2D(SQ.GD, SQ.GD.PresentationParameters.BackBufferWidth, SQ.GD.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            FractalEffect = SQ.CM.Load<Effect>("Shaders//ETFractal");
        }

        public void MoveScreen(params object[] parameters) {
            if (parameters.Length == 1 && parameters[0] is Vector2 direction) {
                Camera.MoveCamera(direction * 0.75f);
            }
        }

        public void ZoomScreen(params object[] parameters) {
            if (parameters.Length == 1 && parameters[0] is float ratio) {
                Camera.ZoomCamera(ratio * 0.75f);
            }
        }

        public void RotateScreen(params object[] parameters) {
            if (parameters.Length == 1 && parameters[0] is float ratio) {
                Camera.RotateCamera(ratio);
            }
        }

        public override void Update() {
            base.Update();
            Camera.Update();
        }

        public override void Draw() {
            if (FractalEffect is null) {
                SQ.GD.Clear(Color.Violet);
                return;
            }
            SQ.GD.SetRenderTarget(RenderTarget);
            SQ.GD.Clear(Color.Black);

            FractalEffect.Parameters["Offset"].SetValue(Camera.CenterPosInWorld);
            FractalEffect.Parameters["Zoom"].SetValue(Camera.ZoomInverse);
            FractalEffect.Parameters["MaxIterations"].SetValue(500);

            // Begin the effect and draw a full-screen quad
            FractalEffect.CurrentTechnique.Passes[0].Apply();

            // Render a full-screen quad
            SQ.SB.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, FractalEffect);
            SQ.SB.Draw(RenderTarget, new Rectangle(0, 0, RenderTarget.Width, RenderTarget.Height), Color.White);
            SQ.SB.End();

            // Reset the render target to the backbuffer
            SQ.GD.SetRenderTarget(null);

            // Draw the fractal render target to the screen
            SQ.SB.Begin();
            SQ.SB.Draw(RenderTarget, new Rectangle(0, 0, SQ.GD.PresentationParameters.BackBufferWidth, SQ.GD.PresentationParameters.BackBufferHeight), Color.White);
            SQ.SB.End();
        }
    }
}