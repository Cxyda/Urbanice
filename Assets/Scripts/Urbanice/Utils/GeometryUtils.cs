using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._1D.Random;

namespace Urbanice.Utils
{
    /// <summary>
    /// This Utility class provides shared functionality regarding geoemetry operations
    /// </summary>
    public static class GeometryUtils
    {
        public static List<Polygon> RemoveOutsidePolygons(List<Polygon> district, Polygon shape)
        {
            if(district.Count == 0)
            {
                return district;
            }

            for (int i = district.Count-1; i >= 0; i--)
            {
                var p = district[i];
                if (!shape.BoundingBox.Contains(p.BoundingBox))
                {
                    district.Remove(p);
                    p.Destroy();
                }
            }
   
            for (int i = district.Count-1; i >= 0; i--)
            {
                var p = district[i];

                var containsMin = shape.Contains(p.BoundingBox.min);
                var containsMax = shape.Contains(p.BoundingBox.max);
                if (!(containsMin && containsMax))
                {
                    p.Destroy();
                    district.Remove(p);
                    continue;
                }

                var containsTL = shape.Contains(p.BoundingBox.TopLeft());
                var containsTR = shape.Contains(p.BoundingBox.BottomRight());

                if (containsTL && containsTR) continue;
                
                // Check if all points are in shape
                foreach (var point in p.Points)
                {
                    if (!shape.Contains(point))
                    {
                        district.Remove(p);
                        p.Destroy();
                    }
                }
            }

            return district;
        }
        public static bool GetLineIntersection(Vector2 p0, Vector2 p1, 
            Vector2 p2, Vector2 p3, out Vector2 i)
        {
            float s1_x, s1_y, s2_x, s2_y;
            s1_x = p1.x - p0.x;     s1_y = p1.y - p0.y;
            s2_x = p3.x - p2.x;     s2_y = p3.y - p2.y;

            float s, t;
            s = (-s1_y * (p0.x - p2.x) + s1_x * (p0.y - p2.y)) / (-s2_x * s1_y + s1_x * s2_y);
            t = ( s2_x * (p0.y - p2.y) - s2_y * (p0.x - p2.x)) / (-s2_x * s1_y + s1_x * s2_y);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                // Collision detected
                i = new Vector2(p0.x + (t * s1_x), p0.y + (t * s1_y));

                return true;
            }

            i = Vector2.positiveInfinity;
            
            return false;
        }

        
        public static Vector2 BisectSegment(Vector2 p0, Vector2 p1, float maxDeviation = 0f)
        {
            Vector2 vector = p1 - p0;
            Vector2 normal = GetNormalVector(vector);
            
            float deviationDirection = MathUtils.Sign((GlobalPRNG.Next() - 0.5f) * 2);
            var deviation = GlobalPRNG.Next() * maxDeviation * deviationDirection;

            Vector2 bisector = p0 + vector * 0.5f + deviation * vector.magnitude * normal;
            return new Vector2(bisector.x, bisector.y);
        }

        public static Vector2 GetNormalVector(Vector2 dir)
        {
            var v = Vector3.Cross(dir.normalized, Vector3.back).normalized;
            return v;
        }

        public static Vector2 GetRandomPointBetween(Vector2 p0, Vector2 p1)
        {
            return GetPointBetween(p0, p1, GlobalPRNG.Next());
        }
        public static Vector2 GetPointBetween(Vector2 p0, Vector2 p1, float value)
        {
            // value : 0 <= value <= 1f
            var direction = (p1 - p0);
            return p0 + direction * value;
        }
        public static Vector2 GetPointOn(HalfEdge e)
        {
            return GetRandomPointBetween(e.Origin, e.Destination);
        }

        public static Vector2 GetPointOn(HalfEdge edge, float value)
        {
            return GetPointBetween(edge.Origin, edge.Destination, value);
        }

        public static Vector2 CreateLineSegmentInDirection(Vector2 p0, Vector2 direction, float segmentLength,
            float deviation = 0f)
        {

            direction.Normalize();
            
            var p1 = p0 + direction * segmentLength;
            if (!(deviation > 0f)) return p1;
            
            var normal = GetNormalVector(direction);
            float deviationDirection = MathUtils.Sign((GlobalPRNG.Next() - 0.5f) * 2);

            var deviationVector = normal * deviation * GlobalPRNG.Next() * segmentLength;

            p1 += deviationVector * deviationDirection;
            return p1;
        }
        public static Vector2 CreateLineSegmentTowardsPoint(Vector2 p0, Vector2 pn, float segmentLength,
            float deviation = 0f)
        {
            var direction = pn - p0;
            
            var distance = direction.magnitude;
            var p1 = CreateLineSegmentInDirection(p0, direction, segmentLength, deviation);

            return p1;
        }
        
        public static List<Vector2> CreateLineTowardsPoint(Vector2 start, Vector2 tp, float maxSegmentLength, float deviation)
        {
            var line = new List<Vector2>();
            
            line.AddMultiple(start, tp);
            var distance = Vector2.Distance(start, line[1]);

            while (distance > maxSegmentLength)
            {
                line = SubdivideLine(line, deviation);
                distance = Vector2.Distance(start, line[1]);
            }
            
            return line;
        }

        public static List<Vector2> SubdivideLineSegment(Vector2 start, Vector2 end, int subdivs)
        {
            var line = new List<Vector2>(2 + subdivs) {start} ;

            var distance = Vector2.Distance(start, end);
            var direction = end - start;
            direction.Normalize();
            var segmentLength = distance / (subdivs+1);

            for (var i = 1; i <= subdivs + 1; i++)
            {
                Vector2 p = start + i * segmentLength * direction;
                line.Add(p);
            }

            return line;
        }
        public static List<Vector2> SubdivideLine(List<Vector2> line, float maxDeviation = 0f)
        {
            if (line.Count == 0)
            {
                throw new Exception("List must not be empty");
            }
            var subdividedLine = new List<Vector2>();
            for (var n = 1; n < line.Count; n++)
            {
                Vector2 p0 = line[n - 1];
                Vector2 p1 = line[n];
                
                Vector2 bisector = BisectSegment(p0, p1, maxDeviation);
                
                subdividedLine.AddMultiple(p0, bisector);
            }

            subdividedLine.Add(line[line.Count - 1]);

            return subdividedLine;
        }

        public static Vector2? WeldPointsIfInRange(Vector2 p1, Vector2 p2, float range)
        {
            if (Vector2.Distance(p1, p2) <= range)
            {
                // weld vertices at their midpoint
                return BisectSegment(p1, p2);
            }

            return null;
        }

        public static List<Vertex> WeldVerticesWithinRange(List<Vertex> list, float weldDistance)
        {
            List<Vertex> verticesToRemove = new List<Vertex>();
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[i] == list[j])
                    {
                        continue;
                    }

                    if (Vector2.Distance(list[i], list[j]) <= weldDistance)
                    {
                        var bisector = BisectSegment(list[i], list[j]);
                        list[i].x = bisector.x;
                        list[i].y = bisector.y;

                        list[j].x = bisector.x;
                        list[j].y = bisector.y;
                        
                        if(!verticesToRemove.Contains(list[j]))
                            verticesToRemove.Add(list[j]);
                    }
                }
            }

            foreach (var v in verticesToRemove)
            {
                list.Remove(v);
            }

            return list;
        }

        public static List<List<Vector2>> WeldLinepointsWithinRange(List<List<Vector2>> lines, float range)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                for (int j = 0; j < lines[i].Count; j++)
                {
                    Vector2 p1 = lines[i][j];
                    for (int k = i+1; k < lines.Count; k++)
                    {
                        for (int l = 0; l < lines[k].Count; l++)
                        {
                            Vector2 p2 = lines[k][l];
                            
                            if (p1 == p2)
                            {
                                continue;
                            }

                            var weldedVertex = WeldPointsIfInRange(p1, p2, range);
                            if (weldedVertex.HasValue)
                            {
                                lines[i][j] = weldedVertex.Value;
                                lines[k][l] = weldedVertex.Value;
                            }
                        }
                    }
                }

            }

            return lines;
        }
        
        public static Rect CalculateBoundingBox(List<Vector2> points)
        {
            CalculateBounds(points, out float minx, out float miny, out float maxx, out float maxy);
            //var dx = (maxx - minx) * 0.5f;
            //var dy = (maxy - miny) * 0.5f;
            
            return new Rect(new Vector2(minx, miny), new Vector2(maxx-minx, maxy-miny));
        }

        public static void CalculateBounds(List<Vector2> points, out float xMin, out float yMin, out float xMax,
            out float yMax)
        {
            xMin = float.MaxValue;
            yMin = float.MaxValue;
            xMax = float.MinValue;
            yMax = float.MinValue;
            
            foreach (var p in points)
            {
                if (p.x < xMin) xMin = p.x;
                if (p.y < yMin) yMin = p.y;
                if (p.x > xMax) xMax = p.x;
                if (p.y > yMax) yMax = p.y;
            }
        }

        public static List<Vertex> EdgesToVertices(List<HalfEdge> edgelist)
        {
            var vertices = new List<Vertex>();

            for (int i = 1; i < edgelist.Count; i++)
            {
                vertices.Add(edgelist[i].Origin);
            }
            vertices.Add(edgelist[edgelist.Count-1].Destination);
            vertices.TrimExcess();
            return vertices;
        }
        public static List<HalfEdge> FindBorderEdges(List<Polygon> polygons)
        {
            List<HalfEdge> borderEdge = new List<HalfEdge>(); 
            HalfEdge start = null;
            foreach (var p in polygons)
            {
                foreach (var edge in p.Edges)
                {
                    var other = edge.Other();
                    if (other != null) continue;
                    
                    start = edge;
                    break;
                }

                if (start != null)
                    break;
            }
            if(start == null)
                throw new Exception("Cannot find border edge");
            
            // traverse around polygons
            HalfEdge currentEdge = start;
            do
            {
                Vertex v = currentEdge.Destination;
                bool foundNextBorder = false;
                foreach (var e in v.Edges)
                {
                    if (e == currentEdge) continue;
                    var other = e.Other();
                    if (other != null) continue;
                    
                    currentEdge = e;
                    foundNextBorder = true;
                    break;
                }
                if(!foundNextBorder)
                    throw new Exception("Border does not continue for some reason");

                if(borderEdge.Contains(currentEdge))
                    throw new Exception($"Edge list already contains this edge! Something went wrong ! Edge count {borderEdge.Count}");
                borderEdge.Add(currentEdge);

            } while (start != currentEdge);

            return borderEdge;
        }

        public static List<Polygon> CreateBorderPolygons(List<Polygon> innerShapes, Polygon districtShape)
        {
            List<Polygon> borderPolygons = new List<Polygon>();
            if (innerShapes.Count == 0)
                return borderPolygons;

            var startingEdge = districtShape.Edges[0];
            var currentEdge = startingEdge;

            var innerBorder = FindBorderEdges(innerShapes);
            var innerBorderPoints = EdgesToVertices(innerBorder);

            currentEdge.Origin.FindClosestIn(innerBorderPoints, out Vertex initialVertex);
            
            Vertex lastVertex = initialVertex;

            do
            {
                List<HalfEdge> pendingEdges = new List<HalfEdge>();

                Vertex currentEdgeOrigin = Vertex.Factory.Create(currentEdge.Origin);
                var e1 = new HalfEdge(lastVertex, currentEdgeOrigin);

                Vertex currentEdgeDestination = Vertex.Factory.Create(currentEdge.Destination);
                var e2 = new HalfEdge(e1.Destination, currentEdgeDestination, e1);

                currentEdge.Destination.FindClosestIn(innerBorderPoints, out Vertex closest);
                var e3 = new HalfEdge(e2.Destination, closest, e2);

                pendingEdges.AddMultiple(e1, e2, e3);

                if (lastVertex != closest)
                {
                    BuildNGon(closest, e3, e1, pendingEdges);
                }
                else
                {
                    // this is a triangle shape
                    e1.PreviousEdge = e3;
                    e3.NextEdge = e1;
                    // Temporary hack to quickly inverse direction
                    e1.Flip(); e2.Flip(); e3.Flip();
                }
                
                Polygon newPoly = new Polygon(pendingEdges);

                borderPolygons.Add(newPoly);

                lastVertex = closest;
                currentEdge = currentEdge.NextEdge;
                
            } while (startingEdge != currentEdge);
            
            return borderPolygons;

            void BuildNGon(Vertex closest, HalfEdge e3, HalfEdge e1, List<HalfEdge> pendingEdges)
            {
                // Build nGon (n > 3)
                List<HalfEdge> newBorder = GetShorestBorderConnection(closest, lastVertex, innerBorderPoints);
                e3.NextEdge = newBorder[0];
                newBorder[newBorder.Count - 1].PreviousEdge = e3;

                e1.PreviousEdge = newBorder[newBorder.Count - 1];
                newBorder[newBorder.Count - 1].NextEdge = e1;

                pendingEdges.AddRange(newBorder);
            }

            void TryMergeLastTriangle(Polygon newPoly)
            {
                if (borderPolygons.Count > 0 && borderPolygons[borderPolygons.Count - 1].Edges.Count == 3)
                {
                    // Combine last triangle shape to the current polygon
                    var triangle = borderPolygons[borderPolygons.Count - 1];
                    newPoly.CombinePolygon(triangle);

                    borderPolygons.Remove(triangle);
                }
            }
        }
        /// <summary>
        /// Calculates the shortest path along edges of a given list from start to end
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="target">End point</param>
        /// <param name="loopingBorder">The list of edges which is looping (first edge is connected to last edge)</param>
        /// <param name="onlyCheckOpenEdges">Only take edges into account which do not have opposite edges.</param>
        /// <returns>Returns the shortest path along the edges</returns>
        public static List<HalfEdge> GetShorestBorderConnection(Vertex start, Vertex target, List<Vertex> loopingBorder, bool onlyCheckOpenEdges = false)
        {
            List<HalfEdge> path = new List<HalfEdge>();
            int startIndex = loopingBorder.IndexOf(start);
            int targetIndex = loopingBorder.IndexOf(target);
            
            if(startIndex < 0 || targetIndex < 0)
                throw new Exception("Points are not part of the border");

            if (startIndex == targetIndex)
                return path;

            int sign = MathUtils.Sign(targetIndex - startIndex);

            int dr = targetIndex - startIndex;
            int df = targetIndex > startIndex ? (loopingBorder.Count - targetIndex + startIndex) : (loopingBorder.Count - startIndex + targetIndex);

            if (Math.Abs(dr) > df)
            {
                // in reverse order and looping
                sign *= -1;
            }
            
            TraceBorder();

            return path;

            void TraceBorder()
            {
                bool retriedOnce = false;
                int index = startIndex;
                Vertex lastVertex = loopingBorder[index];
                do
                {
                    index = MathUtils.Mod(index + sign, loopingBorder.Count);

                    var currentVertex = loopingBorder[index];
                    if(onlyCheckOpenEdges && lastVertex.GetEdgeTo(currentVertex, out HalfEdge connectingEdge))
                        throw new Exception($"There is no edge from '{lastVertex.Index}' to '{currentVertex.Index}'");
                    
                    if (onlyCheckOpenEdges)
                    {
                        if(retriedOnce)
                            throw new Exception($"there is no path connecting start ({start.Index}) to end ({target.Index}) only using open edges");
                        
                        // TODO: this would fix finding edge paths in the wrong direction
                        //if (connectingEdge.Other() != null)
                        //{
                        //    // Try other direction -> reset state
                        //    path.Clear();
                        //    index = startIndex;
                        //    lastVertex = loopingBorder[index];
                        //    retriedOnce = true;
                        //    continue;
                        //}
                    }

                    HalfEdge newEdge = new HalfEdge(lastVertex, currentVertex);
                    if (path.Count > 0)
                    {
                        path[path.Count - 1].NextEdge = newEdge;
                        newEdge.PreviousEdge = path[path.Count - 1];
                    }

                    path.Add(newEdge);
                    lastVertex = currentVertex;
                } while (index != targetIndex);
            }
        }

        public static List<Polygon> GetBorderPolygons(List<Polygon> polylist)
        {
            var borderPolygons = new List<Polygon>();
            foreach (var poly in polylist)
            {
                if(poly.IsBorderPolygon())
                    borderPolygons.Add(poly);
            }

            return borderPolygons;
        }
    }
}