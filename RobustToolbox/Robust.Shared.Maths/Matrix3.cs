#region --- License ---

/*
Copyright (c) 2006 - 2008 The Open Toolkit library.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

#endregion --- License ---

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Robust.Shared.Utility;

namespace Robust.Shared.Maths
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix3 : IEquatable<Matrix3>, IApproxEquatable<Matrix3>, ISpanFormattable
    {
        #region Fields & Access

        /// <summary>Row 0, Column 0</summary>
        public float R0C0;

        /// <summary>Row 0, Column 1</summary>
        public float R0C1;

        /// <summary>Row 0, Column 2</summary>
        public float R0C2;

        /// <summary>Row 1, Column 0</summary>
        public float R1C0;

        /// <summary>Row 1, Column 1</summary>
        public float R1C1;

        /// <summary>Row 1, Column 2</summary>
        public float R1C2;

        /// <summary>Row 2, Column 0</summary>
        public float R2C0;

        /// <summary>Row 2, Column 1</summary>
        public float R2C1;

        /// <summary>Row 2, Column 2</summary>
        public float R2C2;

        /// <summary>Gets the component at the given row and column in the matrix.</summary>
        /// <param name="row">The row of the matrix.</param>
        /// <param name="column">The column of the matrix.</param>
        /// <returns>The component at the given row and column in the matrix.</returns>
        public float this[int row, int column]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0: return R0C0;
                            case 1: return R0C1;
                            case 2: return R0C2;
                        }

                        break;

                    case 1:
                        switch (column)
                        {
                            case 0: return R1C0;
                            case 1: return R1C1;
                            case 2: return R1C2;
                        }

                        break;

                    case 2:
                        switch (column)
                        {
                            case 0: return R2C0;
                            case 1: return R2C1;
                            case 2: return R2C2;
                        }

                        break;
                }

                throw new IndexOutOfRangeException();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0:
                                R0C0 = value;
                                return;
                            case 1:
                                R0C1 = value;
                                return;
                            case 2:
                                R0C2 = value;
                                return;
                        }

                        break;

                    case 1:
                        switch (column)
                        {
                            case 0:
                                R1C0 = value;
                                return;
                            case 1:
                                R1C1 = value;
                                return;
                            case 2:
                                R1C2 = value;
                                return;
                        }

                        break;

                    case 2:
                        switch (column)
                        {
                            case 0:
                                R2C0 = value;
                                return;
                            case 1:
                                R2C1 = value;
                                return;
                            case 2:
                                R2C2 = value;
                                return;
                        }

                        break;
                }

                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>Gets the component at the index into the matrix.</summary>
        /// <param name="index">The index into the components of the matrix.</param>
        /// <returns>The component at the given index into the matrix.</returns>
        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                switch (index)
                {
                    case 0: return R0C0;
                    case 1: return R0C1;
                    case 2: return R0C2;
                    case 3: return R1C0;
                    case 4: return R1C1;
                    case 5: return R1C2;
                    case 6: return R2C0;
                    case 7: return R2C1;
                    case 8: return R2C2;
                    default: throw new IndexOutOfRangeException();
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (index)
                {
                    case 0:
                        R0C0 = value;
                        return;
                    case 1:
                        R0C1 = value;
                        return;
                    case 2:
                        R0C2 = value;
                        return;
                    case 3:
                        R1C0 = value;
                        return;
                    case 4:
                        R1C1 = value;
                        return;
                    case 5:
                        R1C2 = value;
                        return;
                    case 6:
                        R2C0 = value;
                        return;
                    case 7:
                        R2C1 = value;
                        return;
                    case 8:
                        R2C2 = value;
                        return;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>Converts the matrix into an array of floats.</summary>
        /// <param name="matrix">The matrix to convert.</param>
        /// <returns>An array of floats for the matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator float[](in Matrix3 matrix)
        {
            return new[]
            {
                matrix.R0C0,
                matrix.R0C1,
                matrix.R0C2,
                matrix.R1C0,
                matrix.R1C1,
                matrix.R1C2,
                matrix.R2C0,
                matrix.R2C1,
                matrix.R2C2
            };
        }

        #endregion Fields & Access

        #region Constructors

        /// <summary>Constructs left matrix with the same components as the given matrix.</summary>
        /// <param name="matrix">The matrix whose components to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3(in Matrix3 matrix)
        {
            R0C0 = matrix.R0C0;
            R0C1 = matrix.R0C1;
            R0C2 = matrix.R0C2;
            R1C0 = matrix.R1C0;
            R1C1 = matrix.R1C1;
            R1C2 = matrix.R1C2;
            R2C0 = matrix.R2C0;
            R2C1 = matrix.R2C1;
            R2C2 = matrix.R2C2;
        }

        /// <summary>Constructs left matrix with the given values.</summary>
        /// <param name="r0c0">The value for row 0 column 0.</param>
        /// <param name="r0c1">The value for row 0 column 1.</param>
        /// <param name="r0c2">The value for row 0 column 2.</param>
        /// <param name="r1c0">The value for row 1 column 0.</param>
        /// <param name="r1c1">The value for row 1 column 1.</param>
        /// <param name="r1c2">The value for row 1 column 2.</param>
        /// <param name="r2c0">The value for row 2 column 0.</param>
        /// <param name="r2c1">The value for row 2 column 1.</param>
        /// <param name="r2c2">The value for row 2 column 2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3
        (
            float r0c0,
            float r0c1,
            float r0c2,
            float r1c0,
            float r1c1,
            float r1c2,
            float r2c0,
            float r2c1,
            float r2c2
        )
        {
            R0C0 = r0c0;
            R0C1 = r0c1;
            R0C2 = r0c2;
            R1C0 = r1c0;
            R1C1 = r1c1;
            R1C2 = r1c2;
            R2C0 = r2c0;
            R2C1 = r2c1;
            R2C2 = r2c2;
        }

        /// <summary>Constructs left matrix from the given array of float-precision floating-point numbers.</summary>
        /// <param name="floatArray">The array of floats for the components of the matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3(float[] floatArray)
        {
            if (floatArray == null || floatArray.GetLength(0) < 9) throw new MissingFieldException();

            R0C0 = floatArray[0];
            R0C1 = floatArray[1];
            R0C2 = floatArray[2];
            R1C0 = floatArray[3];
            R1C1 = floatArray[4];
            R1C2 = floatArray[5];
            R2C0 = floatArray[6];
            R2C1 = floatArray[7];
            R2C2 = floatArray[8];
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="matrix">A Matrix4 to take the upper-left 3x3 from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3(in Matrix4 matrix)
        {
            R0C0 = matrix.Row0.X;
            R0C1 = matrix.Row0.Y;
            R0C2 = matrix.Row0.Z;

            R1C0 = matrix.Row1.X;
            R1C1 = matrix.Row1.Y;
            R1C2 = matrix.Row1.Z;

            R2C0 = matrix.Row2.X;
            R2C1 = matrix.Row2.Y;
            R2C2 = matrix.Row2.Z;
        }

        /// <summary>
        /// Constructs a new instance from 3 vectors to quickly synthesize affine transforms.
        /// </summary>
        /// <param name="x">A Vector2 for the first, conventionally X basis, vector.</param>
        /// <param name="y">A Vector2 for the second, conventionally Y basis, vector.</param>
        /// <param name="origin">A Vector2 for the third, conventionally origin vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3(in Vector2 x, in Vector2 y, in Vector2 origin)
        {
            R0C0 = x.X;
            R0C1 = y.X;
            R0C2 = origin.X;

            R1C0 = x.Y;
            R1C1 = y.Y;
            R1C2 = origin.Y;

            R2C0 = 0;
            R2C1 = 0;
            R2C2 = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateTranslation(float x, float y)
        {
            var result = Identity;

            /* column major
             1 0 x
             0 1 y
             0 0 1
            */
            result.R0C2 = x;
            result.R1C2 = y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateTranslation(Vector2 vector)
        {
            return CreateTranslation(vector.X, vector.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateRotation(Angle angle)
        {
            var cos = (float) Math.Cos(angle);
            var sin = (float) Math.Sin(angle);

            var result = Identity;

            /* column major
             cos -sin 0
             sin  cos 0
              0    0  1
            */
            result.R0C0 = cos;
            result.R1C0 = sin;
            result.R0C1 = -sin;
            result.R1C1 = cos;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateScale(float x, float y)
        {
            var result = Identity;

            /* column major
             x 0 0
             0 y 0
             0 0 1
            */
            result.R0C0 = x;
            result.R1C1 = y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateScale(in Vector2 scale)
        {
            return CreateScale(scale.X, scale.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateTransform(float posX, float posY, float angle, float scaleX = 1, float scaleY = 1)
        {
            // returns a matrix that is equivalent to returning CreateScale(scale) * CreateRotation(angle) * CreateTranslation(posX, posY)

            var sin = MathF.Sin(angle);
            var cos = MathF.Cos(angle);

            return new Matrix3
            {
                R0C0 = cos * scaleX,
                R0C1 = -sin * scaleY,
                R0C2 = posX,
                R1C0 = sin * scaleX,
                R1C1 = cos * scaleY,
                R1C2 = posY,
                R2C2 = 1
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateInverseTransform(float posX, float posY, float angle, float scaleX = 1, float scaleY = 1)
        {
            // returns a matrix that is equivalent to returning CreateTranslation(-posX, -posY) * CreateRotation(-angle) * CreateScale(1/scaleX, 1/scaleY)

            var sin = MathF.Sin(angle);
            var cos = MathF.Cos(angle);

            return new Matrix3
            {
                R0C0 = cos / scaleX,
                R0C1 = sin / scaleX,
                R0C2 = - (posX * cos + posY * sin) / scaleX,
                R1C0 = -sin / scaleY,
                R1C1 = cos / scaleY,
                R1C2 = (posX * sin - posY * cos) / scaleY,
                R2C2 = 1
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateTransform(in Vector2 position, in Angle angle)
        {
            // Rounding moment
            return angle.Theta switch
            {
                -Math.PI / 2 => new Matrix3(0f, 1f, position.X, -1f, 0f, position.Y, 0f, 0f, 1f),
                Math.PI / 2 => new Matrix3(0f, -1f, position.X, 1f, 0f, position.Y, 0f, 0f, 1f),
                Math.PI => new Matrix3(-1f, 0f, position.X, 0f, -1f, position.Y, 0f, 0f, 1f),
                _ => CreateTransform(position.X, position.Y, (float) angle.Theta)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateTransform(in Vector2 position, in Angle angle, in Vector2 scale)
        {
            return CreateTransform(position.X, position.Y, (float)angle.Theta, scale.X, scale.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateInverseTransform(in Vector2 position, in Angle angle)
        {
            return CreateInverseTransform(position.X, position.Y, (float)angle.Theta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 CreateInverseTransform(in Vector2 position, in Angle angle, in Vector2 scale)
        {
            return CreateInverseTransform(position.X, position.Y, (float)angle.Theta, scale.X, scale.Y);
        }

        #endregion Constructors

        #region Equality

        /// <summary>Indicates whether the current matrix is equal to another matrix.</summary>
        /// <param name="matrix">The Matrix3 structure to compare with.</param>
        /// <returns>true if the current matrix is equal to the matrix parameter; otherwise, false.</returns>
        public readonly bool Equals(Matrix3 other)
        {
            return
                R0C0 == other.R0C0 &&
                R0C1 == other.R0C1 &&
                R0C2 == other.R0C2 &&
                R1C0 == other.R1C0 &&
                R1C1 == other.R1C1 &&
                R1C2 == other.R1C2 &&
                R2C0 == other.R2C0 &&
                R2C1 == other.R2C1 &&
                R2C2 == other.R2C2;
        }

        /// <summary>Indicates whether the current matrix is equal to another matrix.</summary>
        /// <param name="matrix">The Matrix3 structure to compare to.</param>
        /// <returns>true if the current matrix is equal to the matrix parameter; otherwise, false.</returns>
        public readonly bool Equals(in Matrix3 matrix)
        {
            return
                R0C0 == matrix.R0C0 &&
                R0C1 == matrix.R0C1 &&
                R0C2 == matrix.R0C2 &&
                R1C0 == matrix.R1C0 &&
                R1C1 == matrix.R1C1 &&
                R1C2 == matrix.R1C2 &&
                R2C0 == matrix.R2C0 &&
                R2C1 == matrix.R2C1 &&
                R2C2 == matrix.R2C2;
        }

        /// <summary>Indicates whether the current matrix is equal to another matrix.</summary>
        /// <param name="left">The left-hand operand.</param>
        /// <param name="right">The right-hand operand.</param>
        /// <returns>true if the current matrix is equal to the matrix parameter; otherwise, false.</returns>
        public static bool Equals(in Matrix3 left, in Matrix3 right)
        {
            return
                left.R0C0 == right.R0C0 &&
                left.R0C1 == right.R0C1 &&
                left.R0C2 == right.R0C2 &&
                left.R1C0 == right.R1C0 &&
                left.R1C1 == right.R1C1 &&
                left.R1C2 == right.R1C2 &&
                left.R2C0 == right.R2C0 &&
                left.R2C1 == right.R2C1 &&
                left.R2C2 == right.R2C2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsApprox(Matrix3 other)
        {
            return EqualsApprox(in other, 1.0E-6f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsApprox(Matrix3 other, double tolerance)
        {
            return EqualsApprox(in other, (float) tolerance);
        }

        /// <summary>Indicates whether the current matrix is approximately equal to another matrix.</summary>
        /// <param name="matrix">The Matrix3 structure to compare with.</param>
        /// <param name="tolerance">The limit below which the matrices are considered equal.</param>
        /// <returns>true if the current matrix is approximately equal to the matrix parameter; otherwise, false.</returns>
        public readonly bool EqualsApprox(in Matrix3 matrix, float tolerance)
        {
            return
                Math.Abs(R0C0 - matrix.R0C0) <= tolerance &&
                Math.Abs(R0C1 - matrix.R0C1) <= tolerance &&
                Math.Abs(R0C2 - matrix.R0C2) <= tolerance &&
                Math.Abs(R1C0 - matrix.R1C0) <= tolerance &&
                Math.Abs(R1C1 - matrix.R1C1) <= tolerance &&
                Math.Abs(R1C2 - matrix.R1C2) <= tolerance &&
                Math.Abs(R2C0 - matrix.R2C0) <= tolerance &&
                Math.Abs(R2C1 - matrix.R2C1) <= tolerance &&
                Math.Abs(R2C2 - matrix.R2C2) <= tolerance;
        }

        /// <summary>Indicates whether the current matrix is approximately equal to another matrix.</summary>
        /// <param name="left">The left-hand operand.</param>
        /// <param name="right">The right-hand operand.</param>
        /// <param name="tolerance">The limit below which the matrices are considered equal.</param>
        /// <returns>true if the current matrix is approximately equal to the matrix parameter; otherwise, false.</returns>
        public static bool EqualsApprox(in Matrix3 left, in Matrix3 right, float tolerance)
        {
            return
                Math.Abs(left.R0C0 - right.R0C0) <= tolerance &&
                Math.Abs(left.R0C1 - right.R0C1) <= tolerance &&
                Math.Abs(left.R0C2 - right.R0C2) <= tolerance &&
                Math.Abs(left.R1C0 - right.R1C0) <= tolerance &&
                Math.Abs(left.R1C1 - right.R1C1) <= tolerance &&
                Math.Abs(left.R1C2 - right.R1C2) <= tolerance &&
                Math.Abs(left.R2C0 - right.R2C0) <= tolerance &&
                Math.Abs(left.R2C1 - right.R2C1) <= tolerance &&
                Math.Abs(left.R2C2 - right.R2C2) <= tolerance;
        }

        #endregion Equality

        #region Arithmetic Operators

        /// <summary>Add left matrix to this matrix.</summary>
        /// <param name="matrix">The matrix to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in Matrix3 matrix)
        {
            R0C0 = R0C0 + matrix.R0C0;
            R0C1 = R0C1 + matrix.R0C1;
            R0C2 = R0C2 + matrix.R0C2;
            R1C0 = R1C0 + matrix.R1C0;
            R1C1 = R1C1 + matrix.R1C1;
            R1C2 = R1C2 + matrix.R1C2;
            R2C0 = R2C0 + matrix.R2C0;
            R2C1 = R2C1 + matrix.R2C1;
            R2C2 = R2C2 + matrix.R2C2;
        }

        /// <summary>Add left matrix to this matrix.</summary>
        /// <param name="matrix">The matrix to add.</param>
        /// <param name="result">The resulting matrix of the addition.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Add(in Matrix3 matrix, out Matrix3 result)
        {
            result.R0C0 = R0C0 + matrix.R0C0;
            result.R0C1 = R0C1 + matrix.R0C1;
            result.R0C2 = R0C2 + matrix.R0C2;
            result.R1C0 = R1C0 + matrix.R1C0;
            result.R1C1 = R1C1 + matrix.R1C1;
            result.R1C2 = R1C2 + matrix.R1C2;
            result.R2C0 = R2C0 + matrix.R2C0;
            result.R2C1 = R2C1 + matrix.R2C1;
            result.R2C2 = R2C2 + matrix.R2C2;
        }

        /// <summary>Add left matrix to left matrix.</summary>
        /// <param name="matrix">The matrix on the matrix side of the equation.</param>
        /// <param name="right">The matrix on the right side of the equation</param>
        /// <param name="result">The resulting matrix of the addition.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(in Matrix3 left, in Matrix3 right, out Matrix3 result)
        {
            result.R0C0 = left.R0C0 + right.R0C0;
            result.R0C1 = left.R0C1 + right.R0C1;
            result.R0C2 = left.R0C2 + right.R0C2;
            result.R1C0 = left.R1C0 + right.R1C0;
            result.R1C1 = left.R1C1 + right.R1C1;
            result.R1C2 = left.R1C2 + right.R1C2;
            result.R2C0 = left.R2C0 + right.R2C0;
            result.R2C1 = left.R2C1 + right.R2C1;
            result.R2C2 = left.R2C2 + right.R2C2;
        }

        /// <summary>Subtract matrix from this matrix.</summary>
        /// <param name="matrix">The matrix to subtract.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subtract(in Matrix3 matrix)
        {
            R0C0 = R0C0 - matrix.R0C0;
            R0C1 = R0C1 - matrix.R0C1;
            R0C2 = R0C2 - matrix.R0C2;
            R1C0 = R1C0 - matrix.R1C0;
            R1C1 = R1C1 - matrix.R1C1;
            R1C2 = R1C2 - matrix.R1C2;
            R2C0 = R2C0 - matrix.R2C0;
            R2C1 = R2C1 - matrix.R2C1;
            R2C2 = R2C2 - matrix.R2C2;
        }

        /// <summary>Subtract matrix from this matrix.</summary>
        /// <param name="matrix">The matrix to subtract.</param>
        /// <param name="result">The resulting matrix of the subtraction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Subtract(in Matrix3 matrix, out Matrix3 result)
        {
            result.R0C0 = R0C0 - matrix.R0C0;
            result.R0C1 = R0C1 - matrix.R0C1;
            result.R0C2 = R0C2 - matrix.R0C2;
            result.R1C0 = R1C0 - matrix.R1C0;
            result.R1C1 = R1C1 - matrix.R1C1;
            result.R1C2 = R1C2 - matrix.R1C2;
            result.R2C0 = R2C0 - matrix.R2C0;
            result.R2C1 = R2C1 - matrix.R2C1;
            result.R2C2 = R2C2 - matrix.R2C2;
        }

        /// <summary>Subtract right matrix from left matrix.</summary>
        /// <param name="left">The matrix on the matrix side of the equation.</param>
        /// <param name="right">The matrix on the right side of the equation</param>
        /// <param name="result">The resulting matrix of the subtraction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subtract(in Matrix3 left, in Matrix3 right, out Matrix3 result)
        {
            result.R0C0 = left.R0C0 - right.R0C0;
            result.R0C1 = left.R0C1 - right.R0C1;
            result.R0C2 = left.R0C2 - right.R0C2;
            result.R1C0 = left.R1C0 - right.R1C0;
            result.R1C1 = left.R1C1 - right.R1C1;
            result.R1C2 = left.R1C2 - right.R1C2;
            result.R2C0 = left.R2C0 - right.R2C0;
            result.R2C1 = left.R2C1 - right.R2C1;
            result.R2C2 = left.R2C2 - right.R2C2;
        }

        /// <summary>Multiply left matrix times this matrix.</summary>
        /// <param name="matrix">The matrix to multiply.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiply(in Matrix3 matrix)
        {
            var r0c0 = matrix.R0C0 * R0C0 + matrix.R0C1 * R1C0 + matrix.R0C2 * R2C0;
            var r0c1 = matrix.R0C0 * R0C1 + matrix.R0C1 * R1C1 + matrix.R0C2 * R2C1;
            var r0c2 = matrix.R0C0 * R0C2 + matrix.R0C1 * R1C2 + matrix.R0C2 * R2C2;

            var r1c0 = matrix.R1C0 * R0C0 + matrix.R1C1 * R1C0 + matrix.R1C2 * R2C0;
            var r1c1 = matrix.R1C0 * R0C1 + matrix.R1C1 * R1C1 + matrix.R1C2 * R2C1;
            var r1c2 = matrix.R1C0 * R0C2 + matrix.R1C1 * R1C2 + matrix.R1C2 * R2C2;

            R2C0 = matrix.R2C0 * R0C0 + matrix.R2C1 * R1C0 + matrix.R2C2 * R2C0;
            R2C1 = matrix.R2C0 * R0C1 + matrix.R2C1 * R1C1 + matrix.R2C2 * R2C1;
            R2C2 = matrix.R2C0 * R0C2 + matrix.R2C1 * R1C2 + matrix.R2C2 * R2C2;

            R0C0 = r0c0;
            R0C1 = r0c1;
            R0C2 = r0c2;

            R1C0 = r1c0;
            R1C1 = r1c1;
            R1C2 = r1c2;
        }

        /// <summary>Multiply matrix times this matrix.</summary>
        /// <param name="matrix">The matrix to multiply.</param>
        /// <param name="result">The resulting matrix of the multiplication.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Multiply(in Matrix3 matrix, out Matrix3 result)
        {
            result.R0C0 = matrix.R0C0 * R0C0 + matrix.R0C1 * R1C0 + matrix.R0C2 * R2C0;
            result.R0C1 = matrix.R0C0 * R0C1 + matrix.R0C1 * R1C1 + matrix.R0C2 * R2C1;
            result.R0C2 = matrix.R0C0 * R0C2 + matrix.R0C1 * R1C2 + matrix.R0C2 * R2C2;
            result.R1C0 = matrix.R1C0 * R0C0 + matrix.R1C1 * R1C0 + matrix.R1C2 * R2C0;
            result.R1C1 = matrix.R1C0 * R0C1 + matrix.R1C1 * R1C1 + matrix.R1C2 * R2C1;
            result.R1C2 = matrix.R1C0 * R0C2 + matrix.R1C1 * R1C2 + matrix.R1C2 * R2C2;
            result.R2C0 = matrix.R2C0 * R0C0 + matrix.R2C1 * R1C0 + matrix.R2C2 * R2C0;
            result.R2C1 = matrix.R2C0 * R0C1 + matrix.R2C1 * R1C1 + matrix.R2C2 * R2C1;
            result.R2C2 = matrix.R2C0 * R0C2 + matrix.R2C1 * R1C2 + matrix.R2C2 * R2C2;
        }

        /// <summary>Multiply left matrix times right matrix.</summary>
        /// <param name="left">The matrix on the matrix side of the equation.</param>
        /// <param name="right">The matrix on the right side of the equation</param>
        /// <param name="result">The resulting matrix of the multiplication.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(in Matrix3 left, in Matrix3 right, out Matrix3 result)
        {
            result.R0C0 = right.R0C0 * left.R0C0 + right.R0C1 * left.R1C0 + right.R0C2 * left.R2C0;
            result.R0C1 = right.R0C0 * left.R0C1 + right.R0C1 * left.R1C1 + right.R0C2 * left.R2C1;
            result.R0C2 = right.R0C0 * left.R0C2 + right.R0C1 * left.R1C2 + right.R0C2 * left.R2C2;
            result.R1C0 = right.R1C0 * left.R0C0 + right.R1C1 * left.R1C0 + right.R1C2 * left.R2C0;
            result.R1C1 = right.R1C0 * left.R0C1 + right.R1C1 * left.R1C1 + right.R1C2 * left.R2C1;
            result.R1C2 = right.R1C0 * left.R0C2 + right.R1C1 * left.R1C2 + right.R1C2 * left.R2C2;
            result.R2C0 = right.R2C0 * left.R0C0 + right.R2C1 * left.R1C0 + right.R2C2 * left.R2C0;
            result.R2C1 = right.R2C0 * left.R0C1 + right.R2C1 * left.R1C1 + right.R2C2 * left.R2C1;
            result.R2C2 = right.R2C0 * left.R0C2 + right.R2C1 * left.R1C2 + right.R2C2 * left.R2C2;
        }

        /// <summary>Multiply matrix times this scalar.</summary>
        /// <param name="scalar">The scalar to multiply.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiply(float scalar)
        {
            R0C0 = scalar * R0C0;
            R0C1 = scalar * R0C1;
            R0C2 = scalar * R0C2;
            R1C0 = scalar * R1C0;
            R1C1 = scalar * R1C1;
            R1C2 = scalar * R1C2;
            R2C0 = scalar * R2C0;
            R2C1 = scalar * R2C1;
            R2C2 = scalar * R2C2;
        }

        /// <summary>Multiply matrix times this matrix.</summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <param name="result">The resulting matrix of the multiplication.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Multiply(float scalar, out Matrix3 result)
        {
            result.R0C0 = scalar * R0C0;
            result.R0C1 = scalar * R0C1;
            result.R0C2 = scalar * R0C2;
            result.R1C0 = scalar * R1C0;
            result.R1C1 = scalar * R1C1;
            result.R1C2 = scalar * R1C2;
            result.R2C0 = scalar * R2C0;
            result.R2C1 = scalar * R2C1;
            result.R2C2 = scalar * R2C2;
        }

        /// <summary>Multiply left matrix times left matrix.</summary>
        /// <param name="matrix">The matrix on the matrix side of the equation.</param>
        /// <param name="scalar">The scalar on the right side of the equation</param>
        /// <param name="result">The resulting matrix of the multiplication.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Multiply(in Matrix3 matrix, float scalar, out Matrix3 result)
        {
            result.R0C0 = scalar * matrix.R0C0;
            result.R0C1 = scalar * matrix.R0C1;
            result.R0C2 = scalar * matrix.R0C2;
            result.R1C0 = scalar * matrix.R1C0;
            result.R1C1 = scalar * matrix.R1C1;
            result.R1C2 = scalar * matrix.R1C2;
            result.R2C0 = scalar * matrix.R2C0;
            result.R2C1 = scalar * matrix.R2C1;
            result.R2C2 = scalar * matrix.R2C2;
        }

        #endregion Arithmetic Operators

        #region Functions

        public readonly double Determinant
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => R0C0 * R1C1 * R2C2 - R0C0 * R1C2 * R2C1 - R0C1 * R1C0 * R2C2 + R0C2 * R1C0 * R2C1 + R0C1 * R1C2 * R2C0 - R0C2 * R1C1 * R2C0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Transpose()
        {
            MathHelper.Swap(ref R0C1, ref R1C0);
            MathHelper.Swap(ref R0C2, ref R2C0);
            MathHelper.Swap(ref R1C2, ref R2C1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Transpose(out Matrix3 result)
        {
            result.R0C0 = R0C0;
            result.R0C1 = R1C0;
            result.R0C2 = R2C0;
            result.R1C0 = R0C1;
            result.R1C1 = R1C1;
            result.R1C2 = R2C1;
            result.R2C0 = R0C2;
            result.R2C1 = R1C2;
            result.R2C2 = R2C2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transpose(in Matrix3 matrix, out Matrix3 result)
        {
            result.R0C0 = matrix.R0C0;
            result.R0C1 = matrix.R1C0;
            result.R0C2 = matrix.R2C0;
            result.R1C0 = matrix.R0C1;
            result.R1C1 = matrix.R1C1;
            result.R1C2 = matrix.R2C1;
            result.R2C0 = matrix.R0C2;
            result.R2C1 = matrix.R1C2;
            result.R2C2 = matrix.R2C2;
        }

        /// <summary>
        /// Calculate the inverse of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to invert</param>
        /// <returns>The inverse of the given matrix if it has one, or the input if it is singular</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Matrix4 is singular.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 Invert(in Matrix3 mat)
        {
            Invert(mat, out var result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Matrix3 Invert()
        {
            Invert(this, out var result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invert(in Matrix3 m, out Matrix3 minv)
        {
            //Credit: https://stackoverflow.com/a/18504573

            var det = m.Determinant;
            if (MathHelper.CloseToPercent(det, 0))
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

            var invdet = 1 / det;

            minv.R0C0 = (float) ((m.R1C1 * m.R2C2 - m.R2C1 * m.R1C2) * invdet);
            minv.R0C1 = (float) ((m.R0C2 * m.R2C1 - m.R0C1 * m.R2C2) * invdet);
            minv.R0C2 = (float) ((m.R0C1 * m.R1C2 - m.R0C2 * m.R1C1) * invdet);
            minv.R1C0 = (float) ((m.R1C2 * m.R2C0 - m.R1C0 * m.R2C2) * invdet);
            minv.R1C1 = (float) ((m.R0C0 * m.R2C2 - m.R0C2 * m.R2C0) * invdet);
            minv.R1C2 = (float) ((m.R1C0 * m.R0C2 - m.R0C0 * m.R1C2) * invdet);
            minv.R2C0 = (float) ((m.R1C0 * m.R2C1 - m.R2C0 * m.R1C1) * invdet);
            minv.R2C1 = (float) ((m.R2C0 * m.R0C1 - m.R0C0 * m.R2C1) * invdet);
            minv.R2C2 = (float) ((m.R0C0 * m.R1C1 - m.R1C0 * m.R0C1) * invdet);
        }

        #endregion Functions

        #region Transformation Functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Transform(ref Vector3 vector)
        {
            Transform(this, ref vector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(in Matrix3 matrix, ref Vector3 vector)
        {
            var x = matrix.R0C0 * vector.X + matrix.R0C1 * vector.Y + matrix.R0C2 * vector.Z;
            var y = matrix.R1C0 * vector.X + matrix.R1C1 * vector.Y + matrix.R1C2 * vector.Z;
            vector.Z = matrix.R2C0 * vector.X + matrix.R2C1 * vector.Y + matrix.R2C2 * vector.Z;
            vector.X = x;
            vector.Y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(in Matrix3 matrix, ref Vector2 vector)
        {
            var x = matrix.R0C0 * vector.X + matrix.R0C1 * vector.Y + matrix.R0C2;
            vector.Y = matrix.R1C0 * vector.X + matrix.R1C1 * vector.Y + matrix.R1C2;
            vector.X = x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 Transform(Vector2 vector)
        {
            return Transform(this, vector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Box2 TransformBox(in Box2Rotated box)
        {
            return (box.Transform * this).TransformBox(box.Box);
        }

        public readonly Box2 TransformBox(in Box2 box)
        {
            // Do transformation on all 4 corners of the box at once.
            // Then min/max the results to get the new AABB.

            var boxVec = Unsafe.As<Box2, Vector128<float>>(ref Unsafe.AsRef(in box));

            // Convert box into list of X and Y values for each of the 4 corners
            var allX = Vector128.Shuffle(boxVec, Vector128.Create(0, 0, 2, 2));
            var allY = Vector128.Shuffle(boxVec, Vector128.Create(1, 3, 3, 1));

            // Transform coordinates
            var modX = allX * Vector128.Create(R0C0);
            var modY = allX * Vector128.Create(R1C0);
            modX += allY * Vector128.Create(R0C1);
            modY += allY * Vector128.Create(R1C1);
            modX += Vector128.Create(R0C2);
            modY += Vector128.Create(R1C2);

            // Get bounding box by finding the min and max X and Y values.
            var l = SimdHelpers.MinHorizontal128(modX);
            var b = SimdHelpers.MinHorizontal128(modY);
            var r = SimdHelpers.MaxHorizontal128(modX);
            var t = SimdHelpers.MaxHorizontal128(modY);

            var lbrt = SimdHelpers.MergeRows128(l, b, r, t);
            return Unsafe.As<Vector128<float>, Box2>(ref lbrt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transform(in Matrix3 matrix, Vector2 vector)
        {
            // TODO: Look at SIMD coz holy fak this is called a lot

            var x = matrix.R0C0 * vector.X + matrix.R0C1 * vector.Y + matrix.R0C2;
            var y = matrix.R1C0 * vector.X + matrix.R1C1 * vector.Y + matrix.R1C2;

            return new Vector2(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Transform(in Vector3 vector, out Vector3 result)
        {
            Transform(this, vector, out result);
        }

        /// <summary>
        /// Post-multiplies a 3x3 matrix with a 3x1 vector.
        /// </summary>
        /// <param name="matrix">Matrix containing the transformation.</param>
        /// <param name="vector">Input vector to transform.</param>
        /// <param name="result">Transformed vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(in Matrix3 matrix, in Vector3 vector, out Vector3 result)
        {
            result.X = matrix.R0C0 * vector.X + matrix.R0C1 * vector.Y + matrix.R0C2 * vector.Z;
            result.Y = matrix.R1C0 * vector.X + matrix.R1C1 * vector.Y + matrix.R1C2 * vector.Z;
            result.Z = matrix.R2C0 * vector.X + matrix.R2C1 * vector.Y + matrix.R2C2 * vector.Z;
        }

        /// <summary>
        /// Post-multiplies a 3x3 matrix with a 2x1 vector. The column-major 3x3 matrix is treated as
        /// a 3x2 matrix for this calculation.
        /// </summary>
        /// <param name="matrix">Matrix containing the transformation.</param>
        /// <param name="vector">Input vector to transform.</param>
        /// <param name="result">Transformed vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(in Matrix3 matrix, in Vector2 vector, out Vector2 result)
        {
            result.X = matrix.R0C0 * vector.X + matrix.R0C1 * vector.Y + matrix.R0C2;
            result.Y = matrix.R1C0 * vector.X + matrix.R1C1 * vector.Y + matrix.R1C2;
        }

        /// <summary>
        /// Pre-multiples a 1x3 vector with a 3x3 matrix.
        /// </summary>
        /// <param name="matrix">Matrix containing the transformation.</param>
        /// <param name="vector">Input vector to transform.</param>
        /// <param name="result">Transformed vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(in Vector3 vector, in Matrix3 matrix, out Vector3 result)
        {
            result.X = (vector.X * matrix.R0C0) + (vector.Y * matrix.R1C0) + (vector.Z * matrix.R2C0);
            result.Y = (vector.X * matrix.R0C1) + (vector.Y * matrix.R1C1) + (vector.Z * matrix.R2C1);
            result.Z = (vector.X * matrix.R0C2) + (vector.Y * matrix.R1C2) + (vector.Z * matrix.R2C2);
        }

        /// <summary>
        /// Pre-multiples a 1x2 vector with a 3x3 matrix. The row-major 3x3 matrix is treated as
        /// a 2x3 matrix for this calculation.
        /// </summary>
        /// <param name="matrix">Matrix containing the transformation.</param>
        /// <param name="vector">Input vector to transform.</param>
        /// <param name="result">Transformed vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(in Vector2 vector, in Matrix3 matrix, out Vector2 result)
        {
            result.X = (vector.X * matrix.R0C0) + (vector.Y * matrix.R1C0) + (matrix.R2C0);
            result.Y = (vector.X * matrix.R0C1) + (vector.Y * matrix.R1C1) + (matrix.R2C1);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rotate(Angle angle)
        {
            var sin = (float) Math.Sin(angle);
            var cos = (float) Math.Cos(angle);

            var r0c0 = cos * R0C0 + sin * R1C0;
            var r0c1 = cos * R0C1 + sin * R1C1;
            var r0c2 = cos * R0C2 + sin * R1C2;

            R1C0 = cos * R1C0 - sin * R0C0;
            R1C1 = cos * R1C1 - sin * R0C1;
            R1C2 = cos * R1C2 - sin * R0C2;

            R0C0 = r0c0;
            R0C1 = r0c1;
            R0C2 = r0c2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rotate(Angle angle, out Matrix3 result)
        {
            Rotate(this, angle, out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rotate(in Matrix3 matrix, Angle angle, out Matrix3 result)
        {
            var sin = (float) Math.Sin(angle);
            var cos = (float) Math.Cos(angle);

            result.R0C0 = cos * matrix.R0C0 + sin * matrix.R1C0;
            result.R0C1 = cos * matrix.R0C1 + sin * matrix.R1C1;
            result.R0C2 = cos * matrix.R0C2 + sin * matrix.R1C2;
            result.R1C0 = cos * matrix.R1C0 - sin * matrix.R0C0;
            result.R1C1 = cos * matrix.R1C1 - sin * matrix.R0C1;
            result.R1C2 = cos * matrix.R1C2 - sin * matrix.R0C2;
            result.R2C0 = matrix.R2C0;
            result.R2C1 = matrix.R2C1;
            result.R2C2 = matrix.R2C2;
        }
        #endregion Transformation Functions

        #region Operator Overloads

        /// <summary>
        /// Post-multiplies a 3x3 matrix with a 3x1 vector.
        /// </summary>
        /// <param name="matrix">Matrix containing the transformation.</param>
        /// <param name="vector">Input vector to transform.</param>
        /// <returns>Transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Matrix3 matrix, in Vector3 vector)
        {
            Transform(in matrix, in vector, out var result);
            return result;
        }

        /// <summary>
        /// Post-multiplies a 3x3 matrix with a 2x1 vector. The 3x3 matrix is treated as
        /// a 3x2 matrix for this calculation.
        /// </summary>
        /// <param name="matrix">Matrix containing the transformation.</param>
        /// <param name="vector">Input vector to transform.</param>
        /// <returns>Transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(in Matrix3 matrix, in Vector2 vector)
        {
            Transform(in matrix, vector, out var result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Vector3 vector, in Matrix3 matrix)
        {
            Transform(in vector, in matrix, out var result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(in Vector2 vector, in Matrix3 matrix)
        {
            Transform(in vector, in matrix, out var result);
            return result;
        }

        /// <summary>Multiply left matrix times right matrix.</summary>
        /// <param name="left">The matrix on the matrix side of the equation.</param>
        /// <param name="right">The matrix on the right side of the equation</param>
        /// <returns>The resulting matrix of the multiplication.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 operator *(in Matrix3 left, in Matrix3 right)
        {
            Multiply(in left, in right, out var result);
            return result;
        }

        #endregion

        #region Constants

        /// <summary>The identity matrix.</summary>
        public static readonly Matrix3 Identity = new(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        );

        /// <summary>A matrix of all zeros.</summary>
        public static readonly Matrix3 Zero = new(
            0, 0, 0,
            0, 0, 0,
            0, 0, 0
        );

        #endregion Constants

        #region HashCode

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public readonly override int GetHashCode()
        {
            return
                R0C0.GetHashCode() ^ R0C1.GetHashCode() ^ R0C2.GetHashCode() ^
                R1C0.GetHashCode() ^ R1C1.GetHashCode() ^ R1C2.GetHashCode() ^
                R2C0.GetHashCode() ^ R2C1.GetHashCode() ^ R2C2.GetHashCode();
        }

        #endregion HashCode

        #region String

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>A System.String containing left fully qualified type name.</returns>
        public readonly override string ToString()
        {
            return $"|{R0C0}, {R0C1}, {R0C2}|\n"
                   + $"|{R1C0}, {R1C1}, {R1C2}|\n"
                   + $"|{R2C0}, {R2C1}, {R2C2}|\n";
        }

        public readonly string ToString(string? format, IFormatProvider? formatProvider)
        {
            return ToString();
        }

        public readonly bool TryFormat(
            Span<char> destination,
            out int charsWritten,
            ReadOnlySpan<char> format,
            IFormatProvider? provider)
        {
            return FormatHelpers.TryFormatInto(
                destination,
                out charsWritten,
                $"|{R0C0}, {R0C1}, {R0C2}|\n"
                + $"|{R1C0}, {R1C1}, {R1C2}|\n"
                + $"|{R2C0}, {R2C1}, {R2C2}|\n");
        }

        #endregion String
    }
#pragma warning restore 3019
}
