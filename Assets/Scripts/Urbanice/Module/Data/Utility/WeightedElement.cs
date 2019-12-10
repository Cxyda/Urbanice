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

        public WeightedElement(T element, int weight)
        {
            Element = element;
            Weight = weight;
        }
    }
    
    [Serializable]
    public class DistrictWeight : WeightedElement<DistrictType>
    {
        public DistrictWeight(DistrictType element, int weight) : base(element, weight)
        {
        }
    }
    
    [Serializable]
    public class WardWeight : WeightedElement<WardType>
    {
        public WardWeight(WardType element, int weight) : base(element, weight)
        {
        }
    }
}