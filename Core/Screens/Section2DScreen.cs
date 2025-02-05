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
        public Section2DPlayer Player;

        public Section2DScreen(Rectangle boundaries, Section2D section = null) : base(boundaries) {
            Section = section;

            if (Section is null) { // temp
                Section = new();
                Section.Root.AddLayer(new TextureLayer2D());
            }

            Section.Screen = this;

            Editor = new(this);
            Camera.MaxZoom = 32f;
            Camera.MinZoom = 1 / 4f;

            InputManager.RegisterKeybind(Keys.Enter, _ => TogglePlay(), TriggerOnce.True);
        }

        public override void LoadContent() {
            Camera.LoadContent();
            Editor.LoadContent();
        }
        
        public void TogglePlay() {
            if (Player is not null) {
                Player.UnloadContent();
                Player = null;
                Editor = new(this);
                Editor.SwitchEditorMode(new PaintMode(this, Editor));
            } else {
                if (ScreenManager.FocusedScreen != Editor) return; 
                Editor.UnloadContent();
                Editor = null;
                Player = new(this);
            }
        }

        public override void Update() {
            base.Update();
            Editor?.Update();
            Section?.Update();
            Player?.Update();
            Camera.Update();
        }

        public override void Draw() {
            Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);
            Editor?.Draw();
            Player?.Draw();
            // Camera.SB.End();
        }
    }
}