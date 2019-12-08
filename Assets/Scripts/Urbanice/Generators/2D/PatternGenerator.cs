using System;
using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._2D.SimpleVoronoi;

namespace Urbanice.Generators._2D
{
    public abstract class PatternGenerator : ScriptableObject, IPatternGenerator<Vector2>
    {
        [NonSerialized]
        public List<Polygon> GeneratedPolygons;

        [NonSerialized]
        public List<Region> Regions;
        [NonSerialized]
        public List<Vector2> Sites;
        
        public abstract void Init();
        public abstract List<Polygon> Generate(List<Vector2> points, Polygon outsideShape, bool connectToOutside);
    }
}