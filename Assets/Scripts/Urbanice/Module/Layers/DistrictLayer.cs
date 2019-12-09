using System;
using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._1D.Random;
using Urbanice.Generators._2D;
using Urbanice.Generators._2D.SimpleVoronoi;
using Urbanice.Module.Containers;
using Urbanice.Module.Data;
using Urbanice.Module.Data.Utility;
using Urbanice.Utils;

namespace Urbanice.Module.Layers
{
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

        public Dictionary<Vector2, Vertex> DistrictControlPoints;
        public Dictionary<Polygon, DistrictData> PolygonIdToDistrictMap;

        [HideInInspector] public List<Polygon> Polygons;
        [HideInInspector] public List<Vector2> Sites;
        [HideInInspector] public List<Region> Regions;

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

            DistrictControlPoints = new Dictionary<Vector2, Vertex>();
            PolygonIdToDistrictMap = new Dictionary<Polygon, DistrictData>();
            
            // Only needed to expose data to the renderer
            Polygons = new List<Polygon>();
            Sites = new List<Vector2>();
            Regions = new List<Region>();
            
            BuildDistrictPolygons();
            SortRegionsByDistance();

            GenerateDistricts();
            SubdivideDistricts();

            DevelopDistricts();
        }

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

                var clonedPolygon = Polygon.CloneInsetAndFlip(district.Shape, 0.01f);
                district.Neigborhoods = cfg.PatternGenerator.Generate(insidePoints, clonedPolygon, false);
                clonedPolygon.Destroy();
                
                district.Neigborhoods.Sort((n1, n2) =>
                {
                    return Vector2.Distance(n1.Center, district.Shape.Center) >
                           Vector2.Distance(n2.Center, district.Shape.Center) ? 1 : -1;
                });
                
                DevelopNeighbours(district);

                // Temporary
                //Regions.AddRange(cfg.PatternGenerator.Regions);
                //Sites.AddRange(cfg.PatternGenerator.Sites);
            }
        }

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

        private void BuildDistrictPolygons()
        {
            _cityCenter = CityLayer.CityCores[0];

            _citySites = new List<Vector2>(CityLayer.CityCores);

            var polygons = Generator.Generate(_citySites, CityLayer.CityCanvas, false);
            CityLayer.CityCanvas.Destroy();

            foreach (var p in polygons)
            {
                Polygons.Add(p);
            }
        }

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

        private DistrictType GetDistrictTypeForRegion(Polygon region)
        {
            var neighbors = region.GetNeighbors();
            var districtWeights = new WeightedDistrictList();

            List<DistrictType> forbiddenTypes = new List<DistrictType>();

            foreach (var neighbor in neighbors)
            {
                if (!PolygonIdToDistrictMap.ContainsKey(neighbor) || PolygonIdToDistrictMap[neighbor] == null)
                    continue;

                var config = DefinitionContainer.GetDefinitionFor(PolygonIdToDistrictMap[neighbor].Type);

                foreach (DistrictWeight nd in config.NeighboringDistricts)
                {
                    if (nd.Weight == 0)
                    {
                        forbiddenTypes.Add(nd.Element);
                        continue;
                    }

                    districtWeights.Add(nd);
                }
            }

            if (districtWeights.OverallWeight == 0)
                return DistrictType.Invalid;

            foreach (var ft in forbiddenTypes)
            {
                districtWeights.RemoveAll(ft);
            }
            // TODO : Apply filters later

            var value = GlobalPRNG.Next();
            var type = districtWeights.GetElement(value);
            return type;
        }
    }
}