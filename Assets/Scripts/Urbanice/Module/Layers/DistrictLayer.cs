using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
    /// This class is responsible for generating and distributing the Districts of a city
    /// </summary>
    [CreateAssetMenu(menuName = "Urbanice/DataLayers/Create new District Layer", fileName = "newDistrictLayer",
        order = 4)]
    public class DistrictLayer : BaseLayer, IUrbaniceLayer
    {
        public CityLayer CityLayer { get; private set; }

        [Range(0, 5)] public int BorderSubdivinition = 1;
        public bool UseMaxBorderLength;
        [Range(0.05f, .3f)] public float MaxBorderLength = 0.3f;
        public PatternGenerator Generator;

        public DistrictConfigurationContainer DefinitionContainer;

        private Vector2 _cityCenter;
        private List<Vector2> _citySites;

        public Dictionary<Polygon, DistrictData> PolygonIdToDistrictMap;

        [HideInInspector] public Graph<Vertex> SecondaryStreetGraph;
        [HideInInspector] public List<Polygon> Polygons;
        [HideInInspector] public List<Region> Regions;
        [HideInInspector] public List<Vertex> DistrictControlPoints;

        
        public void Init()
        {
            Generator.Init();
        }

        public void Generate(BaseLayer parentLayer)
        {
            CityLayer = parentLayer as CityLayer;
            if (CityLayer == null)
            {
                throw new Exception($"Cannot cast parent layer to type {typeof(CityLayer).Name} ");
            }

            PolygonIdToDistrictMap = new Dictionary<Polygon, DistrictData>();
            SecondaryStreetGraph = new Graph<Vertex>();
            
            // Only needed to expose data to the renderer
            Polygons = new List<Polygon>();
            Regions = new List<Region>();
            
            BuildDistrictPolygons();
            SortRegionsByDistance();

            GenerateDistricts();
            SubdivideDistricts();

            CreateSecondaryStreetGraph();
            DevelopDistricts();

        }

        /// <summary>
        /// Generates the streets between districts
        /// </summary>
        private void CreateSecondaryStreetGraph()
        {
            foreach (var shape in PolygonIdToDistrictMap.Keys)
            {
                Vertex lastVertex = shape.Points[0];
                SecondaryStreetGraph.AddNode(lastVertex);

                for (int i = 0; i < shape.Points.Count; i++)
                {
                    int n = (i + 1) % shape.Points.Count;
                    var cp = shape.Points[n];
                    
                    SecondaryStreetGraph.AddNode(cp);
                    SecondaryStreetGraph.ConnectNodes(lastVertex, cp);

                    lastVertex = cp;
                }
            }
        }

        /// <summary>
        /// Subdivides districts into Wards according to thhe given generator
        /// </summary>
        private void DevelopDistricts()
        {
            foreach (var district in PolygonIdToDistrictMap.Values)
            {
                List<Vector2> insidePoints = new List<Vector2>();

                var shape = district.Shape;
                Polygons.Add(shape);
                
                var points = MathUtils.GeneratePointsAround(shape.Center, 1, shape.MaxRadius, shape.MinRadius / 5);
                insidePoints.Add(district.Shape.Center);
                insidePoints.AddRange(points);
                
                var cfg = DefinitionContainer.GetDefinitionFor(district.Type);
                cfg.PatternGenerator.Init();

                // Subdivide districts in neighborhoods
                var clonedPolygon = Polygon.CloneInsetAndFlip(district.Shape, 0.001f);
                district.Neigborhoods = cfg.PatternGenerator.Generate(insidePoints, clonedPolygon, true);
                clonedPolygon.Destroy();
                
                district.Neigborhoods.Sort((n1, n2) =>
                {
                    return Vector2.Distance(n1.Center, district.Shape.Center) >
                           Vector2.Distance(n2.Center, district.Shape.Center) ? 1 : -1;
                });

                // GetBorders
                //district.BorderEdges = GeometryUtils.FindBorderEdges(district.Neigborhoods);
                
                DevelopNeighbours(district);
            }
        }
        /// <summary>
        /// Subdivides long border edges
        /// </summary>
        private void SubdivideDistricts()
        {
            for (int i = 0; i < BorderSubdivinition; i++)
            {
                foreach (var p in Polygons)
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
        /// Generates the district polygons according to the used generator
        /// </summary>
        private void BuildDistrictPolygons()
        {
            _cityCenter = CityLayer.CityCores[0];

            _citySites = new List<Vector2>(CityLayer.CityCores);

            var polygons = Generator.Generate(_citySites, CityLayer.CityCanvas, false);
            DistrictControlPoints = Generator.ControlPoints;
            
            CityLayer.CityCanvas.Destroy();

            foreach (var p in polygons)
            {
                Polygons.Add(p);
            }
        }

        /// <summary>
        /// Sorts the districts by distance to the city center
        /// </summary>
        private void SortRegionsByDistance()
        {
            Polygons.Sort((d1, d2) =>
                {
                    return Vector2.Distance(d1.Center, _cityCenter) >
                           Vector2.Distance(d2.Center, _cityCenter)
                        ? 1
                        : -1;
                }
            );
        }

        /// <summary>
        /// Assigns district types by the direct neighbors
        /// </summary>
        private void GenerateDistricts()
        {
            if (Polygons.Count == 0)
                return;
            DistrictType type = DistrictType.TownSqaure;
            // TODO : having this here again is ugly, refactor later!
            var config = DefinitionContainer.GetDefinitionFor(type);

            var townSquare = new DistrictData(type, Polygons[0]);
            townSquare.InitialType = config.InitialSpawnType;
            
            PolygonIdToDistrictMap[Polygons[0]] = townSquare;
            
            for (int i = 1; i < Polygons.Count; i++)
            {
                var region = Polygons[i];

                if (region.IsBorderPolygon())
                {
                    type = DistrictType.Farmland;
                }
                else
                {
                    type = GetDistrictTypeForRegion(region);
                }
                config = DefinitionContainer.GetDefinitionFor(type);
                
                DistrictData district = new DistrictData(type, region);
                district.InitialType = config.InitialSpawnType;
                
                PolygonIdToDistrictMap[region] = district;
            }
        }

        private void DevelopNeighbours(DistrictData districtData)
        {
            var neighbors = districtData.Shape.GetNeighbors();

            foreach (var neighbor in neighbors)
            {
                if (PolygonIdToDistrictMap.ContainsKey(neighbor))
                    continue;
            }
        }

        /// <summary>
        /// Finds a valid district type for a district
        /// </summary>
        private DistrictType GetDistrictTypeForRegion(Polygon region)
        {
            var neighbors = region.GetNeighbors();
            var districtWeights = new WeightedDistrictList();

            List<DistrictType> forbiddenTypes = new List<DistrictType>();

            // Gather all neighbors and sum up the weights
            foreach (var neighbor in neighbors)
            {
                if (!PolygonIdToDistrictMap.ContainsKey(neighbor) || PolygonIdToDistrictMap[neighbor] == null)
                    continue;

                var config = DefinitionContainer.GetDefinitionFor(PolygonIdToDistrictMap[neighbor].Type);

                foreach (DistrictWeight nd in config.NeighboringDistricts)
                {
                    if (nd.Weight == 0)
                    {
                        // A forbidden type was found! Add it to the list to remove it later
                        forbiddenTypes.Add(nd.Element);
                        continue;
                    }

                    districtWeights.Add(nd);
                }
            }

            // Assign invalid type because the weighted list is empty
            if (districtWeights.OverallWeight == 0)
                return DistrictType.Invalid;

            // Remove all forbidden district types
            foreach (var ft in forbiddenTypes)
            {
                districtWeights.RemoveAll(ft);
            }
            // TODO : Apply filters later

            // finally return one of the valid types
            var value = GlobalPRNG.Next();
            var type = districtWeights.GetElement(value);
            return type;
        }
    }
}