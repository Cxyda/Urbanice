using Urbanice.Data;

namespace Urbanice.Maniplulators
{
    public interface IManipulator<T>
    {
        T Manipluate(T input);
    }

    public interface IShapeManipulator : IManipulator<Polygon>
    {

    }
}