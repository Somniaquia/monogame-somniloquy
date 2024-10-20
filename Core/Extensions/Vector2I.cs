// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using Microsoft.Xna.Framework;

namespace Somniloquy {
    /// <summary>
    /// Describes a 2D-vector.
    /// </summary>
#if XNADESIGNPROVIDED
    [System.ComponentModel.TypeConverter(typeof(Microsoft.Xna.Framework.Design.Vector2TypeConverter))]
#endif
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Vector2I : IEquatable<Vector2I>
    {
        #region Private Fields

        private static readonly Vector2I zeroVector = new Vector2I(0, 0);
        private static readonly Vector2I unitVector = new Vector2I(0, 0);
        private static readonly Vector2I unitXVector = new Vector2I(0, 0);
        private static readonly Vector2I unitYVector = new Vector2I(0, 0);

        #endregion

        #region Public Fields

        /// <summary>
        /// The x coordinate of this <see cref="Vector2I"/>.
        /// </summary>
        [DataMember]
        public int X;

        /// <summary>
        /// The y coordinate of this <see cref="Vector2I"/>.
        /// </summary>
        [DataMember]
        public int Y;

        #endregion

        #region Properties

        /// <summary>
        /// Returns a <see cref="Vector2I"/> with components 0, 0.
        /// </summary>
        public static Vector2I Zero
        {
            get { return zeroVector; }
        }

        /// <summary>
        /// Returns a <see cref="Vector2I"/> with components 1, 1.
        /// </summary>
        public static Vector2I One
        {
            get { return unitVector; }
        }

        /// <summary>
        /// Returns a <see cref="Vector2I"/> with components 1, 0.
        /// </summary>
        public static Vector2I UnitX
        {
            get { return unitXVector; }
        }

        /// <summary>
        /// Returns a <see cref="Vector2I"/> with components 0, 1.
        /// </summary>
        public static Vector2I UnitY
        {
            get { return unitYVector; }
        }

        #endregion

        #region Internal Properties

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    this.X.ToString(), "  ",
                    this.Y.ToString()
                );
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a 2d vector with X and Y from two values.
        /// </summary>
        /// <param name="x">The x coordinate in 2d-space.</param>
        /// <param name="y">The y coordinate in 2d-space.</param>
        public Vector2I(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Constructs a 2d vector with X and Y set to the same value.
        /// </summary>
        /// <param name="value">The x and y coordinates in 2d-space.</param>
        public Vector2I(int value)
        {
            this.X = value;
            this.Y = value;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Inverts values in the specified <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="value">Source <see cref="Vector2I"/> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Vector2I operator -(Vector2I value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="Vector2I"/> on the right of the add sign.</param>
        /// <returns>Sum of the vectors.</returns>
        public static Vector2I operator +(Vector2I value1, Vector2I value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        /// <summary>
        /// Subtracts a <see cref="Vector2I"/> from a <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="Vector2I"/> on the right of the sub sign.</param>
        /// <returns>Result of the vector subtraction.</returns>
        public static Vector2I operator -(Vector2I value1, Vector2I value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of two vectors by each other.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/> on the left of the mul sign.</param>
        /// <param name="value2">Source <see cref="Vector2I"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication.</returns>
        public static Vector2I operator *(Vector2I value1, Vector2I value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Vector2I"/> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector2I operator *(Vector2I value, int scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
        /// <param name="value">Source <see cref="Vector2I"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector2I operator *(int scaleFactor, Vector2I value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector2I"/> by the components of another <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/> on the left of the div sign.</param>
        /// <param name="value2">Divisor <see cref="Vector2I"/> on the right of the div sign.</param>
        /// <returns>The result of dividing the vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2I operator /(Vector2I value1, Vector2I value2)
        {
            value1.X = (int)MathF.Floor(value1.X / (float)value2.X);
            value1.Y = (int)MathF.Floor(value1.Y / (float)value2.Y);
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector2I"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/> on the left of the div sign.</param>
        /// <param name="divider">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2I operator /(Vector2I value1, int divider)
        {
            value1.X = (int)MathF.Floor(value1.X / (float)divider);
            value1.Y = (int)MathF.Floor(value1.Y / (float)divider);
            return value1;
        }

        /// <summary>
        /// Compares whether two <see cref="Vector2I"/> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector2I"/> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="Vector2I"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Vector2I value1, Vector2I value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y;
        }

        /// <summary>
        /// Compares whether two <see cref="Vector2I"/> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector2I"/> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="Vector2I"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(Vector2I value1, Vector2I value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs vector addition on <paramref name="value1"/> and <paramref name="value2"/>.
        /// </summary>
        /// <param name="value1">The first vector to add.</param>
        /// <param name="value2">The second vector to add.</param>
        /// <returns>The result of the vector addition.</returns>
        public static Vector2I Add(Vector2I value1, Vector2I value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        /// <summary>
        /// Performs vector addition on <paramref name="value1"/> and
        /// <paramref name="value2"/>, storing the result of the
        /// addition in <paramref name="result"/>.
        /// </summary>
        /// <param name="value1">The first vector to add.</param>
        /// <param name="value2">The second vector to add.</param>
        /// <param name="result">The result of the vector addition.</param>
        public static void Add(ref Vector2I value1, ref Vector2I value2, out Vector2I result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 2d-triangle.
        /// </summary>
        /// <param name="value1">The first vector of 2d-triangle.</param>
        /// <param name="value2">The second vector of 2d-triangle.</param>
        /// <param name="value3">The third vector of 2d-triangle.</param>
        /// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 2d-triangle.</param>
        /// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 2d-triangle.</param>
        /// <returns>The cartesian translation of barycentric coordinates.</returns>
        public static Vector2 Barycentric(Vector2I value1, Vector2I value2, Vector2I value3, int amount1, int amount2)
        {
            return new Vector2(
                MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2),
                MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 2d-triangle.
        /// </summary>
        /// <param name="value1">The first vector of 2d-triangle.</param>
        /// <param name="value2">The second vector of 2d-triangle.</param>
        /// <param name="value3">The third vector of 2d-triangle.</param>
        /// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 2d-triangle.</param>
        /// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 2d-triangle.</param>
        /// <param name="result">The cartesian translation of barycentric coordinates as an output parameter.</param>
        public static void Barycentric(ref Vector2I value1, ref Vector2I value2, ref Vector2I value3, int amount1, int amount2, out Vector2 result)
        {
            result.X = MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2);
            result.Y = MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains CatmullRom interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector in interpolation.</param>
        /// <param name="value2">The second vector in interpolation.</param>
        /// <param name="value3">The third vector in interpolation.</param>
        /// <param name="value4">The fourth vector in interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of CatmullRom interpolation.</returns>
        public static Vector2 CatmullRom(Vector2I value1, Vector2I value2, Vector2I value3, Vector2I value4, float amount)
        {
            return new Vector2(
                MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
                MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains CatmullRom interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector in interpolation.</param>
        /// <param name="value2">The second vector in interpolation.</param>
        /// <param name="value3">The third vector in interpolation.</param>
        /// <param name="value4">The fourth vector in interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <param name="result">The result of CatmullRom interpolation as an output parameter.</param>
        public static void CatmullRom(ref Vector2I value1, ref Vector2I value2, ref Vector2I value3, ref Vector2I value4, float amount, out Vector2 result)
        {
            result.X = MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount);
            result.Y = MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount);
        }

        /// <summary>
        /// Clamps the specified value within a range.
        /// </summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <returns>The clamped value.</returns>
        public static Vector2I Clamp(Vector2I value1, Vector2I min, Vector2I max)
        {
            return new Vector2I(
                MathHelper.Clamp(value1.X, min.X, max.X),
                MathHelper.Clamp(value1.Y, min.Y, max.Y));
        }

        /// <summary>
        /// Clamps the specified value within a range.
        /// </summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <param name="result">The clamped value as an output parameter.</param>
        public static void Clamp(ref Vector2I value1, ref Vector2I min, ref Vector2I max, out Vector2I result)
        {
            result.X = MathHelper.Clamp(value1.X, min.X, max.X);
            result.Y = MathHelper.Clamp(value1.Y, min.Y, max.Y);
        }

        /// <summary>
        /// Returns the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between two vectors.</returns>
        public static float Distance(Vector2I value1, Vector2I value2)
        {
            int v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
            return MathF.Sqrt((v1 * v1) + (v2 * v2));
        }

        /// <summary>
        /// Returns the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The distance between two vectors as an output parameter.</param>
        public static void Distance(ref Vector2I value1, ref Vector2I value2, out float result)
        {
            int v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
            result = MathF.Sqrt((v1 * v1) + (v2 * v2));
        }

        /// <summary>
        /// Returns the squared distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between two vectors.</returns>
        public static int DistanceSquared(Vector2I value1, Vector2I value2)
        {
            int v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
            return (v1 * v1) + (v2 * v2);
        }

        /// <summary>
        /// Returns the squared distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The squared distance between two vectors as an output parameter.</param>
        public static void DistanceSquared(ref Vector2I value1, ref Vector2I value2, out int result)
        {
            int v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
            result = (v1 * v1) + (v2 * v2);
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector2I"/> by the components of another <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="value2">Divisor <see cref="Vector2I"/>.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Vector2I Divide(Vector2I value1, Vector2I value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector2I"/> by the components of another <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="value2">Divisor <see cref="Vector2I"/>.</param>
        /// <param name="result">The result of dividing the vectors as an output parameter.</param>
        public static void Divide(ref Vector2I value1, ref Vector2I value2, out Vector2I result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector2I"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector2I Divide(Vector2I value1, int divider)
        {
            int factor = 1 / divider;
            value1.X *= factor;
            value1.Y *= factor;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector2I"/> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
        public static void Divide(ref Vector2I value1, int divider, out Vector2I result)
        {
            int factor = 1 / divider;
            result.X = value1.X * factor;
            result.Y = value1.Y * factor;
        }

        /// <summary>
        /// Returns a dot product of two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The dot product of two vectors.</returns>
        public static int Dot(Vector2I value1, Vector2I value2)
        {
            return (value1.X * value2.X) + (value1.Y * value2.Y);
        }

        /// <summary>
        /// Returns a dot product of two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The dot product of two vectors as an output parameter.</param>
        public static void Dot(ref Vector2I value1, ref Vector2I value2, out int result)
        {
            result = (value1.X * value2.X) + (value1.Y * value2.Y);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Vector2I)
            {
                return Equals((Vector2I)obj);
            }

            return false;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="other">The <see cref="Vector2I"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Vector2I other)
        {
            return (X == other.X) && (Y == other.Y);
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Vector2I"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains hermite spline interpolation.
        /// </summary>
        /// <param name="value1">The first position vector.</param>
        /// <param name="tangent1">The first tangent vector.</param>
        /// <param name="value2">The second position vector.</param>
        /// <param name="tangent2">The second tangent vector.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The hermite spline interpolation vector.</returns>
        public static Vector2 Hermite(Vector2I value1, Vector2I tangent1, Vector2I value2, Vector2I tangent2, float amount)
        {
            return new Vector2(MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount), MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains hermite spline interpolation.
        /// </summary>
        /// <param name="value1">The first position vector.</param>
        /// <param name="tangent1">The first tangent vector.</param>
        /// <param name="value2">The second position vector.</param>
        /// <param name="tangent2">The second tangent vector.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <param name="result">The hermite spline interpolation vector as an output parameter.</param>
        public static void Hermite(ref Vector2I value1, ref Vector2I tangent1, ref Vector2I value2, ref Vector2I tangent2, float amount, out Vector2 result)
        {
            result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
            result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
        }

        /// <summary>
        /// Returns the length of this <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>The length of this <see cref="Vector2I"/>.</returns>
        public float Length()
        {
            return MathF.Sqrt((X * X) + (Y * Y));
        }

        /// <summary>
        /// Returns the squared length of this <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>The squared length of this <see cref="Vector2I"/>.</returns>
        public int LengthSquared()
        {
            return (X * X) + (Y * Y);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains linear interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <returns>The result of linear interpolation of the specified vectors.</returns>
        public static Vector2 Lerp(Vector2I value1, Vector2I value2, float amount)
        {
            return new Vector2(
                MathHelper.Lerp(value1.X, value2.X, amount),
                MathHelper.Lerp(value1.Y, value2.Y, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains linear interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
        public static void Lerp(ref Vector2I value1, ref Vector2I value2, float amount, out Vector2 result)
        {
            result.X = MathHelper.Lerp(value1.X, value2.X, amount);
            result.Y = MathHelper.Lerp(value1.Y, value2.Y, amount);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains linear interpolation of the specified vectors.
        /// Uses <see cref="MathHelper.LerpPrecise"/> on MathHelper for the interpolation.
        /// Less efficient but more precise compared to <see cref="Vector2I.Lerp(Vector2I, Vector2I, int)"/>.
        /// See remarks section of <see cref="MathHelper.LerpPrecise"/> on MathHelper for more info.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <returns>The result of linear interpolation of the specified vectors.</returns>
        public static Vector2 LerpPrecise(Vector2I value1, Vector2I value2, float amount)
        {
            return new Vector2(
                MathHelper.LerpPrecise(value1.X, value2.X, amount),
                MathHelper.LerpPrecise(value1.Y, value2.Y, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains linear interpolation of the specified vectors.
        /// Uses <see cref="MathHelper.LerpPrecise"/> on MathHelper for the interpolation.
        /// Less efficient but more precise compared to <see cref="Vector2I.Lerp(ref Vector2I, ref Vector2I, int, out Vector2I)"/>.
        /// See remarks section of <see cref="MathHelper.LerpPrecise"/> on MathHelper for more info.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
        public static void LerpPrecise(ref Vector2I value1, ref Vector2I value2, float amount, out Vector2 result)
        { 
            result.X = MathHelper.LerpPrecise(value1.X, value2.X, amount);
            result.Y = MathHelper.LerpPrecise(value1.Y, value2.Y, amount);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a maximal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The <see cref="Vector2I"/> with maximal values from the two vectors.</returns>
        public static Vector2I Max(Vector2I value1, Vector2I value2)
        {
            return new Vector2I(value1.X > value2.X ? value1.X : value2.X,
                               value1.Y > value2.Y ? value1.Y : value2.Y);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a maximal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The <see cref="Vector2I"/> with maximal values from the two vectors as an output parameter.</param>
        public static void Max(ref Vector2I value1, ref Vector2I value2, out Vector2I result)
        {
            result.X = value1.X > value2.X ? value1.X : value2.X;
            result.Y = value1.Y > value2.Y ? value1.Y : value2.Y;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a minimal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The <see cref="Vector2I"/> with minimal values from the two vectors.</returns>
        public static Vector2I Min(Vector2I value1, Vector2I value2)
        {
            return new Vector2I(value1.X < value2.X ? value1.X : value2.X,
                               value1.Y < value2.Y ? value1.Y : value2.Y);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a minimal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The <see cref="Vector2I"/> with minimal values from the two vectors as an output parameter.</param>
        public static void Min(ref Vector2I value1, ref Vector2I value2, out Vector2I result)
        {
            result.X = value1.X < value2.X ? value1.X : value2.X;
            result.Y = value1.Y < value2.Y ? value1.Y : value2.Y;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a multiplication of two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="value2">Source <see cref="Vector2I"/>.</param>
        /// <returns>The result of the vector multiplication.</returns>
        public static Vector2I Multiply(Vector2I value1, Vector2I value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a multiplication of two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="value2">Source <see cref="Vector2I"/>.</param>
        /// <param name="result">The result of the vector multiplication as an output parameter.</param>
        public static void Multiply(ref Vector2I value1, ref Vector2I value2, out Vector2I result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a multiplication of <see cref="Vector2I"/> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <returns>The result of the vector multiplication with a scalar.</returns>
        public static Vector2I Multiply(Vector2I value1, int scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a multiplication of <see cref="Vector2I"/> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
        public static void Multiply(ref Vector2I value1, int scaleFactor, out Vector2I result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains the specified vector inversion.
        /// </summary>
        /// <param name="value">Source <see cref="Vector2I"/>.</param>
        /// <returns>The result of the vector inversion.</returns>
        public static Vector2I Negate(Vector2I value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains the specified vector inversion.
        /// </summary>
        /// <param name="value">Source <see cref="Vector2I"/>.</param>
        /// <param name="result">The result of the vector inversion as an output parameter.</param>
        public static void Negate(ref Vector2I value, out Vector2I result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains reflect vector of the given vector and normal.
        /// </summary>
        /// <param name="vector">Source <see cref="Vector2I"/>.</param>
        /// <param name="normal">Reflection normal.</param>
        /// <returns>Reflected vector.</returns>
        public static Vector2I Reflect(Vector2I vector, Vector2I normal)
        {
            Vector2I result;
            int val = 2 * ((vector.X * normal.X) + (vector.Y * normal.Y));
            result.X = vector.X - (normal.X * val);
            result.Y = vector.Y - (normal.Y * val);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains reflect vector of the given vector and normal.
        /// </summary>
        /// <param name="vector">Source <see cref="Vector2I"/>.</param>
        /// <param name="normal">Reflection normal.</param>
        /// <param name="result">Reflected vector as an output parameter.</param>
        public static void Reflect(ref Vector2I vector, ref Vector2I normal, out Vector2I result)
        {
            int val = 2 * ((vector.X * normal.X) + (vector.Y * normal.Y));
            result.X = vector.X - (normal.X * val);
            result.Y = vector.Y - (normal.Y * val);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains cubic interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="value2">Source <see cref="Vector2I"/>.</param>
        /// <param name="amount">Weighting value.</param>
        /// <returns>Cubic interpolation of the specified vectors.</returns>
        public static Vector2 SmoothStep(Vector2I value1, Vector2I value2, float amount)
        {
            return new Vector2(
                MathHelper.SmoothStep(value1.X, value2.X, amount),
                MathHelper.SmoothStep(value1.Y, value2.Y, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains cubic interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="value2">Source <see cref="Vector2I"/>.</param>
        /// <param name="amount">Weighting value.</param>
        /// <param name="result">Cubic interpolation of the specified vectors as an output parameter.</param>
        public static void SmoothStep(ref Vector2I value1, ref Vector2I value2, float amount, out Vector2 result)
        {
            result.X = MathHelper.SmoothStep(value1.X, value2.X, amount);
            result.Y = MathHelper.SmoothStep(value1.Y, value2.Y, amount);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains subtraction of on <see cref="Vector2I"/> from a another.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="value2">Source <see cref="Vector2I"/>.</param>
        /// <returns>The result of the vector subtraction.</returns>
        public static Vector2I Subtract(Vector2I value1, Vector2I value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains subtraction of on <see cref="Vector2I"/> from a another.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2I"/>.</param>
        /// <param name="value2">Source <see cref="Vector2I"/>.</param>
        /// <param name="result">The result of the vector subtraction as an output parameter.</param>
        public static void Subtract(ref Vector2I value1, ref Vector2I value2, out Vector2I result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Vector2I"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>]}
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Vector2I"/>.</returns>
        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + "}";
        }

        /// <summary>
        /// Gets a <see cref="Vector2I"/> representation for this object.
        /// </summary>
        /// <returns>A <see cref="Vector2I"/> representation for this object.</returns>
        public Vector2I ToVector2I()
        {
            return new Vector2I((int) X,(int) Y);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="position">Source <see cref="Vector2I"/>.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <returns>Transformed <see cref="Vector2I"/>.</returns>
        public static Vector2 Transform(Vector2I position, Matrix matrix)
        {
            return new Vector2((position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41, (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="position">Source <see cref="Vector2I"/>.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="result">Transformed <see cref="Vector2I"/> as an output parameter.</param>
        public static void Transform(ref Vector2I position, ref Matrix matrix, out Vector2 result)
        {
            var x = (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41;
            var y = (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42;
            result.X = x;
            result.Y = y;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a transformation of 2d-vector by the specified <see cref="Quaternion"/>, representing the rotation.
        /// </summary>
        /// <param name="value">Source <see cref="Vector2I"/>.</param>
        /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
        /// <returns>Transformed <see cref="Vector2I"/>.</returns>
        public static Vector2I Transform(Vector2I value, Quaternion rotation)
        {
            Transform(ref value, ref rotation, out value);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a transformation of 2d-vector by the specified <see cref="Quaternion"/>, representing the rotation.
        /// </summary>
        /// <param name="value">Source <see cref="Vector2I"/>.</param>
        /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
        /// <param name="result">Transformed <see cref="Vector2I"/> as an output parameter.</param>
        public static void Transform(ref Vector2I value, ref Quaternion rotation, out Vector2I result)
        {
            var rot1 = new Vector3(rotation.X + rotation.X, rotation.Y + rotation.Y, rotation.Z + rotation.Z);
            var rot2 = new Vector3(rotation.X, rotation.X, rotation.W);
            var rot3 = new Vector3(1, rotation.Y, rotation.Z);
            var rot4 = rot1*rot2;
            var rot5 = rot1*rot3;

            var v = new Vector2I();
            v.X = (int)((double)value.X * (1.0 - (double)rot5.Y - (double)rot5.Z) + (double)value.Y * ((double)rot4.Y - (double)rot4.Z));
            v.Y = (int)((double)value.X * ((double)rot4.Y + (double)rot4.Z) + (double)value.Y * (1.0 - (double)rot4.X - (double)rot5.Z));
            result.X = v.X;
            result.Y = v.Y;
        }

        /// <summary>
        /// Apply transformation on vectors within array of <see cref="Vector2I"/> by the specified <see cref="Matrix"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="destinationArray">Destination array.</param>
        /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector2I"/> should be written.</param>
        /// <param name="length">The number of vectors to be transformed.</param>
        public static void Transform(
            Vector2I[] sourceArray,
            int sourceIndex,
            ref Matrix matrix,
            Vector2[] destinationArray,
            int destinationIndex,
            int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if (sourceArray.Length < sourceIndex + length)
                throw new ArgumentException("Source array length is lesser than sourceIndex + length");
            if (destinationArray.Length < destinationIndex + length)
                throw new ArgumentException("Destination array length is lesser than destinationIndex + length");

            for (int x = 0; x < length; x++)
            {
                var position = sourceArray[sourceIndex + x];
                var destination = destinationArray[destinationIndex + x];
                destination.X = (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41;
                destination.Y = (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42;
                destinationArray[destinationIndex + x] = destination;
            }
        }

        /// <summary>
        /// Apply transformation on vectors within array of <see cref="Vector2I"/> by the specified <see cref="Quaternion"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
        /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
        /// <param name="destinationArray">Destination array.</param>
        /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector2I"/> should be written.</param>
        /// <param name="length">The number of vectors to be transformed.</param>
        public static void Transform
        (
            Vector2I[] sourceArray,
            int sourceIndex,
            ref Quaternion rotation,
            Vector2I[] destinationArray,
            int destinationIndex,
            int length
        )
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if (sourceArray.Length < sourceIndex + length)
                throw new ArgumentException("Source array length is lesser than sourceIndex + length");
            if (destinationArray.Length < destinationIndex + length)
                throw new ArgumentException("Destination array length is lesser than destinationIndex + length");

            for (int x = 0; x < length; x++)
            {
                var position = sourceArray[sourceIndex + x];
                var destination = destinationArray[destinationIndex + x];

                Vector2I v;
                Transform(ref position,ref rotation,out v); 

                destination.X = v.X;
                destination.Y = v.Y;

                destinationArray[destinationIndex + x] = destination;
            }
        }

        /// <summary>
        /// Apply transformation on all vectors within array of <see cref="Vector2I"/> by the specified <see cref="Matrix"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="destinationArray">Destination array.</param>
        public static void Transform(
            Vector2I[] sourceArray,
            ref Matrix matrix,
            Vector2[] destinationArray)
        {
            Transform(sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length);
        }

        /// <summary>
        /// Apply transformation on all vectors within array of <see cref="Vector2I"/> by the specified <see cref="Quaternion"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
        /// <param name="destinationArray">Destination array.</param>
        public static void Transform
        (
            Vector2I[] sourceArray,
            ref Quaternion rotation,
            Vector2I[] destinationArray
        )
        {
            Transform(sourceArray, 0, ref rotation, destinationArray, 0, sourceArray.Length);
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a transformation of the specified normal by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="normal">Source <see cref="Vector2I"/> which represents a normal vector.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <returns>Transformed normal.</returns>
        public static Vector2 TransformNormal(Vector2I normal, Matrix matrix)
        {
            return new Vector2((normal.X * matrix.M11) + (normal.Y * matrix.M21),(normal.X * matrix.M12) + (normal.Y * matrix.M22));
        }

        /// <summary>
        /// Creates a new <see cref="Vector2I"/> that contains a transformation of the specified normal by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="normal">Source <see cref="Vector2I"/> which represents a normal vector.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="result">Transformed normal as an output parameter.</param>
        public static void TransformNormal(ref Vector2I normal, ref Matrix matrix, out Vector2 result)
        {
            var x = (normal.X * matrix.M11) + (normal.Y * matrix.M21);
            var y = (normal.X * matrix.M12) + (normal.Y * matrix.M22);
            result.X = x;
            result.Y = y;
        }
        
        /// <summary>
        /// Apply transformation on normals within array of <see cref="Vector2I"/> by the specified <see cref="Matrix"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="destinationArray">Destination array.</param>
        /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector2I"/> should be written.</param>
        /// <param name="length">The number of normals to be transformed.</param>
        public static void TransformNormal
        (
            Vector2I[] sourceArray,
            int sourceIndex,
            ref Matrix matrix,
            Vector2[] destinationArray,
            int destinationIndex,
            int length
        )
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if (sourceArray.Length < sourceIndex + length)
                throw new ArgumentException("Source array length is lesser than sourceIndex + length");
            if (destinationArray.Length < destinationIndex + length)
                throw new ArgumentException("Destination array length is lesser than destinationIndex + length");

            for (int i = 0; i < length; i++)
            {
                var normal = sourceArray[sourceIndex + i];

                destinationArray[destinationIndex + i] = new Vector2((normal.X * matrix.M11) + (normal.Y * matrix.M21),
                                                                     (normal.X * matrix.M12) + (normal.Y * matrix.M22));
            }
        }

        /// <summary>
        /// Apply transformation on all normals within array of <see cref="Vector2I"/> by the specified <see cref="Matrix"/> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="destinationArray">Destination array.</param>
        public static void TransformNormal
            (
            Vector2I[] sourceArray,
            ref Matrix matrix,
            Vector2[] destinationArray
            )
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException("Destination array length is lesser than source array length");

            for (int i = 0; i < sourceArray.Length; i++)
            {
                var normal = sourceArray[i];

                destinationArray[i] = new Vector2((normal.X * matrix.M11) + (normal.Y * matrix.M21),
                                                  (normal.X * matrix.M12) + (normal.Y * matrix.M22));
            }
        }

        /// <summary>
        /// Rotates a vector by the specified number of radians
        /// </summary>
        /// <param name="value">The vector to be rotated.</param>
        /// <param name="radians">The amount to rotate the vector.</param>
        /// <returns>A rotated copy of value.</returns>
        /// <remarks>
        /// A positive angle and negative angle
        /// would rotate counterclockwise and clockwise,
        /// respectively
        /// </remarks>
        public static Vector2 Rotate(Vector2I value, float radians)
        {
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            return new Vector2(value.X * cos - value.Y * sin, value.X * sin + value.Y * cos);
        }

        /// <summary>
        /// Deconstruction method for <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        /// <summary>
        /// Returns a <see cref="System.Numerics.Vector2I"/>.
        /// </summary>
        public System.Numerics.Vector2 ToNumerics()
        {
            return new System.Numerics.Vector2(this.X, this.Y);
        }

        public static implicit operator Point(Vector2I vec)
        {
            return new Point(vec.X, vec.Y);
        }

        public static implicit operator Vector2I(Point point) {
            return new Vector2I(point.X, point.Y);
        }

        public static implicit operator Vector2(Vector2I vec)
        {
            return new Vector2(vec.X, vec.Y);
        }

        public static explicit operator Vector2I(Vector2 vec)
        {
            return new Vector2I((int)MathF.Floor(vec.X), (int)MathF.Floor(vec.Y));
        }

        #endregion
    }
}