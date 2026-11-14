using System.Linq;
using Corelad.Geometry.Analysis;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;
using Corelad.Geometry.Tests.Support;
using Xunit;

namespace Corelad.Geometry.Tests
{
    public class DetachedComponentTests
    {
        private static SmartDivideAnalysis Analyze(ContourSet set) =>
            new ContainmentAnalyzer().Analyze(set);

        [Fact]
        public void TurkishI_ReportsDetachedDotWithoutMerging()
        {
            var analysis = Analyze(TestGeometry.LetterIWithDot());

            Assert.False(analysis.HasErrors, analysis.Describe());
            Assert.Equal(2, analysis.Candidates.Count);
            Assert.All(analysis.Candidates, c => Assert.Empty(c.HoleIndices));

            var problem = Assert.Single(analysis.Problems);
            Assert.Equal(ProblemCode.DetachedComponent, problem.Code);
            Assert.Equal(ProblemSeverity.Warning, problem.Severity);
            Assert.Equal(1, problem.ContourIndex);
            Assert.Equal(0, problem.RelatedContourIndex);

            // Yanlış eşleştirme yok: nokta gövdeyle birleştirilmedi.
            Assert.DoesNotContain(analysis.Candidates, c => c.ContourCount > 1);
            Assert.All(analysis.Candidates, c => Assert.Equal(0, c.Depth));
        }

        [Fact]
        public void JWithDot_ReportsDetachedDot()
        {
            var analysis = Analyze(TestGeometry.LetterJWithDot());

            Assert.False(analysis.HasErrors, analysis.Describe());
            Assert.Equal(2, analysis.Candidates.Count);
            Assert.Contains(analysis.Problems, p => p.Code == ProblemCode.DetachedComponent);
            Assert.DoesNotContain(analysis.Candidates, c => c.ContourCount > 1);
        }

        [Fact]
        public void Cedilla_DetachedBelowBody_IsReportedNotMerged()
        {
            var analysis = Analyze(TestGeometry.LetterCWithCedilla());

            Assert.False(analysis.HasErrors, analysis.Describe());
            Assert.Equal(2, analysis.Candidates.Count);
            Assert.Contains(analysis.Problems,
                p => p.Code == ProblemCode.DetachedComponent && p.ContourIndex == 1 && p.RelatedContourIndex == 0);
            Assert.DoesNotContain(analysis.Candidates, c => c.ContourCount > 1);
        }

        [Fact]
        public void Breve_DetachedAboveBody_IsReportedNotMerged()
        {
            var analysis = Analyze(TestGeometry.LetterGWithBreve());

            Assert.False(analysis.HasErrors, analysis.Describe());
            Assert.Equal(2, analysis.Candidates.Count);
            Assert.Contains(analysis.Problems, p => p.Code == ProblemCode.DetachedComponent);
            Assert.DoesNotContain(analysis.Candidates, c => c.ContourCount > 1);
        }

        [Fact]
        public void TouchingCedilla_IsReportedAsTangentialNotDetached()
        {
            var set = TestGeometry.Set(
                TestGeometry.Rect(1, 0, 0, 0, 100, 100),
                TestGeometry.Rect(1, 1, 40, -10, 20, 10));

            var analysis = Analyze(set);

            Assert.Contains(analysis.Problems, p => p.Code == ProblemCode.TangentialContact);
            Assert.DoesNotContain(analysis.Problems, p => p.Code == ProblemCode.DetachedComponent);
            Assert.Equal(2, analysis.Candidates.Count);
        }

        [Fact]
        public void SideBySideLetters_AreNotReportedAsDetached()
        {
            var set = TestGeometry.Set(
                TestGeometry.Rect(1, 0, 0, 0, 40, 80),
                TestGeometry.Rect(1, 1, 60, 0, 40, 80));

            var analysis = Analyze(set);

            Assert.False(analysis.HasErrors, analysis.Describe());
            Assert.DoesNotContain(analysis.Problems, p => p.Code == ProblemCode.DetachedComponent);
            Assert.Equal(2, analysis.Candidates.Count);
        }

        [Fact]
        public void DetachedDetectionIsDeterministic()
        {
            var set = TestGeometry.LetterIWithDot();

            string first = TestGeometry.Signature(Analyze(set));
            string second = TestGeometry.Signature(Analyze(set));

            Assert.Equal(first, second);
        }
    }
}
