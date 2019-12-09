using UnityEngine;

namespace Urbanice.Utils
{
    /// <summary>
    /// This class provides <see cref="Gizmos"/> extension functionality for easier use
    /// </summary>
    public static class ExtendedGizmos
    {
        private static float _arrowHeadAngle = 20f;
        private static float _arrowHeadLength = .005f;

        public static void DrawLineDirection(Vector3 start, Vector3 end)
        {
            var direction = end - start;
            var pos = start + direction * .5f;
            Gizmos.DrawSphere(start, 0.0005f);

            direction.Normalize();
            
            var right = Quaternion.AngleAxis(180 -_arrowHeadAngle, Vector3.back) * direction;
            var left = Quaternion.AngleAxis(180 + _arrowHeadAngle, Vector3.back) * direction;

            Gizmos.DrawLine(start, end);

            Gizmos.DrawRay(pos, right * _arrowHeadLength);
            Gizmos.DrawRay(pos, left * _arrowHeadLength);
        }//
    }
}