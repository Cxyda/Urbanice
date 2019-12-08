using System.Collections.Generic;
using UnityEngine;
using Urbanice.Module.Data;
using Urbanice.Module.Data.Utility;

namespace Urbanice.Module.Wards
{
    [CreateAssetMenu(menuName = "Urbanice/Configs/Create new Neighborhood Config", fileName = "newNeighborhoodConfig",
        order = 2)]
    public class WardsConfig : ScriptableObject
    {
        public WardType WardType;

        
        [Space]
        [Tooltip("This Type can spwan in the following districts")]
        public List<DistrictType> DistrictTypes;

        public Color RenderColor;

        [Space]
        public List<WardWeight> NeighboringDistricts;
    }
}