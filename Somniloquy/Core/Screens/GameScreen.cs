namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Media;
    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;

    public class GameScreen : Screen {
        public WorldScreen WorldScreen { get; set; }

        public List<World> LoadedWorlds { get; set; } = new();
        public List<Entity> Entities { get; set; } = new();
        public Camera2D Camera { get; set; } = new Camera2D(8.0f);

        public Effect BloomExtractEffect;
        public Effect GaussianBlurEffect;
        public Effect BloomCombineEffect;

        public EffectParameter BrightnessThreshold;

        public EffectParameter SampleWeights;
        public EffectParameter SampleOffsets;
        
        public EffectParameter BloomIntensity;
        public EffectParameter BaseIntensity;
        public EffectParameter BloomSaturation;
        public EffectParameter BaseSaturation;

        private Point windowSize = SQ.WindowSize;  // new(1280, 720);

        float pitch = 0.7f;
        static string music = "bgm006";

        public void LoadWorld(string worldName) {
            LoadedWorlds.Add(SerializationManager.Deserialize<World>(worldName));
        }

        public void UnloadWorld(World world) {
            LoadedWorlds.Remove(world);
        }

        public GameScreen(Rectangle boundaries) : base(boundaries) {
            SoundManager.StartLoop(music, 2f);
            SoundManager.CenterFrequency = 150f;
            SoundManager.SetPitch(music, pitch);

            BloomExtractEffect = SQ.CM.Load<Effect>("Shaders/BloomExtract");
            GaussianBlurEffect = SQ.CM.Load<Effect>("Shaders/GaussianBlur");
            BloomCombineEffect = SQ.CM.Load<Effect>("Shaders/BloomCombine");

            SampleWeights = GaussianBlurEffect.Parameters["SampleWeights"];
            SampleOffsets = GaussianBlurEffect.Parameters["SampleOffsets"];

            BloomIntensity = BloomCombineEffect.Parameters["BloomIntensity"];
            BloomIntensity.SetValue(1.25f);
            BaseIntensity = BloomCombineEffect.Parameters["BaseIntensity"];
            BaseIntensity.SetValue(1.0f);
            BloomSaturation = BloomCombineEffect.Parameters["BloomSaturation"];
            BloomSaturation.SetValue(1.0f);
            BaseSaturation = BloomCombineEffect.Parameters["BaseSaturation"];
            BaseSaturation.SetValue(1.0f);
        }

        public void AddPlayer() {
            var player = new Player {
                CollisionBounds = new CircleF(Point.Zero, 4f),
                Camera = Camera,
                CurrentLayer = LoadedWorlds[0].Layers[0]
            };
            Entities.Add(player);
        }

        public override void OnFocus() {
            
        }

        public override void Update() {
            if (InputManager.IsKeyDown(Keys.Left)) {
                if (pitch > 0.1f) pitch -= 0.001f;
                SoundManager.SetPitch(music, pitch);
            } else if (InputManager.IsKeyDown(Keys.Right)) {
                if (pitch < 2f) pitch += 0.001f;
                SoundManager.SetPitch(music, pitch);
            } else if (InputManager.IsKeyDown(Keys.Up)) {
                SoundManager.CenterFrequency *= 1.01f;
            }
            else if (InputManager.IsKeyDown(Keys.Down)) {
                SoundManager.CenterFrequency /= 1.01f;
            }            

            // if (InputManager.GetNumberKeyPress() != null) {
            //     if (InputManager.IsKeyPressed(Keys.Enter)) {
            //         SoundManager.StopLoop(music);

            //         if (InputManager.IsKeyDown(Keys.D1)) music = "moseni_fog";
            //         if (InputManager.IsKeyDown(Keys.D2)) music = "moseni_lotus";
            //         if (InputManager.IsKeyDown(Keys.D3)) music = "moseni_hydrangea";
            //         if (InputManager.IsKeyDown(Keys.D4)) music = "moseni_luna";
            //         if (InputManager.IsKeyDown(Keys.D5)) music = "loop_74";
            //         if (InputManager.IsKeyDown(Keys.D6)) music = "n3-KtO";
            //         if (InputManager.IsKeyDown(Keys.D7)) music = "n3-LI";
            //         if (InputManager.IsKeyDown(Keys.D8)) music = "本名_1_small";
            //         if (InputManager.IsKeyDown(Keys.D8)) music = "bgm006";

            //         SoundManager.CenterFrequency = 150f;
            //         SoundManager.StartLoop(music);
            //         pitch = 0.7f;
            //         SoundManager.SetPitch(music, pitch);
            //     }
            // } else if (InputManager.IsKeyPressed(Keys.Enter)) {
            //     SoundManager.StopLoop(music);
            //     ScreenManager.ToEditorScreen(this);
            // }

            foreach (var world in LoadedWorlds) {
                world.Update();
            }

            foreach (var entity in Entities) {
                entity.Update();
            }

            Camera.UpdateTransformation();
        }

        public override void Draw() {
            // RenderTarget2D sceneRender = RenderScene();
            // // RenderTarget2D bloomExtractRender = ExtractBloom(sceneRender);
            // // RenderTarget2D bloomBlurRender = BlurBloom(sceneRender);

            // Somniloquy.GD.SetRenderTarget(null);
            // BloomCombineEffect.CurrentTechnique.Passes[0].Apply();
            // Somniloquy.GD.Clear(Color.Black);
            // Somniloquy.SpriteBatch.Begin(0, BlendState.Additive, null, null, null, BloomCombineEffect);
            // //Somniloquy.SpriteBatch.Begin(0, BlendState.Additive, null, null, null, null);
            // SQ.SB.Draw(sceneRender, Vector2.Zero, Color.White);
            // SQ.SB.Draw(bloomBlurRender, Vector2.Zero, Color.White);
            // Somniloquy.SpriteBatch.End();

            // sceneRender.Dispose();
            // bloomExtractRender.Dispose();
            // bloomBlurRender.Dispose();
        }

        private RenderTarget2D BlurBloom(RenderTarget2D bloomExtractRender) {
            int sampleCount =11;
            RenderTarget2D gaussianBlurRenderHorizontal = new(SQ.GD, windowSize.X, windowSize.Y);
            SQ.GD.SetRenderTarget(gaussianBlurRenderHorizontal);
            SQ.GD.Clear(Color.Transparent);

            SampleWeights.SetValue(Utils.GetSampleWeights(sampleCount));
            SampleOffsets.SetValue(Utils.GetSampleOffsets(Direction.Horizontal, sampleCount));
            GaussianBlurEffect.CurrentTechnique.Passes[0].Apply();

            SQ.SB.Begin(0, BlendState.Opaque, null, null, null, GaussianBlurEffect);
            SQ.SB.Draw(bloomExtractRender, Vector2.Zero, Color.White);
            SQ.SB.End();

            RenderTarget2D gaussianBlurRenderVertical = new(SQ.GD, windowSize.X, windowSize.Y);
            SQ.GD.SetRenderTarget(gaussianBlurRenderVertical);
            SQ.GD.Clear(Color.Transparent);

            SampleWeights.SetValue(Utils.GetSampleWeights(sampleCount));
            SampleOffsets.SetValue(Utils.GetSampleOffsets(Direction.Vertical, sampleCount));
            GaussianBlurEffect.CurrentTechnique.Passes[0].Apply();

            SQ.SB.Begin(0, BlendState.Opaque, null, null, null, GaussianBlurEffect);
            SQ.SB.Draw(gaussianBlurRenderHorizontal, Vector2.Zero, Color.White);
            SQ.SB.End();

            gaussianBlurRenderHorizontal.Dispose();
            return gaussianBlurRenderVertical;
        }

        private RenderTarget2D ExtractBloom(RenderTarget2D sceneRender) {
            RenderTarget2D bloomExtractRender = new(SQ.GD, windowSize.X, windowSize.Y);
            BloomExtractEffect.CurrentTechnique.Passes[0].Apply();
            
            SQ.GD.SetRenderTarget(bloomExtractRender);
            SQ.GD.Clear(Color.Transparent);

            SQ.SB.Begin(0, BlendState.Opaque, null, null, null, BloomExtractEffect);
            SQ.SB.Draw(sceneRender, Vector2.Zero, Color.White);
            SQ.SB.End();

            return bloomExtractRender;
        }

        private RenderTarget2D RenderScene() {
            RenderTarget2D sceneRender = new(SQ.GD, windowSize.X, windowSize.Y);
            SQ.GD.SetRenderTarget(sceneRender);
            SQ.GD.Clear(Color.Transparent);
            SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);

            foreach (var world in LoadedWorlds) {
                foreach (var layer in world.Layers) {
                    layer.Draw(Camera);
                }
            }

            foreach (var entity in Entities) {
                entity.Draw();
            }

            SQ.SB.End();
            return sceneRender;
        }
    }
}