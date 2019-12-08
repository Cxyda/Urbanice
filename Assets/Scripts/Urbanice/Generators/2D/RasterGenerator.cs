using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._1D;
using Urbanice.Generators._2D.Grid;
using Urbanice.Maniplulators;

namespace Urbanice.Generators._2D
{
    [CreateAssetMenu(menuName = "Urbanice/Generators/Patterns/Raster", fileName = "new Raster Generator", order = 2)]
    public class RasterGenerator : PatternGenerator, IPatternGenerator<Vector2>
    {
        private Raster _raster;
        public Vector2Int GridSegments = new Vector2Int(3,3);
        public FloatGenerator ValueGenerator;

        [Header("Shape Manipulators")] public List<ShapeRelaxManipulator> ShapeManipulators;

        
        public override void Init()
        {
            _raster = new Raster(ValueGenerator, this, ShapeManipulators);

        }

        public override List<Polygon> Generate(List<Vector2> points, Polygon outsideShape, bool connectToOutside)
        {
            GeneratedPolygons = _raster.Generate(points, outsideShape, connectToOutside);
            return GeneratedPolygons;
        }
    }
}