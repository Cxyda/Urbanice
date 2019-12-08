using System;
using System.Collections.Generic;
using UnityEngine;
using Urbanice.Generators._3D.Mesh;
using Urbanice.Module;
using Urbanice.Module.Containers;
using Urbanice.Module.Data;
using Urbanice.Module.Districts;

namespace Urbanice.Renderer
{
    public class MeshRenderer: MonoBehaviour
    {
        public CityDefinitionContainer CityConfiguration;

        public bool ShowMeshes = true;
        [Space]

        public bool ShowDistricts = false;
        public bool ShowDistrictBorders = false;
        public bool ShowDistrictWire = false;
        [Space]

        public bool ShowNeighborhoods = false;
        public bool ShowNeighborhoodBorders;
        public bool ShowNeighborhoodWire = false;
        
        public DistrictConfigurationContainer DistrictConfigs;
        public WardDefinitionContainer WardConfig;
        
        
        private List<Mesh> DistrictMeshes = new List<Mesh>();
        private List<Mesh> NeighborhoodMeshes = new List<Mesh>();
        
        public void GenerateMeshes()
        {
            DistrictMeshes.Clear();
            NeighborhoodMeshes.Clear();
            if (CityConfiguration.DistrictLayer != null && CityConfiguration.DistrictLayer.PolygonIdToDistrictMap != null)
            {
                foreach (var polygon in CityConfiguration.DistrictLayer.PolygonIdToDistrictMap.Keys)
                {
                    Mesh m = PrimitiveGenerator.GeneratePolygon(polygon);
                    DistrictMeshes.Add(m);
                }
            }

            if (CityConfiguration.WardLayer != null && CityConfiguration.WardLayer.PolygonIdToNeighborhoodMap != null)
            {
                foreach (var polygon in CityConfiguration.WardLayer.PolygonIdToNeighborhoodMap.Keys)
                {
                    Mesh m = PrimitiveGenerator.GeneratePolygon(polygon);
                    NeighborhoodMeshes.Add(m);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!ShowMeshes)
            {
                return;
            }

            GenerateMeshes();

            DrawDistricts();
            DrawNeighborhoods();
        }

        private void DrawDistricts()
        {
            if (!ShowDistricts || CityConfiguration.DistrictLayer.PolygonIdToDistrictMap == null) return;
                
            Vector3 position = Vector3.forward * 0.01f;
            
            int cnt = 0;
            foreach (var district in CityConfiguration.DistrictLayer.PolygonIdToDistrictMap.Values)
            {
                if(district == null)
                    continue;
                var mesh = DistrictMeshes[cnt];
                var type = district.Type;

                Gizmos.color = DistrictConfigs.GetDefinitionFor(type).RenderColor;
                Gizmos.DrawMesh(mesh, position);

                if (ShowDistrictWire)
                {
                    Gizmos.color = new Color(0.2f,0.2f, 0.2f, 0.1f);
                    Gizmos.DrawWireMesh(mesh, position);
                }
                
                cnt++;
            }
            
            if (ShowDistrictBorders)
            {
                Gizmos.color = new Color(0.2f,0.2f, 0.2f, 0.3f);
                foreach (var d in CityConfiguration.DistrictLayer.Polygons)
                {
                    foreach (var e in d.Edges)
                    {
                        Gizmos.DrawLine(e.Origin, e.Destination);
                    }
                }
            }
        }

        private void DrawNeighborhoods()
        {
            if(!ShowNeighborhoods || CityConfiguration.WardLayer.Neighborhoods == null) return;

            Vector3 position = Vector3.forward * 0.02f;
            int cnt = 0;

            foreach (var neighborhood in CityConfiguration.WardLayer.PolygonIdToNeighborhoodMap.Values)
            {
                var mesh = NeighborhoodMeshes[cnt];
                var type = neighborhood.Type;

                Gizmos.color = WardConfig.GetDefinitionFor(type).RenderColor;
                Gizmos.DrawMesh(mesh, position);
                
                if (ShowNeighborhoodWire)
                {
                    Gizmos.color = new Color(0.2f,0.2f, 0.2f, 0.3f);
                    Gizmos.DrawWireMesh(mesh, position);
                }

                if (ShowNeighborhoodBorders)
                {
                    Gizmos.color = new Color(0.2f,0.2f, 0.2f, 0.3f);
                    foreach (var n in neighborhood.Shape.Edges)
                    {
                        Gizmos.DrawLine(n.Origin, n.Destination);
                    }
                }
                cnt++;
            }
        }

        private void ClearMeshes()
        {
            var children = new List<Transform>();
            
            for (int i = 0; i < transform.childCount; i++)
            {
                children.Add(transform.GetChild(i));
            }

            foreach (var c in children)
            {
                DestroyImmediate(c.gameObject);
            }
        }
    }
}