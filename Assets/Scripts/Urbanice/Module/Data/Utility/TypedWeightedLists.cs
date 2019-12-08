namespace Urbanice.Module.Data.Utility
{
    /// <summary>
    /// Since unity is unable to serialized generic types these wrappers are mandatory
    /// </summary>
    public class WeightedDistrictList : WeightedList<DistrictWeight, DistrictType>
    {
        // nothing to do
    }
    
    public class WeightedNWardList : WeightedList<WardWeight, WardType>
    {
        // nothing to do
    }
}