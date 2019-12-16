using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators;
using Urbanice.Generators._1D;
using Urbanice.Generators._1D.Random;
using Urbanice.Module.Data;
using Urbanice.Module.Data.Utility;
using Urbanice.Utils;

namespace Urbanice.Module.Layers
{
    /// <summary>
    /// This class is responsible for generating streets within the city. It is not fully implemented yet.
    /// For now it only generates streets which would connect the city to other cities.
    /// </summary>
    [CreateAssetMenu(menuName = "Urbanice/DataLayers/Create new Street Layer", fileName = "newStreetLayer", order = 3)]
    public class StreetLayer : BaseLayer, IUrbaniceLayer
    {
        public enum Directions
        {
            North,
            East,
            South,
            West
            
        }
        
        public FloatGenerator FloatGenerator;

        public float StreetNoise = .25f;
        [Range(0, 20)]
        public int StreetSmoothness = 3;
        [Range(0,8)]
        public int StreetCount = 2;

        [HideInInspector]public Graph<Vertex> StreetGraph;
        
        public DistrictLayer DistrictLayer { get; private set; }

        public void Init()
        {
            FloatGenerator.Init();
        }

        public void Generate(BaseLayer parentLayer)
        {
            DistrictLayer = parentLayer as DistrictLayer;
            StreetGraph = new Graph<Vertex>();

            //BuildStreetGraph();

            var pointOnBounds = CreatePointOnBounds(StreetCount);

            
            for (var i = 0; i < StreetCount; i++)
            {
                var street = new List<Vertex>();

                // Generate points
                var vertexOnBounds = Vertex.Factory.Create(pointOnBounds[i]);

                var maxSegmentLength = StreetSmoothness == 0 ? 1f : 0.5f / StreetSmoothness;
                if (vertexOnBounds.FindClosestIn(DistrictLayer.DistrictControlPoints, out var closestPointToStart))
                {
                    var connection1 = GeometryUtils.CreateLineTowardsPoint(vertexOnBounds, closestPointToStart, maxSegmentLength, StreetNoise);
                    street.AddRange(connection1);
                    AddLineToGraph(street);
                }
            }
        }

        private void BuildStreetGraph()
        {
            foreach (DistrictData d in DistrictLayer.PolygonIdToDistrictMap.Values)
            {
                BuildStreetOnDistrictEdges(d);
                BuildCrossRoads(d);
            }
        }


        private List<Vector2> CreatePointOnBounds(int amount)
        {
            WeightedList<WeightedElement<Directions>, Directions> borderWeights = new WeightedList<WeightedElement<Directions>, Directions>();
            
            // Add 8 valid directions to the weighted list. For now 8 streets max are possible
            borderWeights.Add(new WeightedElement<Directions>(Directions.North, 1));
            borderWeights.Add(new WeightedElement<Directions>(Directions.East, 1));
            borderWeights.Add(new WeightedElement<Directions>(Directions.South, 1));
            borderWeights.Add(new WeightedElement<Directions>(Directions.West, 1));
            borderWeights.Add(new WeightedElement<Directions>(Directions.North, 1));
            borderWeights.Add(new WeightedElement<Directions>(Directions.East, 1));
            borderWeights.Add(new WeightedElement<Directions>(Directions.South, 1));
            borderWeights.Add(new WeightedElement<Directions>(Directions.West, 1));
            
            List<Vector2> pointOnBounds = new List<Vector2>();

            for (int i = 0; i < amount; i++)
            {
                var rnd = FloatGenerator.Generate();
                var direction = borderWeights.GetElement(rnd);
            
                var rnd1 = FloatGenerator.Generate();

                switch (direction)
                {
                    case Directions.North:
                        pointOnBounds.Add(new Vector2(rnd1, 1f));
                        break;
                    case Directions.East:
                        pointOnBounds.Add(new Vector2(1f, rnd1));

                        break;
                    case Directions.South:
                        pointOnBounds.Add(new Vector2(rnd1, 0f));

                        break;
                    case Directions.West:
                        pointOnBounds.Add(new Vector2(0f, rnd1));
                        break;
                }
            }
            return pointOnBounds;
        }
        /// <summary>
        /// Creates a street on the dirstrict border and adds it to the streetgraph
        /// </summary>
        private void BuildStreetOnDistrictEdges(DistrictData d)
        {
            for (int i = 0; i < d.Shape.Points.Count; i++)
            {
                var p0 = d.Shape.Points[i];
                var p1 = d.Shape.Points[(i + 1) % d.Shape.Points.Count];

                var streetTowardsPoint = GeometryUtils.CreateLineTowardsPoint(p0, p1, 0.5f / StreetSmoothness, 0f);
                AddLineToGraph(streetTowardsPoint, false);
            }
        }

        /// <summary>
        /// Builds a crossroad and branches out towards the bisectrix of a corner point
        /// </summary>
        private void BuildCrossRoads(DistrictData d)
        {
            // find crossroads
            foreach (Vertex cp in d.Shape.Points)
            {
                // Don't allow every crossroad to branch out'
                float crossroadProbability = 5 * FloatGenerator.Generate() / (cp.Edges.Count - 3);
                if (crossroadProbability < .25f)
                    continue;

                Vector2 bisectrix = d.Shape.GetBisectrix(cp);
                Vector2 point = GeometryUtils.CreateLineSegmentInDirection(cp, bisectrix, 0.5f / StreetSmoothness,
                    StreetNoise);

                StreetGraph.AddNode(cp);
                bool intersectionFound = false;

                // Check for borderintersection
                if (CheckForLineIntersection(cp, point, d.Neigborhoods, out Vector2 intersection))
                {
                    point = intersection;
                    intersectionFound = true;
                }

                Vertex vertex = Vertex.Factory.GetOrCreateVertexWithinRange(point, 0.02f);
                StreetGraph.AddNode(vertex);
                StreetGraph.ConnectNodes(cp, vertex);

                if (intersectionFound) continue;

                Vertex lastVertex = vertex;
                do
                {
                    // continue streetbuilding
                    if (!lastVertex.FindClosestIn(d.BorderEdges, out Vertex closestPoint))
                    {
                        break;
                    }
                    Vector2 newPoint = GeometryUtils.CreateLineSegmentTowardsPoint(lastVertex, closestPoint,
                        0.5f / StreetSmoothness,
                        StreetNoise);
                    Vertex v = Vertex.Factory.GetOrCreateVertexWithinRange(newPoint, 0.01f);

                    StreetGraph.AddNode(v);
                    StreetGraph.ConnectNodes(lastVertex, v);

                    lastVertex = v;
                    if (v == closestPoint)
                        break;
                } while (true);
            }
        }

        /// <summary>
        /// Calculates the intersection between a line and a polygon
        /// </summary>
        private bool CheckForLineIntersection(Vector2 p1Start, Vector2 p1End, List<Polygon> polygons, out Vector2 intersection)
        {
            foreach (var p in polygons)
            {
                foreach (var edge in p.Edges)
                {
                    if (GeometryUtils.GetLineIntersection(p1Start, p1End, edge.Origin, edge.Destination,
                        out intersection))
                    {
                        return true;
                    }
                }
            }

            intersection = Vector2.positiveInfinity;
            return false;
        }

        /// <summary>
        /// Adds a line to the streetgraph
        /// </summary>
        private void AddLineToGraph(List<Vertex> line, bool loop = false)
        {
            for (int i = 0; i < line.Count; i++)
            {
                var p0 = line[i];

                StreetGraph.AddNode(p0);

                if (!loop && i == line.Count - 1)
                {
                    continue;
                }
                var p1 = line[(i+1) % line.Count];
                StreetGraph.AddNode(p1);
                StreetGraph.ConnectNodes(p0, p1);
            }
        }
        /* Alternative approach, keep it for later use
        private void CreateStreetGraph()
        {
            SecondaryStreets = new Graph<Vertex>();
            foreach (var district in _districtRegions)
            {
                List<Vertex> innerPoints = new List<Vertex>(district.InnerPoints);
                List<Vertex> borderPoints = new List<Vertex>(district.OuterPoints);
                // Add more points to long edges
                GeneratePointsOnDistrictEdges(district, ref borderPoints);

                foreach (var borderPoint in borderPoints)
                {

                    //Vector2 direction = district.Shape.GetBisectrix(borderPoint);
                    Vertex p = borderPoint.GetRandomPointWithinDistance(innerPoints, .1f);
                    if(p == null)
                        continue;
                    Vector2 direction = p - borderPoint;
                    Vertex lastVertex = borderPoint;
                    SecondaryStreets.AddNode(borderPoint);
                    for (int i = 0; i < 20; i++)
                    {
                        // build line segment towards center
                        Vector2 p1 = GeometryUtils.CreateLineSegmentInDirection(lastVertex, direction, 0.05f, 0.5f);

                        // Is destination within polygon?
                        if (!district.Shape.Contains(p1))
                        {
                            if (!CalculateIntersectionWithBorder(out var intersectionPoint, lastVertex, p1,
                                district.Shape))
                            {
                                break;
                            }
                            i = 20;
                            p1 = intersectionPoint;
                        }
                        
                        Vertex newVertex = Vertex.Factory.Create(p1);
                        // search within range of destination for other vertices
                        Vertex currentVertex = newVertex.GetRandomPointWithinDistance(innerPoints, 0.04f);
                        // TODO : check angle !
                        // use existing point if there is one
                        if (currentVertex == null)
                        {
                            currentVertex = newVertex;
                            district.InnerPoints.Add(currentVertex);
                        }
                        SecondaryStreets.AddNode(currentVertex);
                        SecondaryStreets.ConnectNodes(lastVertex, currentVertex);
                        lastVertex = currentVertex;
                    }
                }
            }
        }*/

        /*
        public void GenerateStreet(Vertex p)
        {
            ControlPoints = new List<List<Vertex>>();
            StreetMesh = new Mesh();

            for (int i = 0; i < StreetCount; i++)
            {
                // Generate points
                bool valid = false;

                Vertex v1;
                Vertex v2;
                var rnd1 = PRNG.Next();
                var rnd2 = PRNG.Next();
                var rnd3 = PRNG.Next();
                var rnd4 = PRNG.Next();
                
                // validate points, don't start and endpoint on the same border
                v1 = rnd1 + rnd2 >= 1f ? new Vertex(Math.Round(rnd1), rnd2) : new Vertex(rnd1, Math.Round(rnd2));
                v2 = rnd3 + rnd4 >= 1f ? new Vertex(Math.Round(rnd3), rnd4) : new Vertex(rnd3, Math.Round(rnd4));
                    
                var list = new List<Vertex>();
                list.AddMultiple(v1, p, v2);

                ControlPoints.Add(list);
            }

            ControlPoints = GeometryHelpers.WeldLinepointsWithinRange(ControlPoints, MinStreetpointDistance);
            
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                // generate Mesh
                var list = new List<Vertex>(ControlPoints[i]);
                for (int n = 0; n < StreetSmoothness; n++)
                {
                    list = GeometryHelpers.SubdivideLine(list, StreetNoise);
                }

                //var m = PrimitiveGenerator.GenerateLine(list, StreetWidth);
                //StreetMesh = PrimitiveGenerator.CombineMesh(StreetMesh, m);
                ControlPoints[i] = list;
            }
        }
        */

    }
}