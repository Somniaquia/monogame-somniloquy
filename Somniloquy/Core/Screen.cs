namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public abstract class Screen {
        public abstract void Update();
        public abstract void Draw();
    }

    public class WorldScreen : Screen {
        public bool EditMode { get; private set; } = true;
        public Camera Camera { get; private set; } = new Camera();
        public World ActiveWorld { get; private set; } = new();
        private Texture2D TransitionSnipplet;

        public void ToggleEditMode() {
            EditMode = !EditMode;
        }

        public override void Update() {
            Camera.UpdateTransformation();
            if (EditMode) {
                Vector2 mousePosition = Camera.ApplyInvertTransform(InputManager.GetMousePosition());
                if (InputManager.IsLeftButtonDown()) {
                    if (ActiveWorld.Layers.Count == 0) ActiveWorld.Layers.Add(new Layer());
                    ActiveWorld.Layers[0].SetTile(ActiveWorld.Layers[0].GetTilePositionOf(mousePosition.ToPoint()), new Tile());
                }
                
            } else {

            }
            ActiveWorld?.Update();
        }

        public override void Draw() {
            ResourceManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, transformMatrix: Camera.Transform);
            ActiveWorld?.Draw();
            ResourceManager.SpriteBatch.End();
        }
    }
}