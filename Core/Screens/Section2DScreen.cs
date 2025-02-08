namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Graphics;
    using System.Linq;

    public class Section2DScreen : BoxUI {
        public Section2D Section;
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

            InputManager.RegisterKeybind(Keys.Enter, _ => TogglePlay(), TriggerOnce.True);
        }

        public override void LoadContent() {
            Editor.LoadContent();
        }
        
        public void TogglePlay() {
            if (Player is not null) {
                Player.UnloadContent();
                Player = null;
                Editor = new(this);
                Editor.SwitchEditorMode(new PaintMode(this, Editor));
                Editor.LoadContent();
            } else {
                if (ScreenManager.FocusedScreen != Editor) return; 
                Editor.UnloadContent();
                Editor = null;
                Player = new(this);
                Player.LoadContent();
            }
        }

        public override void Update() {
            base.Update();
            Editor?.Update();
            Section?.Update();
            Player?.Update();
        }

        public override void Draw() {
            Editor?.Draw();
            Player?.Draw();
            // Camera.SB.End();
        }
    }
}