using System.Collections.Generic;
using Urbanice.Data;

namespace Urbanice.Maniplulators
{
    /// <summary>
    /// Base IManipulator interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IManipulator<T>
    {
        T Manipluate(T input);
    }

    public interface IShapeManipulator : IManipulator<List<Polygon>>
    {

    }
}