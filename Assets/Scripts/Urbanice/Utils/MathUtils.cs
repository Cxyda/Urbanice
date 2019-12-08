using System;
using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._1D.Random;

namespace Urbanice.Utils
{
    public static class MathUtils
    {
        public static float Cross2D(HalfEdge e, Vector2 p)
        {
            var dxc = p.x - e.Origin.x;
            var dyc = p.y - e.Origin.y;

            var dxl = e.Destination.x - e.Origin.x;
            var dyl = e.Destination.y - e.Origin.y;

            return dxc * dyl - dyc * dxl;
        }
        public static List<Vector2> GeneratePointsAround(Vector2 point, int pointAmount, float maxRadius, float minDistance = 0f)
        {
            var points = new List<Vector2>(pointAmount);
            var sa = GlobalPRNG.Next() * 2 * Mathf.PI;

            var cnt = 0;
            do
            {
                var a = sa + Mathf.Sqrt(points.Count) * (3 + 2* GlobalPRNG.Next());

                var r = maxRadius * GlobalPRNG.Next();
                var x = Mathf.Cos(a) * r + point.x;
                var y = Mathf.Sin(a) * r + point.y;

                var v = new Vector2(x, y);
                if (Mathf.Abs(minDistance) > float.Epsilon)
                {
                    var closestPoint = v.FindClosestIn(points);
                    if (closestPoint.HasValue && Vector2.Distance(v, closestPoint.Value) < minDistance)
                    {
                        cnt++;
                        continue;
                    }
                }

                points.Add(v);
                
            } while (cnt < pointAmount * 10 && points.Count < pointAmount );


            return points;
        }
        
        public static int Sign(float f)
        {
            return Math.Abs(f) < float.Epsilon ? 0 : (f < 0 ? -1 : 1);
        }
        
        /// <summary>
        /// Returns the modulo of x, respecting negative x as well
        /// </summary>
        public static int Mod(int x, int d)
        {
            if (x >= 0)
                return x % d;
            return (x % d + d) % d;
        }
    }
}