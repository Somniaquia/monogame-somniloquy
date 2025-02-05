namespace Somniloquy {
    using System;
    using Microsoft.Xna.Framework;

    public struct CircleF {
        public Vector2 Center;
        public float Radius;
        
        public CircleF(Vector2 center, float radius) {
            Center = center; Radius = radius;
        }

        public bool IntersectsLine(Vector2 v1, Vector2 v2) {
            // https://stackoverflow.com/questions/1073336/circle-line-segment-collision-detection-algorithm
            // aaaa TODO Understand
            Vector2 d = v2 - v1;
            Vector2 f = v1 - Center;
            float a = Vector2.Dot(d, d);
            float b = Vector2.Dot(2 * f, d) ;
            float c = Vector2.Dot(f, f) - Radius * Radius;

            float discriminant = b*b-4*a*c;
            if( discriminant < 0 ) {
                return false; // no intersection
            } else {
                // ray didn't totally miss sphere,
                // so there is a solution to
                // the equation.
                
                discriminant = MathF.Sqrt( discriminant );

                // either solution may be on or off the ray so need to test both
                // t1 is always the smaller value, because BOTH discriminant and
                // a are nonnegative.
                float t1 = (-b - discriminant)/(2*a);
                float t2 = (-b + discriminant)/(2*a);

                // 3x HIT cases:
                //          -o->             --|-->  |            |  --|->
                // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

                // 3x MISS cases:
                //       ->  o                     o ->              | -> |
                // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)
                
                if( t1 >= 0 && t1 <= 1 ) {
                    // t1 is the intersection, and it's closer than t2
                    // (since t1 uses -b - discriminant)
                    // Impale, Poke
                    return true ;
                }

                // here t1 didn't intersect so we are either started
                // inside the sphere or completely past it
                if( t2 >= 0 && t2 <= 1 ) {
                    // ExitWound
                    return true ;
                }
                
                return false ; // no intn: FallShort, Past, CompletelyInside
            }
        }
    } 
}