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

        private Rectangle CalculateTrajectoryBounds(Vector2 startPoint, Vector2 endPoint, float radius) {
            float minX = Math.Min(startPoint.X, endPoint.X) - radius;
            float minY = Math.Min(startPoint.Y, endPoint.Y) - radius;
            float maxX = Math.Max(startPoint.X, endPoint.X) + radius;
            float maxY = Math.Max(startPoint.Y, endPoint.Y) + radius;

            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }

        public Vector2 ResolveCollisions(Vector2 potentialPosition) {
            Point startTilePosition = CurrentLayer.GetTilePositionOf(MathsHelper.ToPoint(CollisionBounds.Center));
            Point endTilePosition = CurrentLayer.GetTilePositionOf(potentialPosition.ToPoint());

            var pair = MathsHelper.ValidizePoints(startTilePosition, endTilePosition);
            startTilePosition = pair.Item1 - new Point(1, 1);
            endTilePosition = pair.Item2 + new Point(1, 1);

            var trajectoryBounds = CalculateTrajectoryBounds(CollisionBounds.Center, potentialPosition, CollisionBounds.Radius);

            for (int y = startTilePosition.Y; y <= endTilePosition.Y; y++) {
                for (int x = startTilePosition.X; x <= endTilePosition.X; x++) {
                    var tile = CurrentLayer.GetTile(new Point(x, y));

                    if (tile is not null) {
                        var tileBounds = new Rectangle(x * CurrentLayer.TileLength, y * CurrentLayer.TileLength, CurrentLayer.TileLength, CurrentLayer.TileLength);

                        Vector2 nearestPoint = new(
                            MathF.Max(x * CurrentLayer.TileLength, MathF.Min(potentialPosition.X, (x + 1) * CurrentLayer.TileLength)),
                            MathF.Max(y * CurrentLayer.TileLength, MathF.Min(potentialPosition.Y, (y + 1) * CurrentLayer.TileLength))
                        );

                        Vector2 rayToNearest = nearestPoint - potentialPosition;
                        float overlap = CollisionBounds.Radius - rayToNearest.Length();

                        if (float.IsNaN(overlap)) overlap = 0;

                        if (overlap > 0) {
                            potentialPosition -= Vector2.Normalize(rayToNearest) * overlap;
                        }

                        if (MathsHelper.IntersectsOrAdjacent(trajectoryBounds, tileBounds)) {

                            var vertices = tile.CollisionVertices;

                            // if (vertices is not null) {
                            //     for (var i = 0; i < vertices.Length; i++) {
                            //         var v1 = vertices[i].ToVector2() + new Vector2(x, y) * CurrentLayer.TileLength;
                            //         var v2 = vertices[(i + 1) % vertices.Length].ToVector2() + new Vector2(x, y) * CurrentLayer.TileLength;

                            //         Vector2 edgeVector = Vector2.Normalize(v2 - v1);
                            //         Vector2 circleToV1Vector = CollisionBounds.Center - v1;

                            //         float cosTheta = Vector2.Dot(edgeVector, circleToV1Vector) / (edgeVector.Length() * circleToV1Vector.Length());
                            //         float distanceToEdge = circleToV1Vector.Length() * MathF.Sqrt(1 - cosTheta * cosTheta);

                            //         if (distanceToEdge < CollisionBounds.Radius && distanceToEdge > 0) {
                            //             var closestPointOnEdge = v1 + Vector2.Dot(circleToV1Vector, edgeVector) * edgeVector;
                            //             var collisionDirection = Vector2.Normalize(CollisionBounds.Center - closestPointOnEdge);
                            //             var overlap = CollisionBounds.Radius - distanceToEdge;
                            //             potentialPosition += collisionDirection * overlap;
                            //             Console.Write($"{CollisionBounds.Center}, {potentialPosition}, {overlap} ");
                            //         }
                            //     } 
                            // }
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

            if (Controllability >= 0.5f) {
                if (InputManager.IsKeyDown(Keys.W)) {
                    Velocity += new Vector2(0, -1f);
                } if (InputManager.IsKeyDown(Keys.A)) {
                    Velocity += new Vector2(-1f, 0);
                } if (InputManager.IsKeyDown(Keys.S)) {
                    Velocity += new Vector2(0, 1f);
                } if (InputManager.IsKeyDown(Keys.D)) {
                    Velocity += new Vector2(1f, 0);
                }
            }

            if (Velocity.Length() > 1f) {
                Velocity = Vector2.Normalize(Velocity) * 1f;
            }

            Vector2 potentialPosition = CollisionBounds.Position + Velocity;
            CollisionBounds = new CircleF(ResolveCollisions(potentialPosition), CollisionBounds.Radius);

            Camera.Position = CollisionBounds.Position;

            base.Update();
        }
    }
}