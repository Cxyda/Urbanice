using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Maniplulators.Shape;

namespace Urbanice.Maniplulators
{
    /// <summary>
    /// Simple Relax manipulator, can be extended later with better functionality
    /// </summary>
    [CreateAssetMenu(menuName = "Urbanice/Manipulators/Shape/Relax", fileName = "new ShapeRelax Manipulator", order = 1)]
    public class ShapeRelaxManipulator : ScriptableObject, IShapeManipulator
    {
        [Range(0, 1f)] public float Amount = .33f;
        [Range(0, 10)] public int Iterations = 1;
        
        public List<Polygon> Manipluate(List<Polygon> input)
        {
            var relaxManipulator = new ShapeRelax(this);

            return relaxManipulator.Manipulate(input);
        }
    }
}