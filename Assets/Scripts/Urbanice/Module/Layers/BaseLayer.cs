using UnityEngine;
using Urbanice.Module.Data;

namespace Urbanice.Module.Layers
{
    /// <summary>
    /// The base layer class for all urbanice layers
    /// </summary>
    public abstract class BaseLayer : ScriptableObject
    {
        public LayerVisibility Visibility;
    }
}