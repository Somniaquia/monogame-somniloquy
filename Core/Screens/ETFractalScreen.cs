namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class ETFractalScreen : BoxUI {
        public Camera2D Camera = new();
        public List<Keybind> Keybinds = new();
        private Effect FractalEffect;
        private RenderTarget2D RenderTarget;
        private Vector2 TargetFractalParameter = new();
        private Vector2 FractalParameter = new();
        private bool Julia = false;
        private int FractalType;

        public ETFractalScreen(Rectangle boundaries) : base(boundaries) {
            Keybinds.Add(InputManager.RegisterKeybind(Keys.Tab, _ => Camera.TargetCenterPosInWorld = Vector2.Zero, TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.W, _ => MoveScreen(new Vector2(0, -0.001f)), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.A, _ => MoveScreen(new Vector2(-0.001f, 0)), TriggerOnce.False));
			Keybinds.Add(InputManager.RegisterKeybind(Keys.S, _ => MoveScreen(new Vector2(0, 0.001f)), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.D, _ => MoveScreen(new Vector2(0.001f, 0)), TriggerOnce.False));

            Keybinds.Add(InputManager.RegisterKeybind(Keys.I, _ => ShiftShaderParameter(new Vector2(0, -1f)), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.J, _ => ShiftShaderParameter(new Vector2(-1f, 0)), TriggerOnce.False));
			Keybinds.Add(InputManager.RegisterKeybind(Keys.K, _ => ShiftShaderParameter(new Vector2(0, 1f)), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.L, _ => ShiftShaderParameter(new Vector2(1f, 0)), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.U, _ => TargetFractalParameter = Vector2.Zero, TriggerOnce.False));

            Keybinds.Add(InputManager.RegisterKeybind(Keys.Space, _ => Julia = !Julia, TriggerOnce.True));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.OemPeriod, _ =>  FractalType++, TriggerOnce.True));
            // Keybinds.Add(InputManager.RegisterKeybind(Keys.OemComma, _ => FractalType--, TriggerOnce.True));

            Keybinds.Add(InputManager.RegisterKeybind(Keys.Q, _ => ZoomScreen(-0.05f), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.E, _ => ZoomScreen(0.05f), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.OemPipe, _ => Camera.TargetRotation = 0, TriggerOnce.True));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.OemOpenBrackets, _ => RotateScreen(-0.05f), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(Keys.OemCloseBrackets, _ => RotateScreen(0.05f), TriggerOnce.False));

            ShaderManager.ShaderUpdated += OnShaderUpdated;

            DebugInfo.Subscribe(() => $"Fractal Parameter: {FractalParameter}");
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

        public void ShiftShaderParameter(params object[] parameters) {
            if (parameters.Length == 1 && parameters[0] is Vector2 direction) {
                TargetFractalParameter += direction * 0.005f / Camera.Zoom;
            }
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

            FractalParameter = Vector2.Lerp(FractalParameter, TargetFractalParameter, 0.075f);
        }

        public override void Draw() {
            if (FractalEffect is null) {
                SQ.GD.Clear(Color.Violet);
                return;
            }
            SQ.GD.SetRenderTarget(RenderTarget);
            SQ.GD.Clear(Color.Black);

            FractalEffect.Parameters["Offset"].SetValue(Camera.CenterPosInWorld);
            FractalEffect.Parameters["Zoom"].SetValue(1 / Camera.Zoom);
            FractalEffect.Parameters["Rotation"].SetValue(-Camera.Rotation);
            FractalEffect.Parameters["MaxIterations"].SetValue(500);
            FractalEffect.Parameters["Time"].SetValue((float)SQ.GameTime.TotalGameTime.TotalSeconds);
            FractalEffect.Parameters["Param"].SetValue(FractalParameter);
            FractalEffect.Parameters["FractalType"].SetValue(FractalType);
            FractalEffect.Parameters["Julia"].SetValue(Julia);

            // Begin the effect and draw a full-screen quad
            FractalEffect.CurrentTechnique.Passes[0].Apply();

            SQ.SB.End();
            // Render a full-screen quad
            SQ.SB.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, null, null, FractalEffect);
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