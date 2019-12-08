using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;

namespace Urbanice.Utils
{
    public static class Vector2Extensions
    {
        public static Vector2 MovePointInDirectionOfPoint(this Vector2 origin, Vector2 targetPoint, float distance = 0.5f)
        {
            var direction = targetPoint - origin;
            return origin + direction * distance;
        }

        public static Vector2? FindClosestIn(this Vector2 p, List<Vector2> list)
        {
            var closestDistance = float.MaxValue;
            var closestVertex = new Vector2?();

            foreach (var t in list)
            {
                var currentDistance = Vector2.Distance(p, t);
                if (!(currentDistance < closestDistance)) continue;
                
                closestDistance = currentDistance;
                closestVertex = t;
            }

            return closestVertex == default(Vector2) ? null : closestVertex;
        }
        
        public static Vector2? FindClosestIn(this Vector2 p, List<Vertex> list)
        {
            var closestDistance = float.MaxValue;
            var closestVertex = new Vector2?();

            foreach (var t in list)
            {
                var currentDistance = Vector2.Distance(p, t);
                if (!(currentDistance < closestDistance)) continue;
                
                closestDistance = currentDistance;
                closestVertex = t;
            }

            return closestVertex != default(Vector2) ? null : closestVertex;
        }
    }
}