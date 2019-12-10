using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Urbanice.Generators._2D.SimpleVoronoi;
using Urbanice.Utils;

namespace Urbanice.Data
{
    /// <summary>
    /// A polygon is a Ngon shape which has n vertices and n HalfEdges
    /// </summary>
    public class Polygon
    {
        public static int PolygonCount = 0;
        private int _index;

        public int Index => _index;
        public Vector2 Center => _center;
        public double Area => _area;

        /// <summary>
        /// Getter of the containing vertices. The list is cached once the getter was called
        /// </summary>
        public List<Vertex> Points
        {
            get
            {
                if (_points.Count == Edges.Count)
                   return _points;
                _points = new List<Vertex>(Edges.Count);
                foreach (var e in Edges)
                {
                    _points.Add(e.Origin);
                }

                return _points;
            }
        }
        // All edges of this polygon
        public List<HalfEdge> Edges;
        // All triangles of this polygon. The triangles always connect the center with two vertices on the edge
        public List<Triangle> Triangles;

        // The bounding box (rectangle) of the shape
        public Rect BoundingBox;
        
        // The center position
        private Vector2 _center;
        // The area of the surface
        private double _area;
        private List<Vertex> _points;

        // the closest distance of two vertices of the border
        public float MinRadius
        {
            get { 
                float minRadius = float.MaxValue;
                foreach (var tr in Triangles)
                {
                    if (tr.Radius < minRadius) minRadius = tr.Radius;
                }
    
                return minRadius;
            }
        }
        // the farthest distance of two vertices of the border
        public float MaxRadius
        {
            get { 
                float maxRadius = 0f;
                foreach (var pt in Points)
                {
                    float distance = Vector2.Distance(pt, _center);
                    if (distance > maxRadius) maxRadius = distance;
                }
    
                return maxRadius;
            }
        }

        private Polygon()
        {
            _index = ++PolygonCount;
            _area = 0f;
        }

        /// <summary>
        /// Clones, flips and insets a given polygon to return a new polygon which is smaller and the edges are flipped
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="insetAmount"></param>
        /// <returns></returns>
        public static Polygon CloneInsetAndFlip(Polygon shape, float insetAmount)
        {
            var edges = new List<HalfEdge>();
            Vertex lastVertex = Vertex.Factory.Create(InsetPoint(shape.Edges[0].Origin, shape.Center));

            for(var n = 0; n < shape.Edges.Count; n++)
            {
                var n1 = (n + 1) % shape.Edges.Count;
                
                var e = shape.Edges[n1];
                var iV1 = Vertex.Factory.Create(InsetPoint(e.Origin, shape.Center));

                var newEdge = new HalfEdge(lastVertex,iV1);
                
                edges.Add(newEdge);
                if (n > 0)
                {
                    newEdge.PreviousEdge = edges[n-1];
                    edges[n-1].NextEdge = newEdge;
                }

                lastVertex = iV1;
            }
            // Connect the missing edges
            edges[0].PreviousEdge = edges[edges.Count-1];
            edges[edges.Count-1].NextEdge = edges[0];
            // flip all edges
            foreach (var edge in edges)
            {
                edge.Flip();
            }
            // And reverse the edge list
            edges.Reverse();
            return new Polygon(edges);
            
            Vector2 InsetPoint(Vector2 point, Vector2 insetPoint)
            {
                var dir = insetPoint - point;
                dir.Normalize();
                return point + dir * insetAmount;
            }
        }

        public Polygon(List<HalfEdge> edges) : this()
        {
            Edges = edges;
            CreatePolygon();
        }

        /// <summary>
        /// Destroys the polygon and clears the references
        /// </summary>
        public void Destroy()
        {
            foreach (var e in Edges)
            {
                e.Origin.RemoveEdge(e);
                e.Destination.RemoveEdge(e);
            }
            Edges.Clear();
        }

        /// <summary>
        /// Constructs the polygon from the given edges
        /// </summary>
        private void CreatePolygon()
        {
            List<Vector2> points = new List<Vector2>();
            bool flipEdges = false;
            foreach (var edge in Edges)
            {
                if(!flipEdges)
                {
                    foreach (var connectedEdge in edge.Origin.Edges)
                    {
                        if (edge == connectedEdge)
                            continue;
                        if (edge.Destination != connectedEdge.Destination) continue;
                        
                        // wrong direction found! -> flip it !
                        flipEdges = true;
                        break;
                    }
                }

                edge.AdjacentPolygon = this;
                points.Add(edge.Origin);
            }

            if (flipEdges)
            {
                foreach (var e in Edges)
                {
                    e.Flip();
                }

                Edges.Reverse();
            }

            _points = new List<Vertex>(Edges.Count);

            BoundingBox = GeometryUtils.CalculateBoundingBox(points);
            Update();
        }

        public bool FindCommonEdge(List<HalfEdge> edges, out HalfEdge commonEdge, out bool isFlipped)
        {
            isFlipped = false;
            commonEdge = null;
            
            foreach (var e in edges)
            {
                int originIndex = Points.IndexOf(e.Origin);
                int destinationIndex = Points.IndexOf(e.Destination);
                
                if(originIndex < 0 || destinationIndex < 0)
                    continue;
                
                // common edge found! check orientation
                isFlipped = originIndex - destinationIndex < 0;
                commonEdge = Edges[originIndex];
                return true;
            }

            return false;
        }
        public void ExtendPolygon(List<HalfEdge> edges)
        {
            List<HalfEdge> listCopy = new List<HalfEdge>(Edges);
            if (!FindCommonEdge(edges, out HalfEdge commonEdge, out bool isFlipped))
            {
                return;
            }
            int commonEdgeIndex = Edges.IndexOf(commonEdge);

            if (isFlipped)
            {
                foreach (var e in edges)
                {
                    e.Flip();
                }

                edges.Reverse();
            }

            listCopy.Remove(commonEdge);
            edges.Remove(commonEdge.Other());
            
            commonEdge.Destroy();
            commonEdge.Other().Destroy();
            
            foreach (var e in edges)
            {
                e.AdjacentPolygon = this;
            }

            if (commonEdgeIndex == listCopy.Count)
            {
                listCopy.AddRange(edges);
            }
            else
            {
                listCopy.InsertRange(commonEdgeIndex, edges);
            }
            GeometryUtils.ConnectEdgesInOrder(listCopy);
            Edges = listCopy;
            Update();
        }

        private void CreateTriangles()
        {
            Triangles = new List<Triangle>();

            foreach (var e in Edges)
            {
                var tr = new Triangle(Center, e.Origin, e.Destination);
                Triangles.Add(tr);
            }
        }
        /// <summary>
        /// Recalculates the polygon
        /// </summary>
        public void Update()
        {
            RecalculateArea();
            RecalculateCenter();
            CreateTriangles();
        }
        /// <summary>
        /// Returns all neighboring polygons which share an edge with this polygon
        /// </summary>
        public List<Polygon> GetNeighbors()
        {
            var neighbors = new List<Polygon>();

            foreach (var edge in Edges)
            {
                var otherEdge = edge.Other();
                if (otherEdge == null || otherEdge.AdjacentPolygon == null)
                    continue;

                neighbors.Add(otherEdge.AdjacentPolygon);
            }
            return neighbors;
        }

        /// <summary>
        ///  Does this polygon surface contain the given point?
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool Contains(Vector2 pt)
        {
            foreach (var tr in Triangles)
            {
                var contains = tr.Contains(pt);
                if (contains)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void RecalculateArea()
        {

            _area = 0;

            for (var n = 0; n < Points.Count; n++)
            {
                var n1 = (n + 1) % Points.Count;
                _area += Points[n].x * Points[n1].y - Points[n1].x * Points[n].y;
            }

            _area = Math.Abs(_area * 0.5);
        }

        public void RecalculateCenter()
        {
            double xs = 0f;
            double ys = 0f;
            
            for (var n = 0; n < Points.Count; n++)
            {
                var n1 = (n + 1) % Points.Count;
                xs += (Points[n].x + Points[n1].x) * (Points[n].x * Points[n1].y - Points[n1].x * Points[n].y);
                ys += (Points[n].y + Points[n1].y) * (Points[n].x * Points[n1].y - Points[n1].x * Points[n].y);
            }

            xs = Math.Abs(xs * 1 / (6 * _area));
            ys = Math.Abs(ys / (6 * _area));

            _center = new Vector2((float)xs, (float)ys);
        }

        /// <summary>
        /// Returns the normalized bisectrix vector of two edges connected to this edge
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector2 GetBisectrix(Vertex p)
        {
            for (int n = 0; n < Edges.Count; n++)
            {
                if(Edges[n].Origin != p)
                    continue;

                Vector2 p0 = Edges[n].Destination - p;
                Vector2 p1 = Edges[n].PreviousEdge.Origin - p;
                return (p0.normalized + p1.normalized).normalized;
            }
            // Point is not a polygon point, return center direction for now
            return Center - p;
        }

        /// <summary>
        /// Subdivides all Edges of this polygon at the given length and subdivides its neighbors as well
        /// </summary>
        public void SubdivideAllEdgesWith(float maxBorderLength, bool subdivideNeighbors)
        {
            var clonedEdges = new List<HalfEdge>(Edges);
            foreach (var edge in clonedEdges)
            {
                float edgeLength = edge.Length;
                if(edgeLength > maxBorderLength)
                    SubdivideEdge(edge, .5f, subdivideNeighbors);
            }
        }
        /// <summary>
        /// Subdivides all edges within a 0 to 1 range
        /// </summary>
        public void SubdivideAllEdgesAt(float value, bool subdivideNeighbors)
        {
            var clonedEdges = new List<HalfEdge>(Edges);
            foreach (var edge in clonedEdges)
            {
                SubdivideEdge(edge, value, subdivideNeighbors);
            }
        }
        public void SubdivideEdge(HalfEdge edge, float value, bool splitOther = false)
        {
            var index = Edges.IndexOf(edge);
            if (index < 0)
                return;
            var e = Edges[index].SplitAt(value, splitOther);
            
            if (index != Edges.Count - 1)
            {
                Edges.Insert(index + 1, e);
            }
            else
            {
                Edges.Add(e);
                //Points.Add(e.Origin);
            }
            // TODO: This could be optimized to only recreate triangles of this edge
            CreateTriangles();
        }
        public List<Vertex> CreateVerticesWithin(int maxAmount, float minPointDistance)
        {
            var innerPoints = new List<Vertex>();

            var randomPoints = MathUtils.GeneratePointsAround(Center, maxAmount, MaxRadius, minPointDistance);
            foreach (var p in randomPoints)
            {
                if(Contains(p))
                    innerPoints.Add(Vertex.Factory.Create(p));
            }

            return innerPoints;
        }
        
        public static bool operator ==(Polygon obj1, Polygon obj2)
        {
            if(ReferenceEquals(obj1, obj2)) return true;
            if(ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null)) return false;
            
            return obj1.Index == obj2.Index;
        }

        public static bool operator !=(Polygon obj1, Polygon obj2)
        {
            return !(obj1 == obj2);
        }

        public bool IsBorderPolygon()
        {
            foreach (var edge in Edges)
            {
                if (edge.Other() == null)
                    return true;
            }

            return false;
        }
    }
}