using System.Linq;
using Corelad.Geometry.Analysis;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;
using Corelad.Geometry.Tests.Support;
using Xunit;

namespace Corelad.Geometry.Tests
{
    public class ProblemTests
    {
        private static SmartDivideAnalysis Analyze(ContourSet set) =>
            new ContainmentAnalyzer().Analyze(set);

        [Fact]
        public void OpenContour_IsReportedAndBlocksApply()
        {
            var openOuter = new Contour(
                new[]
                {
                    BezierSegment.Line(new Point2(0, 0), new Point2(100, 0)),
                    BezierSegment.Line(new Point2(100, 0), new Point2(100, 100)),
                    BezierSegment.Line(new Point2(100, 100), new Point2(0, 100)),
                },
                false,
                1,
                5);

            var analysis = Analyze(TestGeometry.Set(openOuter));

            var problem = Assert.Single(analysis.Problems);
            Assert.Equal(ProblemCode.OpenContour, problem.Code);
            Assert.Equal(ProblemSeverity.Error, problem.Severity);
            Assert.Equal(5, problem.SourceSubPathIndex);
            Assert.False(analysis.CanApply);
        }

        [Fact]
        public void BrokenSegmentChain_IsReportedAsOpenContour()
        {
            var contour = new Contour(
                new[]
                {
                    BezierSegment.Line(new Point2(0, 0), new Point2(100, 0)),
                    BezierSegment.Line(new Point2(100, 100), new Point2(0, 100)),
                    BezierSegment.Line(new Point2(0, 100), new Point2(0, 0)),
                },
                true,
                7,
                3);

            var analysis = Analyze(TestGeometry.Set(contour));

            var problem = Assert.Single(analysis.Problems);
            Assert.Equal(ProblemCode.OpenContour, problem.Code);
            Assert.Equal(7, problem.SourceShapeId);
            Assert.Equal(3, problem.SourceSubPathIndex);
        }

        [Fact]
        public void DegenerateContour_IsReported()
        {
            var degenerate = TestGeometry.Polygon(
                1,
                0,
                new Point2(0, 0), new Point2(10, 0));

            var analysis = Analyze(TestGeometry.Set(degenerate));

            Assert.Contains(analysis.Problems, p => p.Code == ProblemCode.DegenerateContour);
            Assert.False(analysis.CanApply);
        }

        [Fact]
        public void TooThinContour_HasNoReliableInteriorPoint()
        {
            var thin = TestGeometry.Rect(1, 0, 0, 0, 10, 0.001);

            var analysis = Analyze(TestGeometry.Set(thin));

            Assert.Contains(analysis.Problems, p => p.Code == ProblemCode.NoInteriorPoint);
            Assert.False(analysis.CanApply);
        }

        [Fact]
        public void OverlappingBodies_AreReportedAsCrossingContours()
        {
            var set = TestGeometry.Set(
                TestGeometry.Rect(1, 0, 0, 0, 50, 50),
                TestGeometry.Rect(1, 1, 40, 40, 40, 40));

            var analysis = Analyze(set);

            var problem = Assert.Single(analysis.Problems);
            Assert.Equal(ProblemCode.CrossingContours, problem.Code);
            Assert.Equal(ProblemSeverity.Error, problem.Severity);
            Assert.Equal(0, problem.ContourIndex);
            Assert.Equal(1, problem.RelatedContourIndex);
            Assert.False(analysis.CanApply);
        }

        [Fact]
        public void HoleCrossingBody_IsReportedAsCrossingContours()
        {
            var set = TestGeometry.Set(
                TestGeometry.Rect(1, 0, 0, 0, 100, 100),
                TestGeometry.Rect(1, 1, 80, 40, 40, 20));

            var analysis = Analyze(set);

            Assert.Contains(analysis.Problems, p => p.Code == ProblemCode.CrossingContours);
            Assert.False(analysis.CanApply);
        }

        [Fact]
        public void TouchingContours_AreReportedAsTangential()
        {
            var set = TestGeometry.Set(
                TestGeometry.Rect(1, 0, 0, 0, 100, 100),
                TestGeometry.Rect(1, 1, 40, -10, 20, 10));

            var analysis = Analyze(set);

            Assert.Contains(analysis.Problems, p => p.Code == ProblemCode.TangentialContact);
            Assert.Equal(2, analysis.Candidates.Count);
            Assert.All(analysis.Candidates, c => Assert.Empty(c.HoleIndices));
            Assert.True(analysis.HasWarnings);
            Assert.False(analysis.HasErrors);
        }

        [Fact]
        public void ProblemsCarrySourceSubPathForTraceability()
        {
            var set = TestGeometry.Set(
                TestGeometry.Rect(42, 7, 0, 0, 50, 50),
                TestGeometry.Rect(42, 8, 40, 40, 40, 40));

            var analysis = Analyze(set);
            var problem = analysis.Problems.Single();

            Assert.Equal(42, problem.SourceShapeId);
            Assert.Equal(8, problem.RelatedSourceSubPathIndex);
        }
    }
}
