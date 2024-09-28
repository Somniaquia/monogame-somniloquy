namespace Somniloquy {
    using Microsoft.Xna.Framework;
    
    public class Section2DScreen : Screen {
        public Camera2D Camera;
        public Section2DEditor Editor;

        public Section2DScreen(Rectangle boundaries) : base(boundaries) {
        }
    }

    public class Section2DEditor {
        public Section2D Section;
        public Layer2D SelectedLayer;
    }
}