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
    [CreateAssetMenu(menuName = "Urbanice/DataLayers/Create new Neighborhood Layer", fileName = "newNeighborhoodLayer", order = 5)]
    public class WardLayer : BaseLayer, IUrbaniceLayer
    {
        public DistrictLayer DistrictLayer { get; private set; }
        
        [Range(0, 5)] public int BorderSubDiv = 1;
        public bool UseMaxBorderLength;
        [Range(0.05f, .3f)] public float MaxBorderLength = 0.3f;
        
        public WardDefinitionContainer WardDefinition;
        [HideInInspector] public List<Polygon> Neighborhoods;

        public Dictionary<Polygon, WardData> PolygonIdToNeighborhoodMap;
        [Space] public PseudoRandomGenerator RandomGenerator;
        
        public void Init()
        {
        }

        public void Generate(BaseLayer parentLayer)
        {
            DistrictLayer = parentLayer as DistrictLayer;
            if (DistrictLayer == null)
            {
                throw new Exception($"Cannot cast parent layer to type {typeof(DistrictLayer).Name} ");
            }

            PolygonIdToNeighborhoodMap = new Dictionary<Polygon, WardData>();
            Neighborhoods = new List<Polygon>();

            DevelopNeighborhoods();
        }

        private void DevelopNeighborhoods()
        {
            foreach (var district in DistrictLayer.PolygonIdToDistrictMap.Values)
            {
                GenerateNeighborhood(district);
            }
        }

        private void GenerateNeighborhood(DistrictData district)
        {

            for (var i = 0; i < district.Neigborhoods.Count; i++)
            {
                var polygon = district.Neigborhoods[i];
                WardType nType;
                

                if (district.InitialType == WardType.Any && i == 0)
                {
                    Debug.Log("Getting types for " + district.Type);

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
        }

        private WardType GetAny(List<WardType> allTypes)
        {
            var index = (int) (RandomGenerator.Generate() * allTypes.Count);
            return allTypes[index];
        }
        private WardType GetNeighborhoodTypeFor(Polygon nPolygon)
        {
            var neighbors = nPolygon.GetNeighbors();
            var wardWeights = new WeightedNWardList();

            List<WardType> forbiddenTypes = new List<WardType>();

            foreach (var neighbor in neighbors)
            {
                if (!PolygonIdToNeighborhoodMap.ContainsKey(neighbor) || PolygonIdToNeighborhoodMap[neighbor] == null)
                    continue;

                var config = WardDefinition.GetDefinitionFor(PolygonIdToNeighborhoodMap[neighbor].Type);

                foreach (WardWeight nd in config.NeighboringWards)
                {
                    if (nd.Weight == 0)
                    {
                        forbiddenTypes.Add(nd.Element);
                        continue;
                    }

                    wardWeights.Add(nd);
                }
            }

            if (wardWeights.OverallWeight == 0)
                return WardType.Invalid;

            foreach (var ft in forbiddenTypes)
            {
                wardWeights.RemoveAll(ft);
            }
            // TODO : Apply filters later

            var value = GlobalPRNG.Next();
            var type = wardWeights.GetElement(value);
            return type;
        }


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

        private static void GeneratePointsOnDistrictEdges(DistrictData district, ref List<Vertex> borderPoints)
        {
            foreach (var edge in district.Shape.Edges)
            {
                if (edge.Length < 0.005f)
                    continue;
                var p = GeometryUtils.GetPointOn(edge, GlobalPRNG.Next(RandomDistribution.Cubic));
                borderPoints.Add(Vertex.Factory.Create(p));
            }
        }

        
/*
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