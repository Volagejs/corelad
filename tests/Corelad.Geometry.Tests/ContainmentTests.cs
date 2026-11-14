using System.Linq;
using Corelad.Geometry.Analysis;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;
using Corelad.Geometry.Tests.Support;
using Xunit;

namespace Corelad.Geometry.Tests
{
    public class ContainmentTests
    {
        private static SmartDivideAnalysis Analyze(ContourSet set) =>
            new ContainmentAnalyzer().Analyze(set);

        [Theory]
        [InlineData("A", 1)]
        [InlineData("B", 2)]
        [InlineData("O", 1)]
        [InlineData("P", 1)]
        [InlineData("R", 1)]
        public void LetterBodies_KeepTheirHolesInOneCandidate(string letter, int expectedHoles)
        {
            ContourSet set = letter switch
            {
                "A" => TestGeometry.LetterA(),
                "B" => TestGeometry.LetterB(),
                "O" => TestGeometry.LetterO(),
                "P" => TestGeometry.LetterP(),
                "R" => TestGeometry.LetterR(),
                _ => throw new System.ArgumentOutOfRangeException(nameof(letter)),
            };

            var analysis = Analyze(set);
            Assert.False(analysis.HasErrors, analysis.Describe());

            var candidate = Assert.Single(analysis.Candidates);
            Assert.Equal(0, candidate.Depth);
            Assert.Equal(expectedHoles, candidate.HoleIndices.Count);
            Assert.Equal(1 + expectedHoles, candidate.ContourCount);
            Assert.False(candidate.IsProblematic);

            foreach (int hole in candidate.HoleIndices)
            {
                Assert.Equal(1, analysis.Contours[hole].Depth);
                Assert.Equal(ContourRole.Hole, analysis.Contours[hole].Role);
                Assert.Equal(candidate.OuterIndex, analysis.Contours[hole].ParentIndex);
            }

            Assert.Equal(ContourRole.Outer, analysis.Contours[candidate.OuterIndex].Role);
        }

        [Fact]
        public void BezierRing_IsDetectedAsBodyWithHole()
        {
            var analysis = Analyze(TestGeometry.Ring(1, 50, 50, 50, 30));

            Assert.False(analysis.HasErrors, analysis.Describe());
            var candidate = Assert.Single(analysis.Candidates);
            Assert.Single(candidate.HoleIndices);

            double outerArea = System.Math.PI * 50 * 50;
            double holeArea = System.Math.PI * 30 * 30;
            Assert.InRange(analysis.Contours[candidate.OuterIndex].AreaMm2, outerArea * 0.99, outerArea * 1.01);
            Assert.InRange(analysis.Contours[candidate.HoleIndices[0]].AreaMm2, holeArea * 0.99, holeArea * 1.01);
        }

        [Fact]
        public void ThreeLevelNesting_ProducesIslandCandidateAndWarning()
        {
            var set = TestGeometry.Set(
                TestGeometry.Rect(1, 0, 0, 0, 100, 100),
                TestGeometry.Rect(1, 1, 10, 10, 80, 80),
                TestGeometry.Rect(1, 2, 30, 30, 40, 40));

            var analysis = Analyze(set);

            Assert.Equal(new[] { 0, 1, 2 }, analysis.Contours.Select(c => c.Depth));
            Assert.Equal(new[] { ContourRole.Outer, ContourRole.Hole, ContourRole.Outer },
                analysis.Contours.Select(c => c.Role));

            Assert.Equal(2, analysis.Candidates.Count);
            Assert.Single(analysis.Candidates[0].HoleIndices);
            Assert.Equal(1, analysis.Candidates[0].HoleIndices[0]);
            Assert.Equal(2, analysis.Candidates[1].OuterIndex);
            Assert.Empty(analysis.Candidates[1].HoleIndices);

            var islandProblem = Assert.Single(analysis.Problems);
            Assert.Equal(ProblemCode.NestedIsland, islandProblem.Code);
            Assert.Equal(2, islandProblem.ContourIndex);
            Assert.False(analysis.HasErrors);
        }

        [Fact]
        public void CombinedWord_ProducesOneCandidatePerLetter()
        {
            var analysis = Analyze(TestGeometry.SyntheticWordMustafa());

            Assert.False(analysis.HasErrors, analysis.Describe());
            Assert.Equal(7, analysis.Candidates.Count);
            Assert.All(analysis.Candidates, c => Assert.Equal(0, c.Depth));
            Assert.Equal(2, analysis.Candidates.Sum(c => c.HoleIndices.Count));
            Assert.DoesNotContain(analysis.Problems, p => p.Code == ProblemCode.DetachedComponent);
            Assert.True(analysis.CanApply);
        }

        [Fact]
        public void ContainmentIsDeterministic()
        {
            var set = TestGeometry.SyntheticWordMustafa();

            string first = TestGeometry.Signature(Analyze(set));
            string second = TestGeometry.Signature(Analyze(set));

            Assert.Equal(first, second);
        }

        [Fact]
        public void CompoundContourWithoutIntersections_IsRejectedNotRepaired()
        {
            // Kendini kesen "diken" kontur: onarım yok, Problem var.
            var bowtie = TestGeometry.Polygon(
                1,
                0,
                new Point2(0, 0), new Point2(40, 0), new Point2(40, 40),
                new Point2(20, -20), new Point2(0, 40));

            var analysis = Analyze(TestGeometry.Set(bowtie));

            Assert.True(analysis.HasErrors);
            Assert.Contains(analysis.Problems, p => p.Code == ProblemCode.SelfIntersection);
            Assert.Empty(analysis.Candidates);
            Assert.False(analysis.CanApply);
        }
    }
}
