using System;
using UnityEngine;
using Urbanice.Generators._1D;

namespace Urbanice.Module.Data.Utility
{
    public abstract class ValueOrRange<T, V>
    {
        [Range(0, 10)] public T FixedValue;
        public bool UseRangedValue;

        public T MinValue;
        public T MaxValue;

        public PseudoRandomGenerator RandomGenerator;

        public abstract V GetValue();
    }
    [Serializable]
    public class IntValueOrRange : ValueOrRange<int, int>
    {
        public override int GetValue()
        {
            if (!UseRangedValue)
                return FixedValue;

            var delta = Math.Abs(MaxValue) - Math.Abs(MinValue);
            return (int) (RandomGenerator.Generate() * delta) + MinValue;
        }
    }
    [Serializable]
    public class FloatValueOrRange : ValueOrRange<float, float>
    {
        public override float GetValue()
        {
            if (!UseRangedValue)
                return FixedValue;

            var delta = Math.Abs(MaxValue) - Math.Abs(MinValue);
            return (RandomGenerator.Generate() * delta) + MinValue;
        }
    }
    [Serializable]
    public class IFValueOrRange : ValueOrRange<int, float>
    {
        public override float GetValue()
        {
            if (!UseRangedValue)
                return FixedValue;

            var delta = Math.Abs(MaxValue) - Math.Abs(MinValue);
            return (RandomGenerator.Generate() * delta) + MinValue;
        }
    }
}