namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class Section2DPlayer : BoxUI {
        public Section2DScreen Screen;
        public Player Player;

        public Section2DPlayer(Section2DScreen screen) : base() {
            Screen = screen;
            Player = new(Screen.Camera);
            Screen.Camera.TargetZoom = 8f;
        }

        public override void LoadContent() {
            
        }

        public override void UnloadContent() {
            base.UnloadContent();
        }

        public override void Update() {
            base.Update();
            Player.Update();
        }

        public override void Draw() {
            base.Draw();
            Screen.Section.Draw(Screen.Camera);
            Player.Draw(Screen.Camera);
            Screen.Camera.SB.End();
        }
    }
}