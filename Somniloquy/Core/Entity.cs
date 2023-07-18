namespace Somniloquy
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;
    using Newtonsoft.Json.Serialization;

    public class Entity {
        public FunctionalSprite FSprite { get; set; }
        public CircleF CollisionBounds { get; set; }
        public Vector2 Velocity { get; set; }
        public Layer CurrentLayer { get; set; }

        public virtual void Update() {
            // Resolve Collisions
            FSprite?.AdvanceFrames();
        }

        public Vector2 ResolveCollisions(Vector2 potentialPosition) {
            Point startTilePosition = CurrentLayer.GetTilePositionOf(MathsHelper.ToPoint(CollisionBounds.Center));
            Point endTilePosition = CurrentLayer.GetTilePositionOf(potentialPosition.ToPoint());

            var pair = MathsHelper.ValidizePoints(startTilePosition, endTilePosition);
            startTilePosition = pair.Item1 - new Point(1, 1);
            endTilePosition = pair.Item2 + new Point(1, 1);

            for (int y = startTilePosition.Y; y <= endTilePosition.Y; y++) {
                for (int x = startTilePosition.X; x <= endTilePosition.X; x++) {
                    
                    var tile = CurrentLayer.GetTile(new Point(x, y));
                    if (tile is null) continue;

                    // var tileBounds = new Rectangle(x * CurrentLayer.TileLength, y * CurrentLayer.TileLength, CurrentLayer.TileLength, CurrentLayer.TileLength);

                    // Vector2 nearestPoint = new(
                    //     MathF.Max(x * CurrentLayer.TileLength, MathF.Min(potentialPosition.X, (x + 1) * CurrentLayer.TileLength)),
                    //     MathF.Max(y * CurrentLayer.TileLength, MathF.Min(potentialPosition.Y, (y + 1) * CurrentLayer.TileLength))
                    // );

                    // Vector2 rayToNearest = nearestPoint - potentialPosition;
                    // float overlap = CollisionBounds.Radius - rayToNearest.Length();

                    // if (float.IsNaN(overlap)) overlap = 0;

                    // if (overlap > 0) {
                    //     potentialPosition -= Vector2.Normalize(rayToNearest) * overlap;
                    // }

                    var vertices = tile.CollisionVertices;
                    if (vertices is null) continue;

                    for (var i = 0; i < vertices.Length; i++) {
                        var v1 = vertices[i].ToVector2() + new Vector2(x, y) * CurrentLayer.TileLength;
                        var v2 = vertices[(i + 1) % vertices.Length].ToVector2() + new Vector2(x, y) * CurrentLayer.TileLength;
                        if (!MathsHelper.Intersects((v1, v2), CollisionBounds)) continue;

                        Vector2 closestPointOnEdge = MathsHelper.GetClosestPointOnLine((v1, v2), potentialPosition);
                        float distanceToEdge = (closestPointOnEdge - potentialPosition).Length();

                        if (distanceToEdge < CollisionBounds.Radius && distanceToEdge > 0) {
                            var collisionDirection = Vector2.Normalize(potentialPosition - closestPointOnEdge);
                            var overlap = CollisionBounds.Radius - distanceToEdge;
                            potentialPosition += collisionDirection * overlap;
                        }
                    }

                    Console.WriteLine();
                }
            }

            return potentialPosition;
        }

        public virtual void Draw() {
            if (FSprite is not null) {
                GameManager.DrawFunctionalSprite(FSprite, FSprite.GetDestinationRectangle(MathsHelper.ToPoint(CollisionBounds.Center)), null);
            } else {
                GameManager.SpriteBatch.DrawCircle(CollisionBounds, 32, Color.White);
            }
        }
    }
    
    public class Affect {
        public enum Emotions { Anticipation, Joy, Trust, Fear, Surprise, Sadness, Disgust, Anger}
        public enum PrimaryDyads { Love, Submission, Alarm, Disappointment, Remorse, Contempt, Aggression, Optimism}
        public enum SecondaryDyads { Guilt, Curiosity, Despair, SurpriseDisgust, Envy, Cynism, Pride, Fatalism }
        public enum TertiaryDyads { Delight, Sentimentality, Shame, Outrage, Pessimism, Morbidness, Dominance, Anxiety }

        public Dictionary<Emotions, float> EmotionIntensities = new();
        public Dictionary<PrimaryDyads, float> PrimaryDyadIntensities = new();
        public Dictionary<TertiaryDyads, float> TertiaryDyadIntensities = new();

        public void Update() {
            // Suppress conflicting emotions

        }
    }

    public class Player : Entity {
        public Camera Camera { get; set; }

        public float Controllability { get; set; } = 1.0f;
        public float Vividness { get; set; } = 1.0f;
        public Affect Emotion { get; set; }

        // public Dictionary<Keys, Action>

        public override void Update() {
            Velocity = Vector2.Zero;
            float speed = 1f;

            if (Controllability >= 0.5f) {
                if (InputManager.IsKeyDown(Keys.W)) {
                    Velocity += new Vector2(0, -speed);
                } if (InputManager.IsKeyDown(Keys.A)) {
                    Velocity += new Vector2(-speed, 0);
                } if (InputManager.IsKeyDown(Keys.S)) {
                    Velocity += new Vector2(0, speed);
                } if (InputManager.IsKeyDown(Keys.D)) {
                    Velocity += new Vector2(speed, 0);
                }
            }

            if (Velocity.Length() > speed) {
                Velocity = Vector2.Normalize(Velocity) * speed;
            }

            Vector2 potentialPosition = CollisionBounds.Position + Velocity;
            CollisionBounds = new CircleF(ResolveCollisions(potentialPosition), CollisionBounds.Radius);

            Camera.Position = CollisionBounds.Position;

            base.Update();
        }
    }
}