using System;
using System.Collections.Generic;
using Urbanice.Data;
using Urbanice.Utils;

namespace Urbanice.Module.Data
{
    [Serializable]
    public class DistrictData
    {
        public Polygon Shape;
        public DistrictType Type;
        public WardType InitialType;

        public List<Polygon> Neigborhoods;
        public List<Vertex> InnerPoints => new List<Vertex>(_innerPoints);
        public List<Vertex> OuterPoints;
        
        public Graph<Vertex> Streets;
        
        public bool WithinWalls;
        public bool WithinCity;

        private List<Vertex> _innerPoints;
        public DistrictData(DistrictType type, Polygon shape)
        {
            Shape = shape;
            Type = type;
            
            Neigborhoods = new List<Polygon>();
            OuterPoints = new List<Vertex>(shape.Points);
            _innerPoints = new List<Vertex>();
            
            Streets = new Graph<Vertex>();
            
            WithinCity	= false;
            WithinWalls	= false;
        }
    }
}