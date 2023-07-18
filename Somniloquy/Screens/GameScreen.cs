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
        public List<World> LoadedWorlds { get; set; } = new();
        public List<Entity> Entities { get; set; } = new();
        public Camera Camera { get; set; } = new Camera(8.0f);

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


        float pitch = 0.7f;
        static string music = "moseni_fog";

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

            BloomExtractEffect = GameManager.ContentManager.Load<Effect>("Shaders/BloomExtract");
            GaussianBlurEffect = GameManager.ContentManager.Load<Effect>("Shaders/GaussianBlur");
            BloomCombineEffect = GameManager.ContentManager.Load<Effect>("Shaders/BloomCombine");

            BrightnessThreshold = BloomExtractEffect.Parameters["BrightnessThreshold"];
            BrightnessThreshold.SetValue(0.25f);

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

            if (InputManager.GetNumberKeyPress() != null) {
                if (InputManager.IsKeyPressed(Keys.Enter)) {
                    SoundManager.StopLoop(music);

                    if (InputManager.IsKeyDown(Keys.D1)) music = "moseni_fog";
                    if (InputManager.IsKeyDown(Keys.D2)) music = "moseni_lotus";
                    if (InputManager.IsKeyDown(Keys.D3)) music = "moseni_hydrangea";
                    if (InputManager.IsKeyDown(Keys.D4)) music = "moseni_luna";
                    if (InputManager.IsKeyDown(Keys.D5)) music = "loop_74";
                    if (InputManager.IsKeyDown(Keys.D6)) music = "n3-KtO";
                    if (InputManager.IsKeyDown(Keys.D7)) music = "n3-LI";
                    if (InputManager.IsKeyDown(Keys.D8)) music = "本名_1_small";

                    SoundManager.CenterFrequency = 150f;
                    SoundManager.StartLoop(music);
                    pitch = 0.7f;
                    SoundManager.SetPitch(music, pitch);
                }
            } else if (InputManager.IsKeyPressed(Keys.Enter)) {
                SoundManager.StopLoop(music);
                ScreenManager.ToEditorScreen(this);
            }

            foreach (var world in LoadedWorlds) {
                world.Update();
            }

            foreach (var entity in Entities) {
                entity.Update();
            }

            Camera.UpdateTransformation();
        }

        public override void Draw() {
            RenderTarget2D sceneRender = RenderScene();
            RenderTarget2D bloomExtractRender = ExtractBloom(sceneRender);
            RenderTarget2D bloomBlurRender = BlurBloom(bloomExtractRender);

            GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);
            BloomCombineEffect.CurrentTechnique.Passes[0].Apply();
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
            GameManager.SpriteBatch.Begin(0, BlendState.Additive, null, null, null, BloomCombineEffect);
            //GameManager.SpriteBatch.Draw(sceneRender, Vector2.Zero, Color.White);
            GameManager.SpriteBatch.Draw(bloomBlurRender, Vector2.Zero, Color.White);
            GameManager.SpriteBatch.End();

            sceneRender.Dispose();
            bloomExtractRender.Dispose();
            bloomBlurRender.Dispose();
        }

        private RenderTarget2D BlurBloom(RenderTarget2D bloomExtractRender) {
            RenderTarget2D gaussianBlurRenderHorizontal = new(GameManager.GraphicsDeviceManager.GraphicsDevice, GameManager.WindowSize.Width, GameManager.WindowSize.Height);
            GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(gaussianBlurRenderHorizontal);
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);

            SampleWeights.SetValue(MathsHelper.GetSampleWeights());
            SampleOffsets.SetValue(MathsHelper.GetSampleOffsets(0));
            GaussianBlurEffect.CurrentTechnique.Passes[0].Apply();

            GameManager.SpriteBatch.Begin(0, BlendState.Opaque, null, null, null, GaussianBlurEffect);
            GameManager.SpriteBatch.Draw(bloomExtractRender, Vector2.Zero, Color.White);
            GameManager.SpriteBatch.End();

            RenderTarget2D gaussianBlurRenderVertical = new(GameManager.GraphicsDeviceManager.GraphicsDevice, GameManager.WindowSize.Width, GameManager.WindowSize.Height);
            GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(gaussianBlurRenderVertical);
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);

            SampleWeights.SetValue(MathsHelper.GetSampleWeights());
            SampleOffsets.SetValue(MathsHelper.GetSampleOffsets(1));
            GaussianBlurEffect.CurrentTechnique.Passes[0].Apply();

            GameManager.SpriteBatch.Begin(0, BlendState.Opaque, null, null, null, GaussianBlurEffect);
            GameManager.SpriteBatch.Draw(gaussianBlurRenderHorizontal, Vector2.Zero, Color.White);
            GameManager.SpriteBatch.End();

            gaussianBlurRenderHorizontal.Dispose();
            return gaussianBlurRenderVertical;
        }

        private RenderTarget2D ExtractBloom(RenderTarget2D sceneRender) {
            RenderTarget2D bloomExtractRender = new(GameManager.GraphicsDeviceManager.GraphicsDevice, GameManager.WindowSize.Width, GameManager.WindowSize.Height);
            BloomExtractEffect.CurrentTechnique.Passes[0].Apply();
            
            GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(bloomExtractRender);
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);

            GameManager.SpriteBatch.Begin(0, BlendState.Opaque, null, null, null, BloomExtractEffect);
            GameManager.SpriteBatch.Draw(sceneRender, Vector2.Zero, Color.White);
            GameManager.SpriteBatch.End();

            return bloomExtractRender;
        }

        private RenderTarget2D RenderScene() {
            RenderTarget2D sceneRender = new(GameManager.GraphicsDeviceManager.GraphicsDevice, GameManager.WindowSize.Width, GameManager.WindowSize.Height);
            GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(sceneRender);
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);
            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);

            foreach (var world in LoadedWorlds) {
                foreach (var layer in world.Layers) {
                    layer.Draw(Camera);
                }
            }

            foreach (var entity in Entities) {
                entity.Draw();
            }

            GameManager.SpriteBatch.End();
            return sceneRender;
        }
    }
}