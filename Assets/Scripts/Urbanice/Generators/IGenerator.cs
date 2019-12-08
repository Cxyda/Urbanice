using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;

namespace Urbanice.Generators
{
    /// <summary>
    /// Generic generator interfaces
    /// </summary>
    public interface IGenerator<T>
    {
        void Init();
    }
    public interface IValueGenerator<T> : IGenerator<T>
    {
        T Generate();
    }
    public interface IPatternGenerator<T>: IGenerator<T>
    {
        List<Polygon> Generate(List<T> points, Polygon outsideShape, bool connectToOutside);
    }
}