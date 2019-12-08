using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Generators._2D.SimpleVoronoi;
using Urbanice.Utils;

namespace Urbanice.Generators._3D.Mesh
{
    /// <summary>
    /// This class can be used to generate meshes to render them later
    /// </summary>
    public static class PrimitiveGenerator
    {
        public static UnityEngine.Mesh CombineMesh(UnityEngine.Mesh m1, UnityEngine.Mesh m2)
        {
            UnityEngine.Mesh m = new UnityEngine.Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();

            vertices.AddRange(m1.vertices);
            vertices.AddRange(m2.vertices);
            
            triangles.AddRange(m1.triangles);

            // update triangleIndices
            for (var i = 0; i < m2.triangles.Length; i++)
            {
                triangles.Add(m2.triangles[i] + m1.vertices.Length);
            }
            
            normals.AddRange(m1.normals);
            normals.AddRange(m2.normals);

            m.vertices = vertices.ToArray();
            m.triangles = triangles.ToArray();
            m.normals = normals.ToArray();
             
            return m;
        }
        public static UnityEngine.Mesh GenerateLine(List<Vector2> line, float thickness)
        {
            UnityEngine.Mesh lineMesh = new UnityEngine.Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();

            var halfThickness = thickness * .5f;
            
            for (var n = 1; n < line.Count; n++)
            {
                var vertexId = (n-1) * 4;
                
                var previousPoint = line[n-1];
                var currentPoint = line[n];

                // Calculate directionVector
                var segmentVector = previousPoint - currentPoint;
                // Generate 4 new vertices along the line with half thickness
                Vector3 normal3 = Vector3.Cross(new Vector3(segmentVector.x, 0, segmentVector.y).normalized, Vector3.up);
                UnityEngine.Vector2 normal2 = new UnityEngine.Vector2(normal3.x, normal3.z);
                
                // TODO: this can be optimized, after the first segment those have been calculated already
                Vector2 s1 = previousPoint + normal2 * halfThickness;
                Vector2 s2 = previousPoint - normal2 * halfThickness;
                
                Vector2 e1 = currentPoint + normal2 * halfThickness;
                Vector2 e2 = currentPoint - normal2 * halfThickness;
                
                if (vertices.Count > 0)
                {
                    // Weld vertices
                    // calculate weld position,
                    // TODO: use a more advanced approach to calculate the position to avoid pinching in corners
                    Vector2 bisect1 = GeometryUtils.BisectSegment(vertices[vertices.Count-2], s1);
                    Vector2 bisect2 = GeometryUtils.BisectSegment(vertices[vertices.Count-1], s2);

                    vertices[vertices.Count - 2] = bisect1;
                    vertices[vertices.Count - 1] = bisect2;
                    s1 = bisect1;
                    s2 = bisect2;
                }
                
                vertices.AddMultiple(s1,s2,e1,e2);
                //generate triangles
                triangles.AddMultiple(vertexId + 0, vertexId + 1, vertexId + 2);
                triangles.AddMultiple(vertexId + 1, vertexId + 3, vertexId + 2);

                normals.AddMultiple(Vector3.up, Vector3.up, Vector3.up, Vector3.up);
            }
            
            lineMesh.vertices = vertices.ToArray();
            lineMesh.triangles = triangles.ToArray();
            lineMesh.normals = normals.ToArray();
            
            return lineMesh;
        }

        public static UnityEngine.Mesh GeneratePolygon(Polygon polygon)
        {
            UnityEngine.Mesh polyMesh = new UnityEngine.Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            
            var center = polygon.Center;
            vertices.Add(center);
            normals.Add(Vector3.up);

            for (int n = 0; n < polygon.Points.Count; n++)
            {
                var n1 = (n+1) % polygon.Points.Count;

                var p0 = polygon.Points[n];
                var p1 = polygon.Points[n1];

                Triangle t = new Triangle(center, p0, p1);
                if (!vertices.Contains(t.P2))
                {
                    vertices.Add(t.P2);
                    normals.Add(Vector3.forward);
                }

                if (!vertices.Contains(t.P3))
                {
                    vertices.Add(t.P3);
                    normals.Add(Vector3.forward);
                }

                int index2 = vertices.IndexOf(t.P2);
                int index3 = vertices.IndexOf(t.P3);
                
                triangles.AddMultiple(0, index2, index3);
            }

            polyMesh.vertices = vertices.ToArray();
            polyMesh.triangles = triangles.ToArray();
            polyMesh.normals = normals.ToArray();
            
            return polyMesh;
        }
    }
}