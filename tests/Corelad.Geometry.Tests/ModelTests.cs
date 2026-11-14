using System;
using System.Linq;
using Corelad.Geometry.Analysis;
using Corelad.Geometry.Model;
using Corelad.Geometry.Tests.Support;
using Xunit;

namespace Corelad.Geometry.Tests
{
    public class ModelTests
    {
        [Fact]
        public void LineSegmentEvaluate_HitsEndpoints()
        {
            var segment = BezierSegment.Line(new Point2(1, 2), new Point2(5, 10));

            Assert.Equal(new Point2(1, 2), segment.Evaluate(0));
            Assert.Equal(new Point2(5, 10), segment.Evaluate(1));
            Assert.Equal(new Point2(3, 6), segment.Evaluate(0.5));
            Assert.Equal(0d, segment.Flatness, 12);
        }

        [Fact]
        public void CubicEvaluate_HitsEndpoints()
        {
            var segment = BezierSegment.Cubic(
                new Point2(0, 0), new Point2(0, 10), new Point2(10, 10), new Point2(10, 0));

            Assert.Equal(new Point2(0, 0), segment.Evaluate(0));
            Assert.Equal(new Point2(10, 0), segment.Evaluate(1));
            var middle = segment.Evaluate(0.5);
            Assert.Equal(5, middle.X, 9);
            Assert.Equal(7.5, middle.Y, 9);
        }

        [Fact]
        public void Split_PreservesEndpointsAndMidpoint()
        {
            var segment = BezierSegment.Cubic(
                new Point2(0, 0), new Point2(3, 9), new Point2(7, 9), new Point2(10, 0));

            segment.Split(0.5, out var left, out var right);

            Assert.Equal(segment.P0, left.P0);
            Assert.Equal(segment.P3, right.P3);
            Assert.Equal(segment.Evaluate(0.5), left.P3);
            Assert.Equal(segment.Evaluate(0.5), right.P0);
        }

        [Fact]
        public void CircleFlattening_AreaMatchesAnalyticWithinOnePercent()
        {
            const double radius = 50;
            var contour = TestGeometry.Circle(1, 0, 0, 0, radius);
            var tolerance = new GeometryTolerances();
            var flat = BezierFlattener.Flatten(contour, 0, tolerance);

            Assert.True(flat.Points.Count > 32);
            Assert.False(flat.HasBrokenChain);

            double analytic = Math.PI * radius * radius;
            Assert.InRange(flat.AreaMm2, analytic * 0.99, analytic * 1.01);
        }

        [Fact]
        public void CircleFlattening_AllPointsNearRadius()
        {
            const double radius = 50;
            var contour = TestGeometry.Circle(1, 0, 0, 0, radius);
            var flat = BezierFlattener.Flatten(contour, 0, new GeometryTolerances());

            foreach (var point in flat.Points)
            {
                double distance = Math.Sqrt(point.X * point.X + point.Y * point.Y);
                Assert.InRange(distance, radius - 0.5, radius + 0.5);
            }
        }

        [Fact]
        public void Rect2_OverlapGapAndUnion()
        {
            var a = new Rect2(0, 0, 10, 10);
            var b = new Rect2(5, 20, 15, 30);

            Assert.True(a.Overlaps(new Rect2(9, 9, 20, 20)));
            Assert.False(a.Overlaps(b));
            Assert.Equal(10, b.VerticalGap(a), 9);
            Assert.Equal(0, a.DistanceTo(new Rect2(10, 0, 20, 10)), 9);

            var union = Rect2.Union(a, b);
            Assert.Equal(0, union.MinX, 9);
            Assert.Equal(30, union.MaxY, 9);
        }

        [Fact]
        public void Tolerances_RejectInvalidValues()
        {
            var tolerances = new GeometryTolerances { ComparisonEpsilonMm = 0 };
            Assert.Throws<ArgumentOutOfRangeException>(() => tolerances.Validate());

            var badDepth = new GeometryTolerances { MaxFlatteningDepth = 0 };
            Assert.Throws<ArgumentOutOfRangeException>(() => badDepth.Validate());
        }

        [Fact]
        public void FlattenedContour_ReportsBrokenChain()
        {
            var first = BezierSegment.Line(new Point2(0, 0), new Point2(10, 0));
            var second = BezierSegment.Line(new Point2(10, 10), new Point2(0, 10));
            var contour = new Contour(new[] { first, second }, true, 1, 0);

            var flat = BezierFlattener.Flatten(contour, 0, new GeometryTolerances());

            Assert.True(flat.HasBrokenChain);
        }

        [Fact]
        public void FromPolygon_ClosesContourImplicitly()
        {
            var contour = TestGeometry.Rect(1, 0, 0, 0, 10, 5);

            Assert.Equal(4, contour.SegmentCount);
            Assert.True(contour.IsClosed);
            Assert.Equal(contour.Start, contour.End);

            var flat = BezierFlattener.Flatten(contour, 0, new GeometryTolerances());
            Assert.Equal(4, flat.Points.Count);
            Assert.Equal(50, flat.AreaMm2, 9);
        }
    }
}
