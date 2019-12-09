using System;

namespace Urbanice.Module.Data.Utility
{
    /// <summary>
    /// Generic WeightedElement class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WeightedElement<T>
    {
        public T Element;
        public int Weight;
    }
    
    [Serializable]
    public class DistrictWeight : WeightedElement<DistrictType>
    {

    }
    
    [Serializable]
    public class WardWeight : WeightedElement<WardType>
    {

    }
}