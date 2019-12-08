using System;
using TMPro;
using UnityEngine;
using Urbanice.Utils;

namespace Urbanice.Data
{
    /// <summary>
    /// This class contains all functionality for half edges.
    /// A HalfEdge is a directional connection between two vertices
    /// </summary>
    [Serializable]
    public class HalfEdge
    {
        public static int HalfEdgeCount = 0;
        private int _index;
        private Vector2? _center;
        
        public int Index => _index;

        public Vertex Origin
        {
            get { return _origin; }
        }
        public Vertex Destination
        {
            get { return _destination; }
        }
        
        public HalfEdge PreviousEdge;
        public HalfEdge NextEdge;
        
        public Polygon AdjacentPolygon;

        public float Length => Vector2.Distance(Origin, Destination);

        private Vertex _origin;
        private Vertex _destination;

        /// <summary>
        /// Recalculates the center
        /// </summary>
        public void Update()
        {
            _center = new Vector2((Destination.x + Origin.x) / 2, (Destination.y + Origin.y) / 2);
        }
        public Vector2 Center
        {
            get
            {
                if (!_center.HasValue)
                    _center = new Vector2((Destination.x + Origin.x) / 2, (Destination.y + Origin.y) / 2);
                return _center.Value;
            }
            
        }
        private HalfEdge()
        {
            _index = ++HalfEdgeCount;
        }
        public HalfEdge(Vertex origin, Vertex destination): this()
        {
            _origin = origin;
            _origin.AddEdge(this);
            _destination = destination;
            _destination.AddEdge(this);
        }

        public HalfEdge(Vertex origin, Vertex destination, HalfEdge previous) : this(origin, destination)
        {
            PreviousEdge = previous;
            PreviousEdge.NextEdge = this;
        }
        public HalfEdge(Vertex origin, Vertex destination, HalfEdge previous, HalfEdge next) : this(origin, destination, previous)
        {
            NextEdge = next;
            NextEdge.PreviousEdge = this;
        }

        /// <summary>
        /// scales the edge by a certain amount from its center
        /// </summary>
        public void ScaleEdge(float scaleFactor)
        {
            var center = Center;
            var p1 = center + (scaleFactor) * (Origin - center);
            var p2 = center + (scaleFactor) * (Destination - center);
            Origin.ChangePosition(p1);
            Destination.ChangePosition(p2);

            Update();
        }
        /// <summary>
        /// Is the given point on the edge ?
        /// </summary>
        public bool IsPointOnEdge(Vector2 point)
        {
            return !(Math.Abs(MathUtils.Cross2D(this, point)) > float.Epsilon);
        }
        
        /// <summary>
        /// Warning! This does not inform the polygon about the new Edge. If the edge is part of a polygon use Polygon.SubdivideEdge() instead
        /// </summary>
        public HalfEdge SplitAt(float percentage, bool subDivideOther = false)
        {
            // value : 0 <= value <= 1f
            percentage = Mathf.Clamp01(percentage);

            var newPoint = Vertex.Factory.Create(GeometryUtils.GetPointOn(this, percentage));
            var newEdge = new HalfEdge(newPoint, Destination);

            newEdge.NextEdge = NextEdge;
            newEdge.PreviousEdge = this;
            newEdge.AdjacentPolygon = AdjacentPolygon;

            if (subDivideOther)
            {
                var otherEdge = Other();
                if (otherEdge != null)
                {
                    otherEdge.AdjacentPolygon.SubdivideEdge(otherEdge,1.0f - percentage);
                }
            }
            
            NextEdge = newEdge;
            ChangeDestination(newPoint);
            Update();

            return newEdge;
        }
        
        /// <summary>
        /// Removes the vertex and polygon references to this edge. GC will cleanup this edge afterwards
        /// </summary>
        public void Destroy()
        {
            Origin.RemoveEdge(this);
            Destination.RemoveEdge(this);

            if (AdjacentPolygon != null)
                AdjacentPolygon.Edges.Remove(this);
        }
        /// <summary>
        /// Changes the origin vertex of this edge
        /// </summary>
        private void ChangeOrigin(Vertex newOrigin)
        {
            _origin.RemoveEdge(this);
            _origin = newOrigin;
            newOrigin.AddEdge(this);
        }
        /// <summary>
        /// Changes the destination vertex of this edge
        /// </summary>
        private void ChangeDestination(Vertex newDestination)
        {
            _destination.RemoveEdge(this);
            _destination = newDestination;
            newDestination.AddEdge(this);
        }
        /// <summary>
        /// Flips the edge
        /// </summary>
        public void Flip()
        {
            var tmpVertex = Origin;

            ChangeOrigin(Destination);
            ChangeDestination(tmpVertex);

            var tmpEdge = NextEdge;
            NextEdge = PreviousEdge;
            PreviousEdge = tmpEdge;
        }
        /// <summary>
        /// Returns the twin half edge
        /// </summary>
        public HalfEdge Other()
        {
            foreach (var v in Origin.Edges)
            {
                if (v.Origin == Destination)
                {
                    return v;
                }
            }
            // Outer border found!
            return null;
        }
        
        public static bool operator ==(HalfEdge obj1, HalfEdge obj2)
        {
            if(ReferenceEquals(obj1, obj2)) return true;

            if(ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null)) return false;

            return obj1.Index == obj2.Index && obj1.Origin == obj2.Origin;
        }

        public static bool operator !=(HalfEdge obj1, HalfEdge obj2)
        {
            return !(obj1 == obj2);
        }
        
        protected bool Equals(HalfEdge other)
        {
            return _index == other._index;
        }

        public static HalfEdge CreateOrReturnOther(HalfEdge edge)
        {
            if (edge == null)
                return null;
            var other = edge.Other();
            if (other != null)
                return other;
            
            return new HalfEdge(edge.Destination, edge.Origin);
        }
    }
}