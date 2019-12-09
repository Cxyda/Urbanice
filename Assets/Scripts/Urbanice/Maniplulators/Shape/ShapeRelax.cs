using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;

namespace Urbanice.Maniplulators.Shape
{
    /// <summary>
    /// This class implements a simple ShapeRelax algorithm
    /// </summary>
    public class ShapeRelax
    {
        private ShapeRelaxManipulator _manipulator;

        private float _averageAngle = 0f;
        private List<Vertex> closedVertices;
        
        
        public ShapeRelax(ShapeRelaxManipulator manipulator)
        {
            _manipulator = manipulator;
        }
        
/*
        public Polygon Manipulate(Polygon input)
        {
            for (var i = 0; i < _manipulator.Iterations; i++)
            {
                var edgeLengths = new List<float>();
                float sum = 0;
                for (var n = 0; n < input.Edges.Count; n++)
                {
                    var length = input.Edges[n].Length;
                    sum += length;
                    edgeLengths.Add(length);
                }

                float average = sum / input.Edges.Count;
                
                edgeLengths.Sort((a, b) => { return a > b ? -1 : 1; });
                
                for (int n = 0; n < edgeLengths.Count; n++)
                {
                    var factor = edgeLengths[n] / average;
                    input.Edges[n].ScaleEdge(1f / factor);
                }
            }

            return input;
        }
*/
        public List<Polygon> Manipulate(List<Polygon> input)
        {
            for (var j = 0; j < _manipulator.Iterations; j++)
            {
                closedVertices = new List<Vertex>();

                for (int i = 0; i < input.Count; i++)
                {
                    var polygon = input[i];
                    _averageAngle = 360f / polygon.Points.Count;

                    polygon = RelaxPolygon3(polygon);
                    polygon.Update();
                }
            }

            return input;
        }

        private Polygon RelaxPolygon3(Polygon input)
        {
            foreach (var point in input.Points)
            {
                if (closedVertices.Contains(point)) continue;

                var tension = Vector2.zero;
                foreach (var e in point.Edges)
                {
                    if (e.Origin != point) continue;

                    tension += e.Destination - point;
                }

                point.x += tension.x * _manipulator.Amount;
                point.y += tension.y * _manipulator.Amount;
                closedVertices.Add(point);
                input.Update();
                
                foreach (var e in point.Edges)
                {
                    e.Update();
                }
            }


            return input;
        }

        private Polygon RelaxPolygon2(Polygon input)
        {
            float shortestEdgeLength = float.MaxValue;
            float longestEdgeLength = 0f;
            
            foreach (var edge in input.Edges)
            {
                if (edge.Length > longestEdgeLength)
                {
                    longestEdgeLength = edge.Length;
                }
                if (edge.Length < shortestEdgeLength)
                {
                    shortestEdgeLength = edge.Length;
                }
            }

            float average = (shortestEdgeLength + longestEdgeLength) / 2f;

            for (int i = 0; i < input.Edges.Count; i++)
            {
                var factor = average / input.Edges[i].Length;
                input.Edges[i].ScaleEdge(1 - factor * _manipulator.Amount * 0.5f);
                input.Update();
            }

            return input;
        }

        private Polygon RelaxPolygon(Polygon input)
        {
            for (var n = 0; n < input.Points.Count; n++)
            {
                int n1 = (n + 1) % input.Points.Count;
                Vector2 d1 = input.Points[n];
                Vector2 d2 = input.Points[n1];
                
                var currentAngle = Vector2.Angle(d1, d2);
                var delta = currentAngle - _averageAngle;

                var newP1 = RotatePointAroundCenter(input.Points[n], input.Center, delta / 4);
                var newP2 = RotatePointAroundCenter(input.Points[n1], input.Center, -delta / 4);

                //input.Points[n].x = newP1.x;
                //input.Points[n].y = newP1.y;
                
                input.Points[n1].x = newP2.x;
                input.Points[n1].y = newP2.y;
            }

            return input;
        }

        private Vector2 RotatePointAroundCenter(Vertex p, Vector2 center, float angle)
        {
            Vector2 direction = p - center;
            direction = Quaternion.Euler(new Vector3(0,0,angle)) * direction;
            return center + direction;
        }
    }
}