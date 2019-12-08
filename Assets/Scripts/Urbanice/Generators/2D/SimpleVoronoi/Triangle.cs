using System;
using UnityEngine;

namespace Urbanice.Generators._2D.SimpleVoronoi
{
    public struct Triangle
    {
        public Vector2 P1;
        public Vector2 P2;
        public Vector2 P3;

        public Vector2 Center;
        public float Radius;

        public Triangle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            var s = Sign(p1, p2, p3);

            P1 = p1;
            // CCW
            P2 = s > 0 ? p2 : p3;
            P3 = s > 0 ? p3 : p2;

            var x1 = (p1.x + p2.x) / 2;
            var y1 = (p1.y + p2.y) / 2;
            var x2 = (p2.x + p3.x) / 2;
            var y2 = (p2.y + p3.y) / 2;

            var dx1 = p1.y - p2.y;
            var dy1 = p2.x - p1.x;
            var dx2 = p2.y - p3.y;
            var dy2 = p3.x - p2.x;

            var tg1 = Math.Abs(dx1) < float.Epsilon ? int.MaxValue : dy1 / dx1;
            var t2 = ((y1 - y2) - (x1 - x2) * tg1) /
                     (dy2 - dx2 * tg1);

            var cx = (p1.x + p2.x + p3.x) / 3f;
            var cy = (p1.y + p2.y + p3.y) / 3f;
            Center = new Vector2(x2 + dx2 * t2, y2 + dy2 * t2);
            Vector2 c2 = new Vector2(x2 + dx2 * t2, y2 + dy2 * t2);
            //Center = new Vector2(cx, cy);

            Radius = Vector2.Distance(Center, p1);
        }

        // checks if a point is within the triangle area
        public bool Contains(Vector2 pt)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(pt, P1, P2);
            d2 = Sign(pt, P2, P3);
            d3 = Sign(pt, P3, P1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }
        
        public bool HasCommonEdge(Vector2 a, Vector2 b)
        {
            return (P1 == a && P2 == b) ||
                   (P2 == a && P3 == b) ||
                   (P3 == a && P1 == b);
        }
        
        public static bool operator ==(Triangle obj1, Triangle obj2)
        {
            return obj1.P1 == obj2.P1 && obj1.P2 == obj2.P2 && obj1.P3 == obj2.P3;
        }

        public static bool operator !=(Triangle obj1, Triangle obj2)
        {
            return !(obj1 == obj2);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p2.x - p1.x) * (p2.y + p1.y) + (p3.x - p2.x) * (p3.y + p2.y) + (p1.x - p3.x) * (p1.y + p3.y);
        }
        
        public bool Equals(Triangle other)
        {
            return P1.Equals(other.P1) && P2.Equals(other.P2) && P3.Equals(other.P3) && Center.Equals(other.Center) && Radius.Equals(other.Radius);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Triangle other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = P1.GetHashCode();
                hashCode = (hashCode * 397) ^ P2.GetHashCode();
                hashCode = (hashCode * 397) ^ P3.GetHashCode();
                hashCode = (hashCode * 397) ^ Center.GetHashCode();
                hashCode = (hashCode * 397) ^ Radius.GetHashCode();
                return hashCode;
            }
        }
    }
}