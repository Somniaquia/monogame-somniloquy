namespace Somniloquy
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;

    public class Entity {
        public FunctionalSprite FSprite { get; private set; }
        public Vector2 Position { get; private set; }
        public CircleF CollisionBounds { get; private set; }

        public virtual void Update() {
            // Resolve Collisions
            FSprite.AdvanceFrames();
        }

        public virtual void Draw() {
            GameManager.DrawFunctionalSprite(FSprite, FSprite.GetDestinationRectangle(MathsHelper.ToPoint(Position)), null);
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
        public float Controllability { get; set; } = 1.0f;
        public float Vividness { get; set; } = 1.0f;
        public Affect Emotion { get; set; }

        // public Dictionary<Keys, Action>

        public override void Update() {
            if (Controllability >= 0.5f) {

            }
            base.Update();
        }
    }
}