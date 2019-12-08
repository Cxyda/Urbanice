using System;

namespace Urbanice.Module.Data.Utility
{
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