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

        //                 var vertices = tile.CollisionVertices;
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
            // if (previous == after) return (after, false);

            var layers = CurrentLayer.GetSelfAndChildren();
            var collisionBounds = new List<(Vector2, Vector2)>();
            
            foreach (var layer in layers) {
                if (layer is TileLayer2D tileLayer) {
                    Vector2I startTilePosition = tileLayer.GetTilePosition((Vector2I)after) - new Vector2I(2, 2);
                    Vector2I endTilePosition = tileLayer.GetTilePosition((Vector2I)after) + new Vector2I(2, 2);

                    for (int y = startTilePosition.Y; y <= endTilePosition.Y; y++) {
                        for (int x = startTilePosition.X; x <= endTilePosition.X; x++) {
                            var tile = tileLayer.GetTile(new Vector2I(x, y));
                            if (tile is null) continue;

                            var vertices = tile.CollisionVertices;
                            if (vertices is null) continue;

                            for (var i = 0; i < vertices.Count; i++) {
                                var v1 = vertices[i] + new Vector2(x, y) * tileLayer.TileLength;
                                var v2 = vertices[(i + 1) % vertices.Count] + new Vector2(x, y) * tileLayer.TileLength;
                                if (new CircleF(after, CollisionBounds.Radius).IntersectsLine(v1, v2)) 
                                    collisionBounds.Add((v1, v2));
                            }
                        }
                    }
                } else if (layer is TextureLayer2D textureLayer) {
                    Vector2I startChunkPosition = textureLayer.GetChunkPosition((Vector2I)after) - Vector2I.One; // TODO: Optimize
                    Vector2I endChunkPosition = textureLayer.GetChunkPosition((Vector2I)after) + Vector2I.One;

                    for (int y = startChunkPosition.Y; y <= endChunkPosition.Y; y++) {
                        for (int x = startChunkPosition.X; x <= endChunkPosition.X; x++) {
                            if (!textureLayer.Chunks.ContainsKey(new Vector2I(x, y))) continue;
                            var chunk = textureLayer.Chunks[new Vector2I(x, y)];
                            if (chunk is null) continue;

                            var vertices = chunk.CollisionVertices;
                            if (vertices is null) continue;

                            for (var i = 0; i < vertices.Count; i++) {
                                var v1 = vertices[i] + new Vector2(x, y) * textureLayer.ChunkLength;
                                var v2 = vertices[(i + 1) % vertices.Count] + new Vector2(x, y) * textureLayer.ChunkLength;
                                if (new CircleF(after, CollisionBounds.Radius).IntersectsLine(v1, v2)) 
                                    collisionBounds.Add((v1, v2));
                            }
                        }
                    }
                }
            }

            bool repositioned = false;
            // priority: perpendicular bounds first
            foreach(var bound in collisionBounds.OrderBy(bound => MathF.Abs(Vector2.Dot(after - previous, bound.Item2 - bound.Item1)))) {
                Vector2 closestVector2IOnEdge = Util.GetClosestVector2IOnLine((bound.Item1, bound.Item2), after);
                float distanceToEdge = (closestVector2IOnEdge - after).Length();

                if (distanceToEdge < CollisionBounds.Radius && distanceToEdge > 0) {
                    var collisionDirection = Vector2.Normalize(after - closestVector2IOnEdge);
                    var overlap = CollisionBounds.Radius - distanceToEdge;
                    after += collisionDirection * overlap;
                    repositioned = true;
                }
            }
            
            return (after, repositioned);
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

            Vector2 potentialPosition = CollisionBounds.Center + Velocity;

            // while (true) {
                var newPos = ResolveCollisions(CollisionBounds.Center, potentialPosition);
                CollisionBounds = new CircleF(newPos.Item1, CollisionBounds.Radius);
            //     if (!newPos.Item2) break;
            // }

            Camera.TargetCenterPosInWorld = CollisionBounds.Center;
            Camera.CenterPosInWorld = CollisionBounds.Center;

            base.Update();
        }
    }
}