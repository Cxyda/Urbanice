using System;
using Urbanice.Data;

namespace Urbanice.Module.Data
{
    [Serializable]
    public class WardData
    {
        public WardType Type;
        public Polygon Shape;
        
        public WardData(WardType type, Polygon shape)
        {
            Type = type;
            Shape = shape;
        }
    }
}