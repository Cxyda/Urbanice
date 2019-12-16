using System.Diagnostics;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._1D.Random;
using Urbanice.Module.Data;
using Urbanice.Module.Layers;
using Debug = UnityEngine.Debug;
using TerrainLayer = Urbanice.Module.Layers.TerrainLayer;

namespace Urbanice.Module.Containers
{
    /// <summary>
    /// This class serves as the central point of the city generation.
    /// It holds all references to the used layers and triggers the generation process
    /// </summary>
    [CreateAssetMenu(menuName = "Urbanice/DefinitionContainers/Create new CityDefinition", fileName = "unknown CityDefinition", order = -10)]
    public class CityDefinitionContainer : ScriptableObject
    {
        public string GlobalSeed;

        public TerrainLayer TerrainData;

        public CityLayer CityLayer;
        public DistrictLayer DistrictLayer;
        public WardLayer WardLayer;
        public StreetLayer PrimaryStreetLayer;

        private void CleanupSerializedData()
        {
            Vertex.VertexCount = 0;
            Vertex.Factory.ClearData();
            
            HalfEdge.HalfEdgeCount = 0;
            Polygon.PolygonCount = 0;
        }
        public void GenerateCity()
        {
            CleanupSerializedData();

            GlobalPRNG.Init(GlobalSeed);
            
            TerrainData.Init();
            CityLayer.Init();
            DistrictLayer.Init();
            WardLayer.Init();
            PrimaryStreetLayer.Init();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            TerrainData.Generate(null);

            if (CityLayer.Visibility == LayerVisibility.Generate)
            {
                CityLayer.Generate(null);
            }

            if (DistrictLayer.Visibility == LayerVisibility.Generate)
            {
                DistrictLayer.Generate(CityLayer);
            }

            if (WardLayer.Visibility == LayerVisibility.Generate)
            {
                WardLayer.Generate(DistrictLayer);
            }
            if (PrimaryStreetLayer.Visibility == LayerVisibility.Generate)
            {
                PrimaryStreetLayer.Generate(DistrictLayer);
            }
            
            sw.Stop();
            Debug.Log($"GenerationTime: {sw.Elapsed.Milliseconds} ms" );
        }
    }
}