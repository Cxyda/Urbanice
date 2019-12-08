using Urbanice.Module.Data;

namespace Urbanice.Wards
{
    public class Ward
    {
        public DistrictData DistrictData;

        public Ward(DistrictData districtData)
        {
            DistrictData = districtData;
        }
    }
}