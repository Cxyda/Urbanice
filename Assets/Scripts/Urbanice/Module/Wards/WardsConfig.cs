using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Urbanice.Module.Data;
using Urbanice.Module.Data.Utility;

namespace Urbanice.Module.Wards
{
    /// <summary>
    /// This class provides all current parameters to manipulate Wards generateion process
    /// </summary>
    [CreateAssetMenu(menuName = "Urbanice/Configs/Create new Neighborhood Config", fileName = "newNeighborhoodConfig",
        order = 2)]
    public class WardsConfig : ScriptableObject
    {
        public WardType WardType;

        [Space]
        [Tooltip("This Type can spwan in the following districts")]
        public List<DistrictType> DistrictTypes;

        public Color RenderColor;

        [FormerlySerializedAs("NeighboringDistricts")] [Space]
        public List<WardWeight> NeighboringWards;
    }
}