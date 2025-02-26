namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    

    public class Entity {
        public CircleF CollisionBounds { get; set; }
        public Vector2 Velocity { get; set; }
        public Layer2D CurrentLayer { get; set; }

        public virtual void Update() {
        }

        // public Vector2 ResolveCollisionsLegacy(Vector2 potentialPosition) {
        //     if (CurrentLayer is TileLayer2D layer) {
        //         Vector2I startTilePosition = layer.GetTilePosition((Vector2I)CollisionBounds.Center);
        //         Vector2I endTilePosition = layer.GetTilePosition((Vector2I)potentialPosition);

        //         var pair = Vector2Extensions.Rationalize(startTilePosition, endTilePosition);
        //         startTilePosition = pair.Item1 - new Vector2I(1, 1);
        //         endTilePosition = pair.Item2 + new Vector2I(1, 1);

        //         for (int y = startTilePosition.Y; y <= endTilePosition.Y; y++) {
        //             for (int x = startTilePosition.X; x <= endTilePosition.X; x++) {
                        
        //                 var tile = layer.GetTile(new Vector2I(x, y));
        //                 if (tile is null) continue;

        //                 var vertices = tile.CollisionEdges;
        //                 if (vertices is null) continue;

        //                 for (var i = 0; i < vertices.Count; i++) {
        //                     var v1 = vertices[i] + new Vector2(x, y) * layer.TileLength;
        //                     var v2 = vertices[(i + 1) % vertices.Count] + new Vector2(x, y) * layer.TileLength;
        //                     if (!CollisionBounds.IntersectsLine(v1, v2)) continue;

        //                     Vector2 closestVector2IOnEdge = Util.GetClosestVector2IOnLine((v1, v2), potentialPosition);
        //                     float distanceToEdge = (closestVector2IOnEdge - potentialPosition).Length();

        //                     if (distanceToEdge < CollisionBounds.Radius && distanceToEdge > 0) {
        //                         var collisionDirection = Vector2.Normalize(potentialPosition - closestVector2IOnEdge);
        //                         var overlap = CollisionBounds.Radius - distanceToEdge;
        //                         potentialPosition += collisionDirection * overlap;
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }

        public (Vector2, bool) ResolveCollisions(Vector2 previous, Vector2 after) {
            // Early exit if no movement
            if (previous == after) return (after, false);

            var layers = CurrentLayer.GetSelfAndChildren();
            var collisionEdges = new List<(Vector2, Vector2)>();
            float radius = CollisionBounds.Radius;
            const int MAX_ITERATIONS = 3;

            foreach (var layer in layers) {
                if (layer is TileLayer2D tileLayer) {
                    // Dynamic tile expansion based on radius
                    int tileExpansion = (int)Math.Ceiling(radius / tileLayer.TileLength) + 1;
                    Vector2I centerTile = tileLayer.GetTilePosition((Vector2I)after);
                    Vector2I startTile = centerTile - new Vector2I(tileExpansion, tileExpansion);
                    Vector2I endTile = centerTile + new Vector2I(tileExpansion, tileExpansion);

                    for (int y = startTile.Y; y <= endTile.Y; y++) {
                        for (int x = startTile.X; x <= endTile.X; x++) {
                            var tile = tileLayer.GetTile(new Vector2I(x, y));
                            if (tile?.CollisionEdges is null) continue;

                            Vector2 tilePos = new Vector2(x, y) * tileLayer.TileLength;
                            foreach (var (v1, v2) in tile.CollisionEdges) {
                                Vector2 worldV1 = v1 + tilePos;
                                Vector2 worldV2 = v2 + tilePos;
                                if (CircleLineIntersection(worldV1, worldV2, after, radius))
                                    collisionEdges.Add((worldV1, worldV2));
                            }
                        }
                    }
                }
                else if (layer is TextureLayer2D textureLayer) {
                    // Dynamic chunk expansion based on radius
                    int chunkExpansion = (int)Math.Ceiling(radius / textureLayer.ChunkLength) + 1;
                    Vector2I centerChunk = textureLayer.GetChunkPosition((Vector2I)after);
                    Vector2I startChunk = centerChunk - new Vector2I(chunkExpansion, chunkExpansion);
                    Vector2I endChunk = centerChunk + new Vector2I(chunkExpansion, chunkExpansion);

                    for (int y = startChunk.Y; y <= endChunk.Y; y++) {
                        for (int x = startChunk.X; x <= endChunk.X; x++) {
                            if (!textureLayer.Chunks.TryGetValue(new Vector2I(x, y), out var chunk) || 
                                chunk?.CollisionEdges is null) continue;

                            Vector2 chunkPos = new Vector2(x, y) * textureLayer.ChunkLength;
                            foreach (var (v1, v2) in chunk.CollisionEdges) {
                                Vector2 worldV1 = v1 + chunkPos;
                                Vector2 worldV2 = v2 + chunkPos;
                                if (CircleLineIntersection(worldV1, worldV2, after, radius))
                                    collisionEdges.Add((worldV1, worldV2));
                            }
                        }
                    }
                }
            }

            // Early exit if no collision edges
            if (collisionEdges.Count == 0) return (after, false);

            bool repositioned;
            int iterations = 0;
            const float EPSILON = 0.001f;
            
            do {
                repositioned = false;
                var orderedEdges = collisionEdges.OrderBy(e => 
                    MathF.Abs(Vector2.Dot(after - previous, e.Item2 - e.Item1)));

                foreach (var edge in orderedEdges) {
                    Vector2 closest = GetClosestPointOnLineSegment(edge.Item1, edge.Item2, after);
                    Vector2 delta = after - closest;
                    float distance = delta.Length();
                    float safeDistance = distance - EPSILON;

                    if (safeDistance < radius && distance > EPSILON) {
                        Vector2 direction = delta / distance;
                        float overlap = radius - safeDistance;
                        after += direction * overlap;
                        repositioned = true;
                    }
                }

                iterations++;
            } while (repositioned && iterations < MAX_ITERATIONS);

            return (after, repositioned);
        }

        // Helper method for circle-line intersection check
        private bool CircleLineIntersection(Vector2 lineStart, Vector2 lineEnd, Vector2 center, float radius) {
            Vector2 closest = GetClosestPointOnLineSegment(lineStart, lineEnd, center);
            return (closest - center).LengthSquared() <= radius * radius;
        }

        // Accurate closest point on line segment calculation
        private Vector2 GetClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 p) {
            Vector2 ap = p - a;
            Vector2 ab = b - a;
            float t = Vector2.Dot(ap, ab) / ab.LengthSquared();
            t = Math.Clamp(t, 0, 1);
            return a + t * ab;
        }

        public virtual void Draw(Camera2D camera) {
            //if (FSprite is not null) {
            //    Somniloquy.DrawFunctionalSprite(FSprite, FSprite.GetDestinationRectangle(MathsHelper.ToVector2I(CollisionBounds.Center)), null);
            //} else {
            camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            int radius = (int)(CollisionBounds.Radius * camera.Zoom);
            camera.SB.DrawCircle((Vector2I)camera.ToScreenPos(CollisionBounds.Center), radius, Color.DeepPink, false);
            //}
        }
    }
    
    public class Affect {
        public enum Emotions { Anticipation, Joy, Trust, Fear, Surprise, Sadness, Disgust, Anger}
        public enum PrimaryDyads { Love, Submission, Alarm, DisapVector2Iment, Remorse, Contempt, Aggression, Optimism}
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
        public Camera2D Camera { get; set; }

        public float Controllability { get; set; } = 1.0f;
        public float Vividness { get; set; } = 1.0f;
        public Affect Emotion { get; set; }

        // public Dictionary<Keys, Action>

        public Player(Camera2D camera) {
            Camera = camera;
            CollisionBounds = new(Vector2.Zero, 8);
            var screen = ScreenManager.GetFirstOfType<Section2DScreen>();
            CurrentLayer = screen?.Section?.Root;
        }

        public override void Update() {
            var acceleration = Vector2.Zero;
            float acc = 0.1f;

            if (Controllability >= 0.5f) {
                if (InputManager.IsKeyDown(Keys.W)) {
                    acceleration += new Vector2(0, -acc);
                } if (InputManager.IsKeyDown(Keys.A)) {
                    acceleration += new Vector2(-acc, 0);
                } if (InputManager.IsKeyDown(Keys.S)) {
                    acceleration += new Vector2(0, acc);
                } if (InputManager.IsKeyDown(Keys.D)) {
                    acceleration += new Vector2(acc, 0);
                }
            }

            if (acceleration.Length() > acc) {
                acceleration = Vector2.Normalize(acceleration) * acc;
            }

            Velocity += acceleration;
            if (Velocity.Length() > 1f) {
                Velocity = Vector2.Normalize(Velocity);
            }

            if (acceleration.X == 0) {
                if (Velocity.X < 0) Velocity = new Vector2(Util.Min(Velocity.X + acc, 0), Velocity.Y); 
                if (Velocity.X > 0) Velocity = new Vector2(Util.Max(Velocity.X - acc, 0), Velocity.Y); 
            }
            if (acceleration.Y == 0) {
                if (Velocity.Y < 0) Velocity = new Vector2(Velocity.X, Util.Min(Velocity.Y + acc, 0)); 
                if (Velocity.Y > 0) Velocity = new Vector2(Velocity.X, Util.Max(Velocity.Y - acc, 0)); 
            }

            Vector2 potentialPosition = CollisionBounds.Center + Velocity;

            // while (true) {
                var newPos = ResolveCollisions(CollisionBounds.Center, potentialPosition);
                CollisionBounds = new CircleF(newPos.Item1, CollisionBounds.Radius);
            //     if (!newPos.Item2) break;
            // }
            // if (newPos.Item2) Velocity = Vector2.Zero;

            Camera.TargetCenterPosInWorld = CollisionBounds.Center;
            Camera.CenterPosInWorld = CollisionBounds.Center;

            base.Update();
        }
    }
}