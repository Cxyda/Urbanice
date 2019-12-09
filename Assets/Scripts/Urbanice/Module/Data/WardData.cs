using System;
using Urbanice.Data;

namespace Urbanice.Module.Data
{
    /// <summary>
    /// A simple WardData class, this can be extended later to add more functionality
    /// </summary>
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