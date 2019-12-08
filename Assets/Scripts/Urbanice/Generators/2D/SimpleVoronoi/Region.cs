using System;
using System.Collections.Generic;
using UnityEngine;
using Urbanice.Utils;

namespace Urbanice.Generators._2D.SimpleVoronoi
{
    public struct Region
    {
        public bool IsClosed;
        
        public List<Triangle> Triangles;
        public Vector2 Site;
        
        public Region(Vector2 p, List<Triangle> triangles)
        {
            IsClosed = false;
            Site = p;
            Triangles = triangles;
        }

        public Vector2 Center()
        {
            var c = new Vector2();
            foreach (var t in Triangles)
            {
                c += t.Center;
            }

            c *= 1f / Triangles.Count;
            return c;
        }

        public Region SortTriangles()
        {
            var cachedRegion = this;
            cachedRegion.Triangles.Sort((t1, t2) =>
            {
                var x1 = t1.Center.x - cachedRegion.Site.x;
                var y1 = t1.Center.y - cachedRegion.Site.y;
                var x2 = t2.Center.x - cachedRegion.Site.x;
                var y2 = t2.Center.y - cachedRegion.Site.y;

                if (x1 >= 0 && x2 < 0) return 1;
                if (x2 >= 0 && x1 < 0) return -1;
                if (Math.Abs(x1) < float.Epsilon && Math.Abs(x2) < float.Epsilon)
                    return y2 > y1 ? 1 : -1;

                return MathUtils.Sign( x2 * y1 - x1 * y2 );
            });
            Triangles = cachedRegion.Triangles;
            return this;
        }
    }
}