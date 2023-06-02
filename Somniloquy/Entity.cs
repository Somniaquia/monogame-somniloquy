namespace Somniloquy
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    public abstract class CollisionBounds {
        public CollisionBounds ChildCollisionBounds { get; set; }
    }

    public class RectangleCollisionBounds : CollisionBounds {
        public Rectangle Rectangle { get; set; }
    }

    public class CircularCollisionBounds : CollisionBounds {
        public Vector2 Center { get; set; }
        public float Radius { get; set; }
    }

    public class Entity {
        public string Name { get; set; }
        public FunctionalSprite FSprite { get; set; }
        public CollisionBounds CollisionBounds { get; set; }

        public virtual void Update() {
            // Resolve Collisions
            FSprite.AdvanceFrames();
        }

        public virtual void Draw() {
            var boundaries = new Rectangle();
            ResourceManager.DrawFunctionalSprite(FSprite, boundaries, null);
        }
    }

    public class Player : Entity {
        public bool Controllable { get; set; } = true;

        // public Dictionary<Keys, Action>

        public override void Update()
        {
            if (Controllable) {

            }
            base.Update();
        }
    }
}