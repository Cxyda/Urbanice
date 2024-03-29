using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._1D;
using Urbanice.Generators._1D.Random;
using Urbanice.Generators._2D;
using Urbanice.Generators._2D.SimpleVoronoi;
using Urbanice.Module.Containers;
using Urbanice.Module.Data;
using Urbanice.Module.Data.Utility;
using Urbanice.Utils;

namespace Urbanice.Module.Layers
{
    /// <summary>
    /// This class is responsible for generating and distributing the Wards of a city
    /// </summary>
    [CreateAssetMenu(menuName = "Urbanice/DataLayers/Create new Neighborhood Layer", fileName = "newNeighborhoodLayer", order = 5)]
    public class WardLayer : BaseLayer, IUrbaniceLayer
    {
        public DistrictLayer DistrictLayer { get; private set; }
        
        [Range(0, 5)] public int BorderSubdivinition = 1;
        public bool UseMaxBorderLength;
        [Range(0.05f, .3f)] public float MaxBorderLength = 0.3f;
        
        public WardDefinitionContainer WardDefinition;

        public Dictionary<Polygon, WardData> PolygonIdToNeighborhoodMap;
        [Space] public PseudoRandomGenerator RandomGenerator;
        
        [HideInInspector] public Graph<Vertex> TernaryStreetGraph;
        [HideInInspector] public List<Polygon> Neighborhoods;

        public void Init()
        {
            // Nothing to do here yet
        }

        public void Generate(BaseLayer parentLayer)
        {
            DistrictLayer = parentLayer as DistrictLayer;
            if (DistrictLayer == null)
            {
                throw new Exception($"Cannot cast parent layer to type {typeof(DistrictLayer).Name} ");
            }
            TernaryStreetGraph = new Graph<Vertex>();

            PolygonIdToNeighborhoodMap = new Dictionary<Polygon, WardData>();
            Neighborhoods = new List<Polygon>();

            DevelopNeighborhoods();
            SubdivideNeighborhoods();

            CreateTernaryStreetGraph();
            
        }

        /// <summary>
        /// Creates the streetgraph for the small ward streets
        /// </summary>
        private void CreateTernaryStreetGraph()
        {
            foreach (var shape in PolygonIdToNeighborhoodMap.Keys)
            {
                Vertex lastVertex = shape.Points[0];
                TernaryStreetGraph.AddNode(lastVertex);

                for (int i = 0; i < shape.Points.Count; i++)
                {
                    int n = (i + 1) % shape.Points.Count;
                    var cp = shape.Points[n];
                    
                    TernaryStreetGraph.AddNode(cp);
                    TernaryStreetGraph.ConnectNodes(lastVertex, cp);

                    lastVertex = cp;
                }
            }
        }

        /// <summary>
        ///  Subdivides long edges of the ward polygons
        /// </summary>
        private void SubdivideNeighborhoods()
        {
            for (int i = 0; i < BorderSubdivinition; i++)
            {
                foreach (var p in Neighborhoods)
                {
                    if (UseMaxBorderLength)
                    {
                        p.SubdivideAllEdgesWith(MaxBorderLength, false);
                    }
                    else
                    {
                        p.SubdivideAllEdgesAt(0.5f, false);
                    }
                }
            }
        }
        /// <summary>
        /// Generates the Neighboring Wards for each district
        /// </summary>
        private void DevelopNeighborhoods()
        {
            foreach (var district in DistrictLayer.PolygonIdToDistrictMap.Values)
            {
                GenerateNeighborhood(district);
            }
        }

        /// <summary>
        /// Generates the neighboring wards
        /// </summary>
        private void GenerateNeighborhood(DistrictData district)
        {
            for (var i = 0; i < district.Neigborhoods.Count; i++)
            {
                var polygon = district.Neigborhoods[i];
                WardType nType;
                

                if (district.InitialType == WardType.Any && i == 0)
                {
                    var allValidTypes = WardDefinition.GetTypesFor(district.Type);
                    nType = GetAny(allValidTypes);
                }
                else if (i == 0)
                {
                    nType = district.InitialType;
                }
                else
                {
                    nType = GetNeighborhoodTypeFor(polygon);
                }
                var nData = new WardData(nType, polygon);
                PolygonIdToNeighborhoodMap[polygon] = nData;
            }
            Neighborhoods.AddRange(district.Neigborhoods);
        }

        /// <summary>
        /// Returns a random element from the given list
        /// </summary>
        private WardType GetAny(List<WardType> allTypes)
        {
            var index = (int) (RandomGenerator.Generate() * allTypes.Count);
            return allTypes[index];
        }
        
        /// <summary>
        /// Finds a valid ward type for a polygon ward
        /// </summary>
        /// <param name="nPolygon"></param>
        /// <returns></returns>
        private WardType GetNeighborhoodTypeFor(Polygon nPolygon)
        {
            var neighbors = nPolygon.GetNeighbors();
            var wardWeights = new WeightedNWardList();

            List<WardType> forbiddenTypes = new List<WardType>();

            // Check all neighbors for valid and invalid neighbor types
            foreach (var neighbor in neighbors)
            {
                if (!PolygonIdToNeighborhoodMap.ContainsKey(neighbor) || PolygonIdToNeighborhoodMap[neighbor] == null)
                    continue;

                var config = WardDefinition.GetDefinitionFor(PolygonIdToNeighborhoodMap[neighbor].Type);

                foreach (WardWeight nd in config.NeighboringWards)
                {
                    if (nd.Weight == 0)
                    {
                        // A weight of 0 means this type is forbidden, add it to the list to remove it later
                        forbiddenTypes.Add(nd.Element);
                        continue;
                    }

                    wardWeights.Add(nd);
                }
            }

            // Assign type invalid, because for some reason the weighs list is empty
            if (wardWeights.OverallWeight == 0)
                return WardType.Invalid;

            // Remove all forbidden types
            foreach (var ft in forbiddenTypes)
            {
                wardWeights.RemoveAll(ft);
            }
            // TODO : Apply filters later

            // Finally get the type
            var value = GlobalPRNG.Next();
            var type = wardWeights.GetElement(value);
            return type;
        }

        /// <summary>
        /// Calculates a line intersection with a border
        /// </summary>
        private bool  CalculateIntersectionWithBorder(out Vector2 intersection, Vector2 insideVertex, Vector2 outsideVertex, Polygon districtShape)
        {
            foreach (var edge in districtShape.Edges)
            {
                if (!GeometryUtils.GetLineIntersection( insideVertex, outsideVertex, edge.Origin, edge.Destination, out intersection))
                {
                    continue;
                }

                return true;
            }
            intersection = new Vector2(float.MaxValue, float.MaxValue);

            return false;
        }

        
/*    Different algorithms to problems already solved above, keep it for later
        private void Option2(Vertex p0)
        {
            SecondaryStreets.AddNode(p0);
            var lastNode = p0;
            var list = new List<Vertex>(_innerPoints);

            list.Remove(lastNode);
            var neighbors = SecondaryStreets.GetNeighbors(lastNode);
            if (neighbors == null)
            {
                SecondaryStreets.AddNode(lastNode);
            }
            else
            {
                foreach (var n in neighbors)
                {
                    list.Remove(n);
                }
            }

            var closestVertex = lastNode.FindClosestIn(list);

            var connection = GeometryHelpers.CreateLineTowardsPoint(p0, closestVertex, 0.5f, 0.0f);
            // exclude first and last point
            for (int n = 1; n < connection.Count-1; n++)
            {
                var pn = new Vertex(connection[n]);
                SecondaryStreets.AddNode(pn);
                SecondaryStreets.ConnectNodes(lastNode, pn);
                lastNode = pn;
            }
            SecondaryStreets.AddNode(closestVertex);
            SecondaryStreets.ConnectNodes(p0, closestVertex);

        }
        */
        /*

    private void Option1()
    {
        do
        {
            list.Remove(lastNode);
        
            var neighbors = SecondaryStreets.GetNeighbors(lastNode);
            closestVertex = lastNode.FindClosestIn(list);
        
            if (neighbors == null || !neighbors.Contains(closestVertex))
            {
                break;
            }
            lastNode = closestVertex;
        
            cnt--;
        
        } while (cnt > 0);
        var connection = GeometryHelpers.CreateLineTowardsPoint(p0, closestVertex, 0.1f, 0.2f);
        // exclude first and last point
        for (int n = 1; n < connection.Count-1; n++)
        {
            var pn = new Vertex(connection[n]);
            SecondaryStreets.AddNode(pn);
            SecondaryStreets.ConnectNodes(lastNode, pn);
            lastNode = pn;
        }
        SecondaryStreets.AddNode(closestVertex);
        SecondaryStreets.ConnectNodes(p0, closestVertex);
    }
        */

/*
        public void CreateNeighborhoodIn(District district)
        {
            Voronoi = new SimpleVoronoi();
            var points = new List<Vector2>();

            points.Add(district.Shape.Center);
            foreach (var p in district.Shape.Points)
            {
                points.Add(p);
            }

            Debug.Log("Size : " + district.Shape.Area);
            var randomPoints = MathUtils.GeneratePointsAround(district.Shape.Center, (int) (district.Shape.Area * 100),
                0.2f, new Rect(0, 0, 1, 1), district.Shape.Area);
            
            
            points.AddRange(randomPoints);

            var regions = Voronoi.Generate(points);
            Neighborhoods = regions;
        }
 */
    }
}