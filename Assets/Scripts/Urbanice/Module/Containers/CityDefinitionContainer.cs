using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._1D.Random;
using Urbanice.Module.Data;
using Urbanice.Module.Layers;
using TerrainLayer = Urbanice.Module.Layers.TerrainLayer;

namespace Urbanice.Module.Containers
{
    [CreateAssetMenu(menuName = "Urbanice/DefinitionContainers/Create new CityDefinition", fileName = "unknown CityDefinition", order = -10)]
    public class CityDefinitionContainer : ScriptableObject
    {
        public string GlobalSeed;

        public TerrainLayer TerrainData;

        public CityLayer CityLayer;
        public DistrictLayer DistrictLayer;
        public WardLayer WardLayer;
        [HideInInspector]public PrimaryStreetLayer PrimaryStreetLayer;

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
            
            TerrainData.Generate(null);

            if (CityLayer.Visibility == LayerVisibility.Generate)
            {
                Debug.Log($"+++++++++++ Generating CityLayer +++++++++++");
                CityLayer.Generate(null);
                Debug.Log($"----------- DONE! -----------");
            }

            if (DistrictLayer.Visibility == LayerVisibility.Generate)
            {
                Debug.Log($"+++++++++++ Generating DistrictLayer +++++++++++");
                DistrictLayer.Generate(CityLayer);
                Debug.Log($"----------- DONE! -----------");
            }

            if (WardLayer.Visibility == LayerVisibility.Generate)
            {
                Debug.Log($"+++++++++++ Generating NeighborhoodLayer +++++++++++");
                WardLayer.Generate(DistrictLayer);
                Debug.Log($"----------- DONE! -----------");
            }





            //PrimaryStreetLayer.Generate(DistrictLayer);
        }
    }
}