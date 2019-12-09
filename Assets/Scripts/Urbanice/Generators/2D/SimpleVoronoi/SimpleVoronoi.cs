using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Maniplulators;
using Urbanice.Module.Layers;
using Urbanice.Utils;

namespace Urbanice.Generators._2D.SimpleVoronoi
{
    public class SimpleVoronoi
    {
        public List<Region> Regions;

        public List<Triangle> Triangles;
        public List<Vector2> Sites;
        public List<Vector2> Frame;
        
        public List<Polygon> ClosedCells;

        private Rect BoundingBox;
        private Polygon OutSideShape;
        
        private IValueGenerator<float> _prng;

        private Dictionary<Vector2, Region> _regions;
        
        private List<ShapeRelaxManipulator> _manipulators;
        private VoronoiGenerator _parameters;

        public SimpleVoronoi(IValueGenerator<float> random, VoronoiGenerator parameters, List<ShapeRelaxManipulator> manipulatorses)
        {
            _prng = random;
            _prng.Init();
            _manipulators = manipulatorses;
            
            _parameters = parameters;

            }

        public List<Polygon> Generate(List<Vector2> additionalControlPoints, Polygon outsideShape, bool connectToOutsideShape)
        {
            OutSideShape = outsideShape;
            BoundingBox = OutSideShape.BoundingBox;

            List<Vector2> points = GenerateInnerPoints(additionalControlPoints);
                
            BuildRegions(points, OutSideShape);
            PartitionRegions();
            
            Sites.Sort((p1, p2) => MathUtils.Sign(Vector2.SqrMagnitude(p1) - Vector2.SqrMagnitude(p2)));

            CreateInnerCells();

            if (_manipulators != null)
            {
                foreach (var manipluator in _manipulators)
                {
                    manipluator.Manipluate(ClosedCells);
                }
            }
            ClosedCells = GeometryUtils.RemoveOutsidePolygons(ClosedCells, OutSideShape);

            if (connectToOutsideShape)
            {
                var borderCells = GeometryUtils.CreateBorderPolygons(ClosedCells, outsideShape);
                ClosedCells.AddRange(borderCells);
            }
            
            return ClosedCells;
        }

        private List<Vector2> GenerateInnerPoints(List<Vector2> initialPoints)
        {
            var points = new List<Vector2>(initialPoints);
            
            var sa = _prng.Generate() * 2 * Mathf.PI;
            int citySize = CityLayer.CitySize;
            var pointAmount = _parameters.CellDensity + (2*citySize) + (int)((1 + _prng.Generate() * 3) * 2f);

            int cnt = 0;
            for (int i = 0; i < pointAmount * 10; i++)
            {
                var a = sa + Mathf.Sqrt(i) * 5;

                var progress = (float) cnt / (pointAmount - initialPoints.Count);
                var evaluatedVal = 1 +_parameters.PointDistribution.Evaluate(progress);
                
                var radius = (evaluatedVal + citySize) * _prng.Generate();
                
                var xr = radius * Mathf.Cos(a);
                var yr = radius * Mathf.Sin(a);

                var p = new Vector2(xr + points[0].x, yr + points[0].y);

                EvaluatePoint(p);

                if (points.Count >= pointAmount)
                    break;
            }
            return MapPointsToBoundingBox(points);

            void EvaluatePoint(Vector2 p)
            {
                // Check min neighbor point distance
                var closest = p.FindClosestIn(points);
                if (closest.HasValue && Vector2.Distance(closest.Value, p) >= _parameters.MinPointDistance)
                {
                    points.Add(p);
                    cnt++;
                }
            }
        }

        private List<Vector2> MapPointsToBoundingBox(List<Vector2> points)
        {
            GeometryUtils.CalculateBounds(points, out float minx, out float miny, out float maxx, out float maxy);

            for (var i = 0; i < points.Count; i++)
            {
                var x = Mathf.InverseLerp(minx, maxx, points[i].x);
                var y = Mathf.InverseLerp(miny, maxy, points[i].y);

                x = Mathf.Lerp(BoundingBox.xMin, BoundingBox.xMin + BoundingBox.size.x, x);
                y = Mathf.Lerp(BoundingBox.yMin, BoundingBox.yMin + BoundingBox.size.y, y);

                points[i] = new Vector2(x,y);
            }

            return points;
        }

        private Polygon CreatePolygonFromRegion(Region region)
        {
            var points = new List<Vector2>();
            
            foreach (var s in region.Triangles)
            {
                points.Add(s.Center);
            }

            points = points.Distinct().ToList();
            var edges = new List<HalfEdge>(points.Count);

            HalfEdge previousEdge = null;

            for (var n = 0; n < points.Count; n++)
            {
                var n1 = (n + 1) % points.Count;

                Vertex p = Vertex.Factory.Create(points[n]);
                Vertex p1 = Vertex.Factory.Create(points[n1]);

                var newEdge = new HalfEdge(p, p1);
                if (previousEdge != null) previousEdge.NextEdge = newEdge;

                newEdge.PreviousEdge = previousEdge;
                
                previousEdge = newEdge;
                edges.Add(previousEdge);
            }

            edges[0].PreviousEdge = previousEdge;
            edges[edges.Count-1].NextEdge = edges[0];

            return new Polygon(edges);
        }

        public void AddPoint(Vector2 p)
        {
            var toSplit = new List<Triangle>();
            foreach (var t in Triangles)
            {
                if (Vector2.Distance(p, t.Center) < t.Radius)
                {
                    toSplit.Add(t);
                }
            }

            if (toSplit.Count == 0) return;
            
            Sites.Add(p);
            
            var a = new List<Vector2>();
            var b = new List<Vector2>();

            foreach (var t1 in toSplit)
            {
                var e1 = true;
                var e2 = true;
                var e3 = true;

                foreach (var t2 in toSplit)
                {
                    if(t1 == t2) continue;
                    
                    // If triangles have a common edge, it goes in opposite directions
                    if (e1 && t2.HasCommonEdge( t1.P2, t1.P1 )) 
                        e1 = false;
                    if (e2 && t2.HasCommonEdge( t1.P3, t1.P2 )) 
                        e2 = false;
                    if (e3 && t2.HasCommonEdge( t1.P1, t1.P3 )) 
                        e3 = false;

                    if (!(e1 || e2 || e3)) break;
                }
                
                if (e1) { 
                    if(!a.Contains(t1.P1)) a.Add( t1.P1 ); 
                    if(!b.Contains(t1.P2)) b.Add( t1.P2 ); }
                if (e2) { 
                    if(!a.Contains(t1.P2)) a.Add( t1.P2 ); 
                    if(!b.Contains(t1.P3)) b.Add( t1.P3 ); }
                if (e3) { 
                    if(!a.Contains(t1.P3)) a.Add( t1.P3 ); 
                    if(!b.Contains(t1.P1)) b.Add( t1.P1 ); }
            }

            if (a.Count > 0 && b.Count > 0)
            {
                var index = 0;

                foreach (var point in a)
                {
                    Triangles.Add( new Triangle( p, a[index], b[index] ) );
                    index = a.IndexOf( b[index] );
                }
            }

            foreach (var tr in toSplit)
            {
                Triangles.Remove( tr );
            }
        }

        private bool BuildRegion(Vector2 site, out Region region )
        {
            var triangles = new List<Triangle>();
            
            foreach (var tr in Triangles)
            {
                if (tr.P1 == site || tr.P2 == site || tr.P3 == site)
                    triangles.Add(tr);
            }

            if (triangles.Count > 0)
            {
                region = new Region( site, triangles );
                region.SortTriangles();
                return true;
            }

            region = default(Region);
            return false;
        }
        /**
        * Checks if neither of a triangle's vertices is a frame point
        **/
        private bool IsReal(Triangle tr)
        {
            return BoundingBox.Contains(tr.Center) && !(Frame.Contains( tr.P1 ) || Frame.Contains( tr.P2 ) || Frame.Contains( tr.P3 ));
        }

        private void BuildRegions(List<Vector2> points, Polygon outsideShape)
        {
            Triangles = new List<Triangle>();
            Frame = new List<Vector2>();
            Sites = new List<Vector2>();


            var outSidePoints = outsideShape.Points;
            
            var c1 = BoundingBox.BottomLeft();
            var c2 = BoundingBox.TopLeft();
            var c3 = BoundingBox.BottomRight();
            var c4 = BoundingBox.TopRight();
            
            Frame.Add(c1);
            Frame.Add(c2);
            Frame.Add(c3);
            Frame.Add(c4);

            Sites.AddRange(Frame);
            Triangles.Add(new Triangle(c1, c2, c3));
            Triangles.Add(new Triangle(c2, c3, c4));

            foreach (var p in points)
            {
                AddPoint(p);
            }
            
            foreach (var p in outSidePoints)
            {
                AddPoint(p);
            }
        }

        private void Relax()
        {
            var points = new List<Vector2>(Sites);
            Frame.ForEach(x =>
            {
                points.Remove(x);
            });
            List<Vector2> toRelax = Sites;

            foreach (var cell in Regions)
            {
                if (!toRelax.Contains(cell.Site)) continue;
                
                points.Remove(cell.Site);
                points.Add(cell.Center());
            }

            BuildRegions(points, OutSideShape);
        }

        private void CreateInnerCells()
        {
            ClosedCells = new List<Polygon>();
            
            foreach (var cell in Regions)
            {
                var polygon = CreatePolygonFromRegion(cell);
                ClosedCells.Add(polygon);
            }
        }
        private void PartitionRegions()
        {
            Regions = new List<Region>();

            // Iterating over points, not regions, to use points ordering
            foreach (var p in Sites)
            {
                if(!BuildRegion(p, out Region r))
                    continue;
                
                var isReal = true;
                foreach (var t in r.Triangles)
                {
                    if (IsReal(t)) continue;
                    isReal = false;
                    break;
                }

                if (isReal)
                {
                    r.IsClosed = true;
                    Regions.Add( r );
                }
            }
        }
    }

}
