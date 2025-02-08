namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class Section2DPlayer : BoxUI {
        public Section2DScreen Screen;
        public Camera2D Camera = new();
        public Player Player;

        public Section2DPlayer(Section2DScreen screen) : base() {
            Screen = screen;
            Player = new(Camera);
        }

        public override void LoadContent() {
            Camera.LoadContent();
            Camera.TargetZoom = 8f;
        }

        public override void UnloadContent() {
            base.UnloadContent();
        }

        public override void Update() {
            base.Update();
            Player.Update();
            Camera.Update();
        }

        public override void Draw() {
            base.Draw();
            Screen.Section.Draw(Camera);
            Player.Draw(Camera);
            Camera.SB.End();
        }
    }
}