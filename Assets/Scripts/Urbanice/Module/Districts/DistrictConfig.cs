using System.Collections.Generic;
using UnityEngine;
using Urbanice.Generators._2D;
using Urbanice.Module.Data;
using Urbanice.Module.Data.Utility;

namespace Urbanice.Module.Districts
{
    [CreateAssetMenu(menuName = "Urbanice/Configs/Create new District Config", fileName = "newDistrictConfig", order = 1)]
    public class DistrictConfig : ScriptableObject
    {
        public DistrictType DistrictType;
        public Color RenderColor;

        [Space, Tooltip("Which Type will spawn initially on this district")]
        public WardType InitialSpawnType = WardType.Any;
        [Space]
        public PatternGenerator PatternGenerator;
        [Range(0f, 1f)]
        public float MinDistanceFromTownSquare;
        [Range(0f, 1f)]
        public float MaxDistanceFromTownSquare;
        
        [Range(0.1f, 10f)]
        public float MinSize;
        [Range(0.1f, 10f)]
        public float MaxSize;

        public List<DistrictWeight> NeighboringDistricts;
    }
}