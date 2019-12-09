using System;
using System.Collections.Generic;
using UnityEngine;
using Urbanice.Generators._1D.Random;
using Urbanice.Module;

namespace Urbanice.Data
{
    /// <summary>
    /// A vertex is a point at a unique location
    /// </summary>
    [Serializable]
    public class Vertex
    {
        /// <summary>
        /// This is the factory class for vertices. Never create a vertex on your own!
        /// </summary>
        public class Factory
        {
            private static Dictionary<Vector2, Vertex> _positionPointMap = new Dictionary<Vector2, Vertex>();

            /// <summary>
            /// this method needs to be called before a city gets generated because Unity sometimes does serialize the data
            /// of the dictionary
            /// </summary>
            public static void ClearData()
            {
                _positionPointMap = new Dictionary<Vector2, Vertex>();
            }
            public static Vertex Create(Vector2 position)
            {
                if (!_positionPointMap.TryGetValue(position, out Vertex v))
                {
                    v = new Vertex(position);
                    _positionPointMap[position] = v;
                }

                return v;
            }
            public static Vertex Create(float x, float y)
            {
                return Create(new Vector2(x,y));
            }

            public static Vertex GetOrCreateVertexWithinRange(Vector2 position, float maxRange)
            {
                float closestDistance = maxRange;
                Vertex closestVertex = null;
                
                foreach (var pos in _positionPointMap.Keys)
                {
                    var distance = Vector2.Distance(pos, position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestVertex = _positionPointMap[pos];
                    }

                    if (closestDistance <= float.Epsilon)
                    {
                        break;
                    }
                }

                if (closestVertex == null)
                {
                    return Create(position);
                }
                return closestVertex;
            }
        }
        public static int VertexCount = 0;
        private int _index;

        // Getter of the index id
        public int Index => _index;

        // x and y position of the vertex
        public float x;
        public float y;

        // ALL conntected edges, this could be optimized to only hold the outgoing edges
        private List<HalfEdge> _edges;

        // Getter of the edges
        public List<HalfEdge> Edges => new List<HalfEdge>(_edges);

        protected Vertex()
        {
            _index = ++VertexCount;
            _edges = new List<HalfEdge>();
        }
        protected Vertex(Vector2 position) : this(position.x, position.y)
        {
        }

        protected Vertex(double x, double y) : this((float) x, (float) y)
        {
        }

        protected Vertex(float x, float y) : this()
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Method to connect an edge to this vertex. Never add an edge directly to the list
        /// </summary>
        public void AddEdge(HalfEdge edge)
        {
            if(!_edges.Contains(edge)) _edges.Add(edge);
        }
        public void RemoveEdge(HalfEdge edge)
        {
            if (!_edges.Remove(edge))
            {
                Debug.LogWarning($"This edge {edge.Index} cannot be removed from Vertex {_index} because it's not in the list'");
            }
        }
        /// <summary>
        /// Modifies the position of the vertex
        /// </summary>
        /// <param name="newPosition"></param>
        public void ChangePosition(Vector2 newPosition)
        {
            x = newPosition.x;
            y = newPosition.y;
        }
        /// <summary>
        /// Finds and returns the closest vertex in a list of vertices. it's possible to exclude certain vertices of the list
        /// </summary>
        public bool FindClosestIn(List<Vertex> list, out Vertex closest, params Vertex[] exclude)
        {
            var closestDistance = float.MaxValue;
            closest = null;
            List<Vertex> copy = new List<Vertex>(list);
            if (exclude != null)
            {
                foreach (var p in exclude)
                {
                    copy.Remove(p);
                }
            }

            foreach (var t in copy)
            {
                var currentDistance = Vector2.Distance(this, t);
                if (!(currentDistance < closestDistance)) continue;

                closestDistance = currentDistance;
                closest = t;
            }

            return closest != null;
        }

        public bool FindClosestIn(List<HalfEdge> list, out Vertex closest, params Vertex[] exclude)
        {
            var vertexList = new List<Vertex>();
            foreach (var edge in list)
            {
                vertexList.Add(edge.Origin);
            }

            return FindClosestIn(vertexList, out closest, exclude); 
        }
        
        /// <summary>
        /// Returns a random vertex of the given list within distance to this vertex
        /// </summary>
        public Vertex GetRandomPointWithinDistance(List<Vertex> list, float maxDistance)
        {
            var list1 = new List<Vertex>();

            foreach (var p in list)
            {
                if(Vector2.Distance(this, p) > maxDistance)
                    continue;
                
                list1.Add(p);
            }

            var index = (int) (GlobalPRNG.Next() * list1.Count);
            return index >= list1.Count ? null : list1[index];
        }

        /// <summary>
        /// Returns all polygons which contain this vertex
        /// </summary>
        public List<Polygon> GetConnectedRegions()
        {
            var regions = new List<Polygon>();
            
            foreach (var edge in Edges)
            {
                if (edge.Origin != this)
                {
                    // Only get outgoing edges, otherwise we would need to filter out duplicates
                    continue;
                }

                regions.Add(edge.AdjacentPolygon);
            }

            return regions;
        }
        /// <summary>
        /// Returns the which connects this vertex to the given vertex, if there is any
        /// </summary>
        public bool GetEdgeTo( Vertex destination, out HalfEdge edge)
        {
            foreach (var e in Edges)
            {
                if (e.Destination != destination)
                    continue;
                
                edge = e;
                return true;
            }

            edge = default(HalfEdge);
            return false;
        }
        
        public static implicit operator Vector2(Vertex p)
        {
            return new Vector2(p.x, p.y);
        }
        public static implicit operator Vector3(Vertex p)
        {
            return new Vector3(p.x, p.y, 0);
        }
        
        public override bool Equals(object obj)
        {
            return obj != null && base.Equals(obj);
        }
        public static bool operator ==(Vertex obj1, Vertex obj2)
        {
            if(ReferenceEquals(obj1, obj2)) return true;
            if(ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null)) return false;
            
            return obj1.Index == obj2.Index;
        }

        public static bool operator !=(Vertex obj1, Vertex obj2)
        {
            return !(obj1 == obj2);
        }

        public static Vector2 operator +(Vertex p1, Vertex p2)
        {
            return new Vector2(p1.x + p2.x, p1.y + p2.y);
        }

        public static Vector2 operator +(Vertex p1, Vector2 p2)
        {
            return new Vector2(p1.x + p2.x, p1.y + p2.y);
        }

        public static Vector2 operator -(Vertex p1, Vertex p2)
        {
            return new Vector2(p1.x - p2.x, p1.y - p2.y);
        }

        public static Vector2 operator -(Vertex p1, Vector2 p2)
        {

            return new Vector2(p1.x - p2.x, p1.y - p2.y);
        }

        public static Vector2 operator *(Vertex p1, float s)
        {
            return new Vector2(p1.x, p1.y) * s;
        }

        public static bool operator ==(Vector2 obj1, Vertex obj2)
        {
            return obj2 == obj1;
        }
        public static bool operator !=(Vector2 obj1, Vertex obj2)
        {
            return !(obj2 == obj1);
        }
        
        public static bool operator ==(Vertex obj1, Vector2 obj2)
        {
            return Math.Abs(obj1.x - obj2.x) < float.Epsilon && Math.Abs(obj1.y - obj2.y) < float.Epsilon;
        }
        public static bool operator !=(Vertex obj1, Vector2 obj2)
        {
            return !(obj1 == obj2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Index;
                hashCode = (hashCode * 397) ^ x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                return hashCode;
            }
        }
    }
}
