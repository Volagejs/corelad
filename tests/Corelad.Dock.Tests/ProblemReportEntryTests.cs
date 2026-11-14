using Corelad.Dock.Diagnostics;
using Corelad.Dock.Tests.Support;
using Corelad.Geometry.Diagnostics;
using Xunit;

namespace Corelad.Dock.Tests
{
    /// <summary>
    /// ProblemReportEntry: kullanıcıya gösterilen 1 tabanlı numaralar, erişilebilirlik
    /// metinleri ve çekirdek indekslerin korunması.
    /// </summary>
    public sealed class ProblemReportEntryTests
    {
        [Fact]
        public void LocationText_ShowsOneBasedContourAndSubPathNumbers()
        {
            var entry = new ProblemReportEntry(ProblemFactory.Create(
                code: ProblemCode.OpenContour,
                contourIndex: 3,
                sourceSubPathIndex: 2));

            Assert.Contains("Kontur 4", entry.LocationText, System.StringComparison.Ordinal);
            Assert.Contains("Alt kontur 3", entry.LocationText, System.StringComparison.Ordinal);
        }

        [Fact]
        public void LocationText_ShowsRelatedContoursBothOneBased()
        {
            var entry = new ProblemReportEntry(ProblemFactory.Create(
                code: ProblemCode.CrossingContours,
                contourIndex: 3,
                relatedContourIndex: 5,
                sourceSubPathIndex: 2,
                relatedSourceSubPathIndex: 4));

            Assert.Contains("Kontur 4 ↔ 6", entry.LocationText, System.StringComparison.Ordinal);
            Assert.Contains("Alt kontur 3 ↔ 5", entry.LocationText, System.StringComparison.Ordinal);
        }

        [Fact]
        public void LocationText_IsEmptyWithoutContourInformation()
        {
            var entry = new ProblemReportEntry(ProblemFactory.Create(
                code: ProblemCode.UnsupportedObject,
                message: "grup"));

            Assert.Equal(string.Empty, entry.LocationText);
            Assert.False(entry.HasContourTarget);
            Assert.Null(entry.HighlightContourIndex);
        }

        [Fact]
        public void CoreIndexes_StayZeroBasedForHighlightRequests()
        {
            var entry = new ProblemReportEntry(ProblemFactory.Create(
                contourIndex: 3,
                sourceSubPathIndex: 2));

            Assert.Equal(3, entry.HighlightContourIndex.Value);
            Assert.Equal(2, entry.SourceSubPathIndex.Value);
            Assert.True(entry.HasContourTarget);
        }

        [Fact]
        public void HighlightContourIndex_FallsBackToRelatedContour()
        {
            var entry = new ProblemReportEntry(ProblemFactory.Create(
                contourIndex: null,
                relatedContourIndex: 5));

            Assert.Null(entry.Source.ContourIndex);
            Assert.Equal(5, entry.HighlightContourIndex.Value);
            Assert.True(entry.HasContourTarget);
        }

        [Fact]
        public void AccessibleName_ContainsSeverityTitleLocationAndCode()
        {
            var entry = new ProblemReportEntry(ProblemFactory.Create(
                code: ProblemCode.CrossingContours,
                severity: ProblemSeverity.Error,
                contourIndex: 0,
                sourceShapeId: 1));

            Assert.Contains("Hata", entry.AccessibleName, System.StringComparison.Ordinal);
            Assert.Contains("Konturlar kesişiyor", entry.AccessibleName, System.StringComparison.Ordinal);
            Assert.Contains("Kontur 1", entry.AccessibleName, System.StringComparison.Ordinal);
            Assert.Contains("CrossingContours", entry.AccessibleName, System.StringComparison.Ordinal);
            Assert.Contains("şekil", entry.AccessibleName, System.StringComparison.Ordinal);
        }

        [Fact]
        public void AccessibleHelpText_ContainsDescriptionAndAnalyzerMessage()
        {
            const string message = "Kontur 2 ile 4 kesişiyor.";
            var entry = new ProblemReportEntry(ProblemFactory.Create(
                code: ProblemCode.CrossingContours,
                message: message));

            Assert.Contains(
                ProblemText.Description(ProblemCode.CrossingContours),
                entry.AccessibleHelpText,
                System.StringComparison.Ordinal);
            Assert.Contains(message, entry.AccessibleHelpText, System.StringComparison.Ordinal);
        }

        [Fact]
        public void DetailText_KeepsAnalyzerMessage()
        {
            const string message = "Analizden gelen özgün ayrıntı.";
            var entry = new ProblemReportEntry(ProblemFactory.Create(message: message));

            Assert.Equal(message, entry.DetailText);
        }

        [Fact]
        public void CodeText_UsesProblemCodeName()
        {
            var entry = new ProblemReportEntry(ProblemFactory.Create(code: ProblemCode.DetachedComponent));

            Assert.Equal("Kod: DetachedComponent", entry.CodeText);
        }

        [Fact]
        public void ToString_ContainsSeverityAndTitle()
        {
            var entry = new ProblemReportEntry(ProblemFactory.Create(
                code: ProblemCode.OpenContour,
                severity: ProblemSeverity.Warning));

            Assert.Contains("Uyarı", entry.ToString(), System.StringComparison.Ordinal);
            Assert.Contains("Kontur kapalı değil", entry.ToString(), System.StringComparison.Ordinal);
        }
    }
}
