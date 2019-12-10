using System;
using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._2D.SimpleVoronoi;

namespace Urbanice.Generators._2D
{
    /// <summary>
    /// Base class for all PatternGenerator classes
    /// </summary>
    public abstract class PatternGenerator : ScriptableObject, IPatternGenerator<Vector2>
    {
        [NonSerialized]
        public List<Polygon> GeneratedPolygons;
        [NonSerialized]
        public List<Vertex> ControlPoints;

        public abstract void Init();
        public abstract List<Polygon> Generate(List<Vector2> points, Polygon outsideShape, bool connectToOutside);
    }
}