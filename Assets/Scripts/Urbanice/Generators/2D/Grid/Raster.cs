using System.Collections.Generic;
using UnityEngine;
using Urbanice.Data;
using Urbanice.Maniplulators;
using Urbanice.Utils;

namespace Urbanice.Generators._2D.Grid
{
    public class Raster
    {
        private RasterGenerator _rasterGenerator;

        private List<List<Vector2>> _gridPoints;
        private List<ShapeRelaxManipulator> _manipulators;
        private Polygon OutSideShape;

        public List<Polygon> Regions;

        public Raster(IValueGenerator<float> random, RasterGenerator parameters, List<ShapeRelaxManipulator> manipulators)
        {
            _rasterGenerator = parameters;
            _manipulators = manipulators;
        }

        public List<Polygon> Generate(List<Vector2> additionalControlPoints, Polygon outsideShape, bool connectToOutsideShape)
        {
            _gridPoints = new List<List<Vector2>>();
            Regions = new List<Polygon>();
            OutSideShape = outsideShape;
            
            GenerateGridPoints();
            GenerateRegions();

            if (_manipulators != null)
            {
                foreach (var manipluator in _manipulators)
                {
                    foreach (var polygon in Regions)
                    {
                        manipluator.Manipluate(polygon);
                    }
                }
            }
            Regions = GeometryUtils.RemoveOutsidePolygons(Regions, OutSideShape);

            if (connectToOutsideShape)
            {
                var borderCells = GeometryUtils.CreateBorderPolygons(Regions, outsideShape);
                Regions.AddRange(borderCells);
            }
            
            return Regions;
        }

        private void GenerateRegions()
        {
            for (var x = 0; x < _gridPoints.Count - 1; x++)
            {
                for (var y = 0; y < _gridPoints[x].Count - 1; y++)
                {
                    var px0 = _gridPoints[x][y];
                    var py0 = _gridPoints[x][y+1];
                    var py1 = _gridPoints[x+1][y+1];
                    var px1 = _gridPoints[x+1][y];
                    
                    var edgeList = new List<HalfEdge>(4);

                    var e1 = new HalfEdge(Vertex.Factory.Create(px0), Vertex.Factory.Create(py0));
                    var e2 = new HalfEdge(Vertex.Factory.Create(py0), Vertex.Factory.Create(py1));
                    var e3 = new HalfEdge(Vertex.Factory.Create(py1), Vertex.Factory.Create(px1));
                    var e4 = new HalfEdge(Vertex.Factory.Create(px1), Vertex.Factory.Create(px0));

                    e1.NextEdge = e3.PreviousEdge = e2;
                    e2.NextEdge = e4.PreviousEdge = e3;
                    e3.NextEdge = e1.PreviousEdge = e4;
                    e4.NextEdge = e2.PreviousEdge = e1;
                    
                    edgeList.AddMultiple(e1, e2, e3, e4);
                    
                    var p = new Polygon(edgeList);

                    Regions.Add(p);
                }
            }
        }

        private void GenerateGridPoints()
        {
            float segmentLength = OutSideShape.BoundingBox.size.y / _rasterGenerator.GridSegments.x;
            for (int i = 0; i < _rasterGenerator.GridSegments.x+1; i++)
            {
                Vector2 ps =  new Vector2(OutSideShape.BoundingBox.min.x, OutSideShape.BoundingBox.min.y + segmentLength * i ) ;
                Vector2 pe = new Vector2(OutSideShape.BoundingBox.max.x, y: OutSideShape.BoundingBox.min.y + segmentLength * i ) ;
                var h = GeometryUtils.SubdivideLineSegment(ps,pe, _rasterGenerator.GridSegments.y - 1);
                
                _gridPoints.Add(h);
            }
        }
    }
}