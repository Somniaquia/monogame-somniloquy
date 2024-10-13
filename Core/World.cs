namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// A World stores multiple Sections, where a Section corresponds to a continuous area that can be traveled without screen transitions,
    /// basically a World is a container of Sections with similar themes that would be better be grouped in sake of wiki-ing the game.
    /// </summary>
    public class World {
        public Dictionary<string, Section2D> Sections; // TODO: 3D Sections
    }
}