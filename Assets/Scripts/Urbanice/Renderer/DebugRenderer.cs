using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Module;
using Urbanice.Module.Containers;
using Urbanice.Utils;

namespace Urbanice.Renderer
{
    /// <summary>
    /// TODO:
    /// </summary>
    [ExecuteInEditMode]
    public class DebugRenderer : MonoBehaviour
    {
        public CityDefinitionContainer CityConfiguration;

        [Header("Show Element")]

        public bool ShowCityCore;
        public bool ShowCityBounds;
        public bool ShowCityIndexes;
        
        [Space]
        public bool ShowDistricts;
        public bool ShowDistrictCenters;
        public bool ShowDistrictBorders;
        public bool ShowDistrictIndexes;
        public bool ShowDistrictBounds;
        public bool ShowDistrictRegions;
        [Space]

        public bool ShowNeighborhoods;
        public bool ShowNeighborhoodBorders;
        public bool ShowNeighborhoodIndexes;
        public bool ShowNeighborhoodBounds;
        public bool ShowNeighborhoodRegions = false;
        [Space]

        public bool ShowStreets;
        public bool ShowCityPatches;
        
        private void OnDrawGizmos()
        {
            DrawDistrictPattern();
            DrawNeighborhoodPatterns();
            
            DrawCityElements();
            DrawCityCores();

            if (CityConfiguration == null || CityConfiguration.PrimaryStreetLayer == null ||
                CityConfiguration.PrimaryStreetLayer.StreetGraph == null)
            {
                return;
            }
        }

        private void DrawDistrictPattern()
        {
            if(!ShowDistricts || CityConfiguration.DistrictLayer.Generator.GeneratedPolygons == null)
                return;
            

            if (ShowDistrictCenters)
            {
                foreach (var p in CityConfiguration.DistrictLayer.Polygons)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(p.Center, 0.005f);
                    foreach (var point in p.Points)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawSphere(point, 0.003f);

                    }
                }
            }

            
            if (ShowDistrictRegions)
            {
                foreach (var region in CityConfiguration.DistrictLayer.Regions)
                {
                    foreach (var tr in region.Triangles)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(tr.Center, 0.003f);

                        Gizmos.color = Color.green;
                                
                        Gizmos.DrawLine(tr.P1, tr.P2);
                        Gizmos.DrawLine(tr.P2, tr.P3);
                        Gizmos.DrawLine(tr.P3, tr.P1);
                    }
                }

            }
            
            foreach (var district in CityConfiguration.DistrictLayer.Generator.GeneratedPolygons)
            {
                if (ShowDistrictIndexes)
                {
                    Handles.Label(district.Center, district.Index.ToString());
                }
                foreach (var e in district.Edges)
                {
                    if (ShowDistrictIndexes)
                    {
                        Handles.Label(e.Origin, e.Origin.Index.ToString());
                        Handles.Label(e.Center, e.Index.ToString()); 
                    }

                    if (ShowDistrictBorders)
                    {
                        Gizmos.color = Color.black;
                        if(e.Other() == null)
                            Gizmos.color = Color.red;
                        ExtendedGizmos.DrawLineDirection(((Vector2)e.Origin).MovePointInDirectionOfPoint(district.Center, 0.02f), 
                            ((Vector2)e.Destination).MovePointInDirectionOfPoint(district.Center, 0.02f));

                    }
                }            
                
                if (ShowDistrictBounds)
                {
                    // Draw Bounding Box of district
                    Gizmos.color = Color.green;
                
                    Gizmos.DrawLine(district.BoundingBox.BottomLeft(), district.BoundingBox.BottomRight());
                    Gizmos.DrawLine(district.BoundingBox.TopLeft(), district.BoundingBox.TopRight());
                
                    Gizmos.DrawLine(district.BoundingBox.BottomLeft(), district.BoundingBox.TopLeft());
                    Gizmos.DrawLine(district.BoundingBox.BottomRight(), district.BoundingBox.TopRight());
                }
            }
        }
        private void DrawNeighborhoodPatterns()
        {
            if(!ShowNeighborhoods || CityConfiguration.DistrictLayer.PolygonIdToDistrictMap == null)
                return;
            
            foreach (var d in CityConfiguration.DistrictLayer.PolygonIdToDistrictMap.Values)
            {
                if(d == null || d.Neigborhoods == null)
                    continue;
                
                // Show neighborhood Sites
                //foreach (var site in CityConfiguration.NeighborhoodLayer.Sites)
                //{
                //    Gizmos.color = Color.white;
                //    Gizmos.DrawSphere(site, 0.005f);
                //}
                
                foreach (var n in d.Neigborhoods)
                {
                    if (ShowNeighborhoodIndexes)
                    {
                        Handles.Label(n.Center, n.Index.ToString());
                    }
                    int cnt = 0;

                    foreach (var e in n.Edges)
                    {
                        if (ShowNeighborhoodIndexes)
                        {
                            Handles.Label(e.Origin + Vector2.down * Mathf.Sin(e.Origin.Index + 10 * cnt) / 500, e.Origin.Index.ToString());
                            Handles.Label(e.Center  + Vector2.down * Mathf.Sin(e.Index + 10 * cnt) / 500, e.Index.ToString());
                        }

                        if (ShowNeighborhoodBorders)
                        {
                            Gizmos.color = Color.black;

                            if(e.Other() == null)
                                Gizmos.color = Color.red;
                            
                            ExtendedGizmos.DrawLineDirection(((Vector2)e.Origin).MovePointInDirectionOfPoint(n.Center, 0.02f), 
                                ((Vector2)e.Destination).MovePointInDirectionOfPoint(n.Center, 0.02f));
                        }
                        cnt++;
                    }

                    if (ShowNeighborhoodRegions && CityConfiguration.WardLayer.Regions != null)
                    {
                        foreach (var region in CityConfiguration.WardLayer.Regions)
                        {
                            foreach (var tr in region.Triangles)
                            {
                                Gizmos.color = Color.blue;

                                Gizmos.DrawSphere(tr.Center, 0.001f);

                                Gizmos.color = Color.green;
                                
                                
                                Gizmos.DrawLine(tr.P1, tr.P2);
                                Gizmos.DrawLine(tr.P2, tr.P3);
                                Gizmos.DrawLine(tr.P3, tr.P1);
                            }
                        }
                    }
                    if (ShowNeighborhoodBounds)
                    {
                        // Draw Bounding Box of the neighborhood
                        Gizmos.color = Color.green;
                
                        Gizmos.DrawLine(n.BoundingBox.BottomLeft(), n.BoundingBox.BottomRight());
                        Gizmos.DrawLine(n.BoundingBox.TopLeft(), n.BoundingBox.TopRight());
                
                        Gizmos.DrawLine(n.BoundingBox.BottomLeft(), n.BoundingBox.TopLeft());
                        Gizmos.DrawLine(n.BoundingBox.BottomRight(), n.BoundingBox.TopRight());
                    }
                }
            }
        }

        private void DrawCityCores()
        {
            var c = Gizmos.color;
            Gizmos.color = Color.yellow;

            foreach (var sp in CityConfiguration.CityLayer.CityCores)
            {
                Gizmos.DrawSphere(sp, .002f);

            }
            Gizmos.color = c;
        }

        private void DrawStreetControlPoints()
        {
            for (int i = 1; i < ((ICollection) CityConfiguration.PrimaryStreetLayer.StreetMesh.vertices).Count; i++)
            {
                Vector2 p0 = CityConfiguration.PrimaryStreetLayer.StreetMesh.vertices[i-1];
                Vector2 p1 = CityConfiguration.PrimaryStreetLayer.StreetMesh.vertices[i];
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(p0, .01f);
            }
        }

        private void DrawCityElements()
        {
            if(!ShowCityCore)
                return;
            
            foreach (var core in CityConfiguration.CityLayer.CityCores)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(core, .005f);

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(core, .002f);
            }

            if (ShowCityIndexes && CityConfiguration.CityLayer.CityCanvas != null)
            {
                Handles.Label(CityConfiguration.CityLayer.CityCanvas.Center, CityConfiguration.CityLayer.CityCanvas.Index.ToString());

                foreach (var edge in CityConfiguration.CityLayer.CityCanvas.Edges)
                {
                    Handles.Label(edge.Center, edge.Index.ToString());
                    Handles.Label(edge.Origin, edge.Origin.Index.ToString());
                }
            }
            
            if(ShowCityBounds && CityConfiguration.CityLayer.CityCanvas != null)
            {
                // Draw Bounding Box of district
                Gizmos.color = new Color(0f, .5f, 0f, 1f);
    
                var c = CityConfiguration.CityLayer.CityCanvas;
                
                Gizmos.DrawLine(CityConfiguration.CityLayer.CityCanvas.BoundingBox.BottomLeft(), CityConfiguration.CityLayer.CityCanvas.BoundingBox.BottomRight());
                Gizmos.DrawLine(CityConfiguration.CityLayer.CityCanvas.BoundingBox.TopLeft(), CityConfiguration.CityLayer.CityCanvas.BoundingBox.TopRight());
                Gizmos.DrawLine(CityConfiguration.CityLayer.CityCanvas.BoundingBox.BottomLeft(), CityConfiguration.CityLayer.CityCanvas.BoundingBox.TopLeft());
                Gizmos.DrawLine(CityConfiguration.CityLayer.CityCanvas.BoundingBox.BottomRight(), CityConfiguration.CityLayer.CityCanvas.BoundingBox.TopRight());
                
            }

        }
        
        private void DrawStreetGraph(Graph<Vertex> graph, Color c)
        {
            if(!ShowStreets)
                return;

            Gizmos.color = c;

            List<Vertex> closedNodes = new List<Vertex>();
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                var node = graph.Nodes[i];
                closedNodes.Add(node);
                var currentNode = node;
                var neighbors = graph.GetNeighbors(node);
                Gizmos.DrawSphere(currentNode, .005f);

                Gizmos.color = Color.magenta;
                for (var j = 0; j < neighbors.Count; j++)
                {
                    if (closedNodes.Contains(neighbors[j]))
                    {
                        continue;
                    }
                    
                    Gizmos.DrawLine(currentNode, neighbors[j]);
                }
            }
        }
    }
}