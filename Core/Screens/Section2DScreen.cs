namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Graphics;
    using System.Linq;

    public class Section2DScreen : BoxUI {
        public Section2D Section;
        public Camera2D Camera = new();
        public Section2DEditor Editor;

        public Section2DScreen(Rectangle boundaries, Section2D section = null) : base(boundaries) {
            Section = section;

            if (Section is null) { // temp
                Section = new();
                Section.Root.AddLayer(new TileLayer2D(16, 16));
                Section.Root.AddLayer(new TextureLayer2D());
            }

            Section.Screen = this;

            Editor = new(this);
            Camera.MaxZoom = 16f;
            Camera.MinZoom = 1 / 4f;
        }

        public override void LoadContent() {
            Camera.LoadContent();
            Editor.LoadContent();
        }

        public override void Update() {
            base.Update();
            Camera.Update();
            Editor?.Update();
            Section?.Update();
        }

        public override void Draw() {
            Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);
            Editor?.Draw();
            // Camera.SB.End();
        }
    }
}