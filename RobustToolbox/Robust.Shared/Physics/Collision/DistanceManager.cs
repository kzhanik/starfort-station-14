/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
*
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org
*
* This software is provided 'as-is', without any express or implied
* warranty.  In no event will the authors be held liable for any damages
* arising from the use of this software.
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
* 1. The origin of this software must not be misrepresented; you must not
* claim that you wrote the original software. If you use this software
* in a product, an acknowledgment in the product documentation would be
* appreciated but is not required.
* 2. Altered source versions must be plainly marked as such, and must not be
* misrepresented as being the original software.
* 3. This notice may not be removed or altered from any source distribution.
*/

using System;
using Robust.Shared.Maths;

namespace Robust.Shared.Physics.Collision
{

    internal static class DistanceManager
    {
        private const byte MaxGJKIterations = 20;

        public static void ComputeDistance(out DistanceOutput output, out SimplexCache cache, in DistanceInput input)
        {
            cache = new SimplexCache();

            /*
            if (Settings.EnableDiagnostics) //FPE: We only gather diagnostics when enabled
                ++GJKCalls;
            */

            // Initialize the simplex.
            Simplex simplex = new Simplex();
            simplex.ReadCache(ref cache, input.ProxyA, in input.TransformA, input.ProxyB, in input.TransformB);

            // These store the vertices of the last simplex so that we
            // can check for duplicates and prevent cycling.
            Span<int> saveA = stackalloc int[3];
            Span<int> saveB = stackalloc int[3];
            saveA.Clear();
            saveB.Clear();

            //float distanceSqr1 = Settings.MaxFloat;

            var vSpan = simplex.V.AsSpan;

            // Main iteration loop.
            int iter = 0;
            while (iter < MaxGJKIterations)
            {
                // Copy simplex so we can identify duplicates.
                int saveCount = simplex.Count;
                for (var i = 0; i < saveCount; ++i)
                {
                    saveA[i] = vSpan[i].IndexA;
                    saveB[i] = vSpan[i].IndexB;
                }

                switch (simplex.Count)
                {
                    case 1:
                        break;
                    case 2:
                        simplex.Solve2();
                        break;
                    case 3:
                        simplex.Solve3();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // If we have 3 points, then the origin is in the corresponding triangle.
                if (simplex.Count == 3)
                {
                    break;
                }

                //FPE: This code was not used anyway.
                // Compute closest point.
                //Vector2 p = simplex.GetClosestPoint();
                //float distanceSqr2 = p.LengthSquared();

                // Ensure progress
                //if (distanceSqr2 >= distanceSqr1)
                //{
                //break;
                //}
                //distanceSqr1 = distanceSqr2;

                // Get search direction.
                Vector2 d = simplex.GetSearchDirection();

                // Ensure the search direction is numerically fit.
                if (d.LengthSquared < float.Epsilon * float.Epsilon)
                {
                    // The origin is probably contained by a line segment
                    // or triangle. Thus the shapes are overlapped.

                    // We can't return zero here even though there may be overlap.
                    // In case the simplex is a point, segment, or triangle it is difficult
                    // to determine if the origin is contained in the CSO or very close to it.
                    break;
                }

                // Compute a tentative new simplex vertex using support points.
                SimplexVertex vertex = vSpan[simplex.Count];
                vertex.IndexA = input.ProxyA.GetSupport(Transform.MulT(input.TransformA.Quaternion2D, -d));
                vertex.WA = Transform.Mul(input.TransformA, input.ProxyA.Vertices[vertex.IndexA]);

                vertex.IndexB = input.ProxyB.GetSupport(Transform.MulT(input.TransformB.Quaternion2D, d));
                vertex.WB = Transform.Mul(input.TransformB, input.ProxyB.Vertices[vertex.IndexB]);
                vertex.W = vertex.WB - vertex.WA;
                vSpan[simplex.Count] = vertex;

                // Iteration count is equated to the number of support point calls.
                ++iter;

                /*
                if (Settings.EnableDiagnostics) //FPE: We only gather diagnostics when enabled
                    ++GJKIters;
                */

                // Check for duplicate support points. This is the main termination criteria.
                bool duplicate = false;
                for (int i = 0; i < saveCount; ++i)
                {
                    if (vertex.IndexA == saveA[i] && vertex.IndexB == saveB[i])
                    {
                        duplicate = true;
                        break;
                    }
                }

                // If we found a duplicate support point we must exit to avoid cycling.
                if (duplicate)
                {
                    break;
                }

                // New vertex is ok and needed.
                ++simplex.Count;
            }

            // Prepare output.
            simplex.GetWitnessPoints(out output.PointA, out output.PointB);
            output.Distance = (output.PointA - output.PointB).Length;
            output.Iterations = iter;

            // Cache the simplex.
            simplex.WriteCache(ref cache);

            // Apply radii if requested.
            if (input.UseRadii)
            {
                float rA = input.ProxyA.Radius;
                float rB = input.ProxyB.Radius;

                if (output.Distance > rA + rB && output.Distance > float.Epsilon)
                {
                    // Shapes are still no overlapped.
                    // Move the witness points to the outer surface.
                    output.Distance -= rA + rB;
                    Vector2 normal = output.PointB - output.PointA;
                    normal = normal.Normalized;
                    output.PointA += normal * rA;
                    output.PointB -= normal * rB;
                }
                else
                {
                    // Shapes are overlapped when radii are considered.
                    // Move the witness points to the middle.
                    Vector2 p = (output.PointA + output.PointB) * 0.5f;
                    output.PointA = p;
                    output.PointB = p;
                    output.Distance = 0.0f;
                }
            }
        }
    }
}
