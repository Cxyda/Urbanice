using System;
using System.Collections.Generic;
using UnityEngine;
using Urbanice.Module.Data;
using Urbanice.Module.Wards;

namespace Urbanice.Module.Containers
{
    [CreateAssetMenu(menuName = "Urbanice/DefinitionContainers/WardDefinitionContainer", fileName = "new WardDefinitionContainer", order = 11)]
    public class WardDefinitionContainer : ScriptableObject
    {
        private Dictionary<WardType, WardsConfig> _typeConfigMap = new Dictionary<WardType, WardsConfig>();
        private Dictionary<DistrictType, List<WardType>> _districtTypeToWardTypeMap = new Dictionary<DistrictType, List<WardType>>();
        
        public List<WardsConfig> WardConfig = new List<WardsConfig>();

        public List<WardType> GetTypesFor(DistrictType districtType)
        {
            if (!_districtTypeToWardTypeMap.TryGetValue(districtType, out List<WardType> types))
            {
                types = new List<WardType>();
                foreach (var cfg in WardConfig)
                {
                    if(!cfg.DistrictTypes.Contains(districtType))
                        continue;

                    types.Add(cfg.WardType);
                }

                _districtTypeToWardTypeMap[districtType] = types;
            }

            return types;
        }
        
        public WardsConfig GetDefinitionFor(WardType type)
        {
            if (!_typeConfigMap.TryGetValue(type, out WardsConfig config))
            {
                foreach (var cfg in WardConfig)
                {
                    if(type != cfg.WardType)
                        continue;

                    config = cfg;
                    _typeConfigMap.Add(type, config);
                    return config;
                }
                throw new Exception($"{name} does not contain a definition for type {type}");
            }

            return config;
        }
    }
}