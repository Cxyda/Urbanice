using System;
using System.Collections.Generic;
using UnityEngine;

namespace Urbanice.Generators._2D.SimpleVoronoi
{
    [Serializable]
    public struct Cell
    {
        public List<Vector2> Points => new List<Vector2>(_points);

        public Vector2 Center => _center;
        public float Area => _area;
        
        public List<Vector2> _points;
        
        private Vector2 _center;
        private float _area;

        public Cell(List<Vector2> vertices)
        {
            _center = new Vector2();
            _area = 0f;

            _points = vertices;
            CalculateCenter();
        }

        private void CalculateCenter()
        {
            CalculateArea();
            var xs = 0f;
            var ys = 0f;
            
            for (var n = 0; n < _points.Count; n++)
            {
                var n1 = (n + 1) % _points.Count;
                xs += (_points[n].x + _points[n1].x) * (_points[n].x * _points[n1].y - _points[n1].x * _points[n].y);
                ys += (_points[n].y + _points[n1].y) * (_points[n].x * _points[n1].y - _points[n1].x * _points[n].y);
            }

            xs *= 1f / (6 * _area);
            ys *= 1f / (6 * _area);

            _center = new Vector2(xs, ys);
        }
        private void CalculateArea()
        {
            _area = 0f;

            for (var n = 0; n < _points.Count; n++)
            {
                var n1 = (n + 1) % _points.Count;
                _area += ((_points[n].x * _points[n1].y) - (_points[n1].x * _points[n].y));
            }

            _area *= 0.5f;
        }
    }
}