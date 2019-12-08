using System;
using System.Collections.Generic;
using UnityEngine;
using Urbanice.Module.Data;
using Urbanice.Module.Districts;

namespace Urbanice.Module.Containers
{
    [CreateAssetMenu(menuName = "Urbanice/DefinitionContainers/DistrictDefinitionContainer", fileName = "new DistrictDefinitionContainer", order = 10)]
    public class DistrictConfigurationContainer : ScriptableObject
    {
        private Dictionary<DistrictType, DistrictConfig> _typeConfigMap = new Dictionary<DistrictType, DistrictConfig>();
        
        public List<DistrictConfig> DistrictConfigs = new List<DistrictConfig>();

        public DistrictConfig GetDefinitionFor(DistrictType type)
        {
            if (!_typeConfigMap.TryGetValue(type, out DistrictConfig config))
            {
                foreach (var cfg in DistrictConfigs)
                {
                    if(type != cfg.DistrictType)
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