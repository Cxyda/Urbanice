using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._1D;
using Urbanice.Maniplulators;

namespace Urbanice.Generators._2D
{
    /// <summary>
    /// Voronoi pattern generator class serves as a container for the actual generator instance
    /// </summary>
    [CreateAssetMenu(menuName = "Urbanice/Generators/Patterns/Voronoi", fileName = "new Voronoi Generator", order = 1)]
    public class VoronoiGenerator : PatternGenerator
    {
        private SimpleVoronoi.SimpleVoronoi _voronoi;

        public FloatGenerator ValueGenerator;

        [Header("Point Attributes")] 
        [Range(0.01f, 0.2f)]
        public float MinPointDistance = 0.001f;
        public AnimationCurve PointDistribution = AnimationCurve.Linear(0, 0, 1, 1);

        [Range(1, 20)]
        public int CellDensity = 10;

        [Header("Shape Manipulators")] public List<ShapeRelaxManipulator> ShapeManipulators;

        public override List<Polygon> Generate(List<Vector2> points, Polygon outsideShape, bool connectToOutside)
        {
            GeneratedPolygons = _voronoi.Generate(points, outsideShape, connectToOutside);
            
            return GeneratedPolygons;
        }

        public override void Init()
        {
            _voronoi = new SimpleVoronoi.SimpleVoronoi(ValueGenerator, this, ShapeManipulators);

        }
    }
}