using System.Collections.Generic;
using System.Linq;
using Corelad.Geometry.Analysis;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;
using Corelad.Geometry.Tests.Support;
using Xunit;
using Xunit.Abstractions;

namespace Corelad.Geometry.Tests
{
    /// <summary>
    /// Gerçek font dış konturları (GDI+). CorelDRAW açılmaz; yalnızca test bağımlılığı.
    /// Bu testler font sürümüne duyarlıdır; beklenen değerler Arial (Windows) içindir.
    /// </summary>
    public class FontOutlineTests
    {
        private readonly ITestOutputHelper _output;

        public FontOutlineTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private static SmartDivideAnalysis Analyze(string text)
        {
            var set = TestGeometry.FromText(text);
            return new ContainmentAnalyzer().Analyze(set);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("B")]
        [InlineData("O")]
        [InlineData("P")]
        [InlineData("R")]
        [InlineData("İ")]
        [InlineData("j")]
        [InlineData("Ç")]
        [InlineData("Ğ")]
        [InlineData("MUSTAFA")]
        public void DiagnoseRealFontOutlines(string text)
        {
            var analysis = Analyze(text);
            _output.WriteLine("=== " + text + " ===");
            _output.WriteLine(analysis.Describe());
        }

        [Theory]
        [InlineData("A", 1)]
        [InlineData("B", 2)]
        [InlineData("O", 1)]
        [InlineData("P", 1)]
        [InlineData("R", 1)]
        public void HollowLetters_KeepHolesInSingleCandidate(string letter, int expectedHoles)
        {
            var analysis = Analyze(letter);
            _output.WriteLine(analysis.Describe());

            var candidate = Assert.Single(analysis.Candidates);
            Assert.Equal(expectedHoles, candidate.HoleIndices.Count);
            Assert.False(analysis.HasErrors, analysis.Describe());
        }

        [Fact]
        public void TurkishI_DoesNotMergeDotIntoBody()
        {
            var analysis = Analyze("İ");
            _output.WriteLine(analysis.Describe());

            Assert.True(analysis.Candidates.Count >= 2);
            Assert.Contains(analysis.Problems, p => p.Code == ProblemCode.DetachedComponent);
            Assert.All(analysis.Candidates, c => Assert.Equal(0, c.Depth));
        }

        [Fact]
        public void WordMustafa_HasSevenLogicalLettersAndTwoHoles()
        {
            var analysis = Analyze("MUSTAFA");
            _output.WriteLine(analysis.Describe());

            Assert.Equal(7, analysis.Candidates.Count);
            Assert.Equal(2, analysis.Candidates.Sum(c => c.HoleIndices.Count));
            Assert.False(analysis.HasErrors, analysis.Describe());
        }

        [Fact]
        public void TurkishCharacterSet_IsAnalyzedDeterministicallyWithoutLosingContours()
        {
            const string characters = "ÇĞİıÖŞÜ";

            foreach (char character in characters)
            {
                string glyph = character.ToString();
                var first = Analyze(glyph);
                var second = Analyze(glyph);

                _output.WriteLine("=== " + glyph + " ===");
                _output.WriteLine(first.Describe());

                Assert.NotEmpty(first.Source.Contours);
                Assert.Equal(TestGeometry.Signature(first), TestGeometry.Signature(second));
                Assert.All(first.Candidates, candidate =>
                    Assert.InRange(candidate.OuterIndex, 0, first.Source.Count - 1));
                Assert.All(first.Problems, problem =>
                {
                    Assert.True(problem.ContourIndex.HasValue);
                    Assert.InRange(problem.ContourIndex.Value, 0, first.Source.Count - 1);
                });

                var accountedFor = new HashSet<int>();
                foreach (var candidate in first.Candidates)
                {
                    accountedFor.Add(candidate.OuterIndex);
                    foreach (int holeIndex in candidate.HoleIndices) accountedFor.Add(holeIndex);
                }

                foreach (var problem in first.Problems)
                {
                    if (problem.ContourIndex.HasValue) accountedFor.Add(problem.ContourIndex.Value);
                    if (problem.RelatedContourIndex.HasValue) accountedFor.Add(problem.RelatedContourIndex.Value);
                }

                Assert.Equal(Enumerable.Range(0, first.Source.Count).OrderBy(i => i),
                    accountedFor.OrderBy(i => i));
            }
        }
    }
}
