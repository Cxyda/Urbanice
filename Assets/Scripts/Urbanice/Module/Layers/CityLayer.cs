using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._1D.Random;
using Urbanice.Module.Data;
using Urbanice.Utils;

namespace Urbanice.Module.Layers
{
    [CreateAssetMenu(menuName = "Urbanice/DataLayers/Create new City Layer", fileName = "newCityLayer", order = 2)]
    public class CityLayer : BaseLayer, IUrbaniceLayer
    {
        private static CityLayer _instance;

        public static int CitySize => (int)_instance.Size;
        
        [Header("City Core Configs")]
        public CityType Size;
        public AnimationCurve CityCorePlacement = AnimationCurve.Linear(0, 0, 1, 1);
        
        public Rect BoundingBox;

        private List<Vector2> _generatedCityCores;

        [HideInInspector]
        public Polygon CityCanvas;
        [HideInInspector]
        public List<Vector2> CityCores;

        public BaseLayer ParentLayer { get; private set; }

        public void Init()
        {
            _instance = this;
        }

        public void Generate(BaseLayer parentLayer)
        {
            _generatedCityCores = new List<Vector2>();
            CityCanvas = CanvasPolygon();
            
            while (_generatedCityCores.Count < (int)Size)
            {
                var x = CityCorePlacement.Evaluate(GlobalPRNG.Next());
                var y = CityCorePlacement.Evaluate(GlobalPRNG.Next());
            
                // TODO: Add filters
                _generatedCityCores.Add(new Vector2(Mathf.Lerp(BoundingBox.xMin, BoundingBox.xMax, x),
                    Mathf.Lerp(BoundingBox.yMin, BoundingBox.yMax, y)));
            }

            CityCores = _generatedCityCores;
        }

        private Polygon CanvasPolygon()
        {
            var v1 = Vertex.Factory.Create(BoundingBox.BottomLeft());
            var v2 = Vertex.Factory.Create(BoundingBox.TopLeft());
            var v3 = Vertex.Factory.Create(BoundingBox.TopRight());
            var v4 = Vertex.Factory.Create(BoundingBox.BottomRight());

            var e1 = new HalfEdge(v1,v2);
            var e2 = new HalfEdge(v2,v3, e1);
            var e3 = new HalfEdge(v3,v4, e2);
            var e4 = new HalfEdge(v4,v1, e3);

            e1.PreviousEdge = e4;
            e1.NextEdge = e2;
            e2.NextEdge = e3;
            e3.NextEdge = e4;
            e4.NextEdge = e1;

            var border = new List<HalfEdge>() {e1, e2, e3, e4};
            
            var canvasPolygon = new Polygon(border);

            // 2 Subdivs for now
            canvasPolygon.SubdivideAllEdgesAt(0.5f, false);
            canvasPolygon.SubdivideAllEdgesAt(0.5f, false);

            return canvasPolygon;
        }
    }
}