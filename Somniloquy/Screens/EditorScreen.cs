namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;

    public enum EditorState { PaintMode, TileMode, PropertiesMode }
    public enum EditorAction { PaintIdle, PaintRectangle, PaintLine, TileIdle, TileSelection, TileRectangle, TileLine, PropertiesIdle }

    /// <summary>
    /// EditorScreen is a screen for... well... editing the worlds you create!
    /// It has several states to allow easier world creations, and here are the list of the actions of each states:
    /// 
    /// Universal:
    /// - W, A, S, D: Move around the world, as in play mode
    /// - Space + Left Mouse Button: Move around the world, probably a more preferred way for artists of conventional art programs 
    /// - Q, E: Zoom in and out to get a better view of what you're drawing
    /// - Tab: Toggle PaintMode/TileMode
    /// - Hold Alt: Select the topmost layer beneath the mouse
    /// - +/-: Progress animation frames
    /// - Ctrl + Z/ Ctrl + Shift + Z: Undo/Redo
    /// 
    /// PaintMode:
    /// - [PaintCommand] Left Mouse Button: Draw pixels directly on tiles
    /// - Alt + Left Mouse Button: Get the uppermost color beneath the mouse
    /// - [PaintCommand] Shift + Left Mouse Button: Draw rectangles
    /// - [PaintCommand] Ctrl + Shift + Left Mouse Button: Draw Lines (either 0, pi/6, pi/4, pi/4, pi/2 (and inversed) angles)
    /// - [PaintCommand] Ctrl + Left Mouse Button: Draw lines (free angle)
    /// - Hold Z: When drawing, you erase instead
    /// - I, J, K, L: Move around the color palette
    /// - U, O: Change color palette's hue
    /// -  
    /// 
    /// TileMode:
    /// - [SetCommand] Left Mouse Button: Set tiles / tile patterns
    /// - Alt + Left Mouse Button: Get uppermost tile beneath the mouse
    /// - Alt + Shift + Left Mouse Button: Pick up tiles within specified rectangular boundary, get tile pattern
    /// - [SetCommand] Shift + Left Mouse Button: Draw rectangles of tiles / tile patterns
    /// - [SetCommand] Ctrl + Left Mouse Button: Draw lines of tiles (either 0, pi/6, pi/4, pi/4, pi/2 (and inversed) angles)
    /// - Hold Z: When setting tiles, you remove the tile from the position instead
    ///
    /// - PropertiesMode:
    /// - Ctrl + Left Mouse Button: Draw boundaries
    /// - Shift + Left Mouse Button: Draw rectangular boundaries (shortcut for above approach when creating simple rectangles)
    /// - Left Mouse Button: Select boundary
    /// - Alt + Left Mouse Button: Select all connecting boundaries of same type
    /// </summary>
    public class EditorScreen : Screen {
        public WorldScreen WorldScreen { get; set; }
        public ColorChart ColorChart { get; private set; } = null;

        public EditorState CurrentEditorState = EditorState.PaintMode;
        public EditorAction CurrentEditorAction = EditorAction.PaintIdle;
        
        public Color SelectedColor { get; set; } = Color.AliceBlue;        
        public ICommand ActiveCommand { get; set; } = null;
        public int SelectedAnimationFrame { get; set; } = 0;
        public bool Sync = false;

        public EditorScreen(Rectangle boundaries) : base(boundaries) {
            DividingDirection = Direction.Horizontal;
            var worldScreen = new WorldScreen(Utils.ResizeRectangle(boundaries, DividingDirection, 0.8f, 0), this);

            var rightContainerRectangle = Utils.ResizeRectangle(boundaries, DividingDirection, 0.2f, 0.8f);
            var rightContainer = new Screen(rightContainerRectangle);

            ChildScreens.Add(0, worldScreen);
            ChildScreens.Add(1, rightContainer);

            var spriteSheetScreen = new SpriteSheetScreen(Utils.ResizeRectangle(boundaries, Direction.Vertical, 0f, 0.8f));
            ColorChart = new ColorChart(Utils.ResizeRectangle(rightContainerRectangle, Direction.Vertical, 0.2f, 0.8f), this);
            ColorChart.UpdateChart();

            rightContainer.DividingDirection = Direction.Vertical;
            rightContainer.ChildScreens.Add(0, spriteSheetScreen);
            rightContainer.ChildScreens.Add(1, ColorChart);
        }

        public override void Update() {
            if (InputManager.IsKeyPressed(Keys.F1)) {
                CurrentEditorState = EditorState.PaintMode;
                CurrentEditorAction = EditorAction.PaintIdle;
            } else if (InputManager.IsKeyPressed(Keys.F2)) {
                CurrentEditorState = EditorState.TileMode;
                CurrentEditorAction = EditorAction.TileIdle;
            } else if (InputManager.IsKeyPressed(Keys.F3)) {
                CurrentEditorState = EditorState.PropertiesMode;
                CurrentEditorAction = EditorAction.PropertiesIdle;
            }

            if (InputManager.IsKeyPressed(Keys.Tab)) {
                Sync = !Sync;
            }

            if (InputManager.IsKeyPressed(Keys.Enter)) {
                ScreenManager.ToGameScreen(this);
            }

            base.Update();
        }
    }
}