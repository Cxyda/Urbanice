using UnityEngine;

namespace Urbanice.Generators._1D
{
    /// <summary>
    /// This generic class is the base class for all value generators
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueGenerator<T> : ScriptableObject, IGenerator<T>
    {
        public abstract void Init();

        public abstract T Generate();
    }
    /// <summary>
    /// This wrapper class is required since unity is not able to serialize generic classes
    /// </summary>
    public abstract class FloatGenerator : ScriptableObject, IValueGenerator<float>
    {
        public abstract void Init();

        public abstract float Generate();
    }
}