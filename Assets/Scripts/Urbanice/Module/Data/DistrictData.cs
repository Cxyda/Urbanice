using System;
using System.Collections.Generic;
using Urbanice.Data;
using Urbanice.Utils;

namespace Urbanice.Module.Data
{
    /// <summary>
    /// This class stores all data regarding Districts
    /// </summary>
    [Serializable]
    public class DistrictData
    {
        public Polygon Shape;
        public DistrictType Type;
        public WardType InitialType;

        public List<Polygon> Neigborhoods;

        private List<Vertex> _innerPoints;
        public DistrictData(DistrictType type, Polygon shape)
        {
            Shape = shape;
            Type = type;
            
            Neigborhoods = new List<Polygon>();
            _innerPoints = new List<Vertex>();
        }
    }
}