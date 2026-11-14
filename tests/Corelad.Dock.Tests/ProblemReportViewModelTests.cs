using System;
using System.Linq;
using Corelad.Dock.Diagnostics;
using Corelad.Dock.Tests.Support;
using Corelad.Dock.ViewModels;
using Corelad.Geometry.Diagnostics;
using Xunit;

namespace Corelad.Dock.Tests
{
    /// <summary>
    /// ProblemReportViewModel: boş/hazır durum ayrımı, sayımlar, sıralama, seçim ve
    /// kontur vurgu isteği sözleşmesi. CorelDRAW ve COM kullanılmaz.
    /// </summary>
    public sealed class ProblemReportViewModelTests
    {
        [Fact]
        public void NewViewModel_ShowsNoReportYetState()
        {
            var viewModel = new ProblemReportViewModel();

            Assert.False(viewModel.IsLoaded);
            Assert.False(viewModel.HasProblems);
            Assert.Equal(0, viewModel.TotalCount);
            Assert.Equal("Henüz rapor yok", viewModel.EmptyStateTitle);
            Assert.Equal("Rapor bekleniyor", viewModel.SummaryText);
            Assert.Equal(string.Empty, viewModel.HighlightHint);
        }

        [Fact]
        public void LoadEmpty_ShowsNoProblemsState()
        {
            var viewModel = new ProblemReportViewModel();

            viewModel.Load(Array.Empty<Problem>());

            Assert.True(viewModel.IsLoaded);
            Assert.False(viewModel.HasProblems);
            Assert.Equal("Sorun bulunmadı", viewModel.EmptyStateTitle);
            Assert.Equal("Sorun bulunmadı", viewModel.SummaryText);
            Assert.Contains(
                "hata veya uyarı üretmedi",
                viewModel.EmptyStateMessage,
                StringComparison.Ordinal);
        }

        [Fact]
        public void Clear_ReturnsToNoReportYetState()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(severity: ProblemSeverity.Error, contourIndex: 0),
                ProblemFactory.Create(
                    code: ProblemCode.DetachedComponent,
                    severity: ProblemSeverity.Warning,
                    contourIndex: 1),
            });

            viewModel.Clear();

            Assert.False(viewModel.IsLoaded);
            Assert.False(viewModel.HasProblems);
            Assert.Empty(viewModel.Items);
            Assert.Equal(0, viewModel.ErrorCount);
            Assert.Equal(0, viewModel.WarningCount);
            Assert.Equal(0, viewModel.InfoCount);
            Assert.Equal("Henüz rapor yok", viewModel.EmptyStateTitle);
            Assert.Equal("Rapor bekleniyor", viewModel.SummaryText);
        }

        [Fact]
        public void Load_CountsSeveritiesAndOrdersErrorsFirst()
        {
            var viewModel = new ProblemReportViewModel();

            viewModel.Load(new[]
            {
                ProblemFactory.Create(code: ProblemCode.TooComplex, severity: ProblemSeverity.Info),
                ProblemFactory.Create(code: ProblemCode.OpenContour, severity: ProblemSeverity.Error),
                ProblemFactory.Create(code: ProblemCode.DetachedComponent, severity: ProblemSeverity.Warning),
                ProblemFactory.Create(code: ProblemCode.SelfIntersection, severity: ProblemSeverity.Error),
            });

            Assert.Equal(4, viewModel.TotalCount);
            Assert.Equal(2, viewModel.ErrorCount);
            Assert.Equal(1, viewModel.WarningCount);
            Assert.Equal(1, viewModel.InfoCount);
            Assert.True(viewModel.HasErrors);
            Assert.True(viewModel.HasWarnings);
            Assert.True(viewModel.HasInfo);

            Assert.Equal(
                new[]
                {
                    ProblemSeverity.Error,
                    ProblemSeverity.Error,
                    ProblemSeverity.Warning,
                    ProblemSeverity.Info,
                },
                viewModel.Items.Select(item => item.Severity).ToArray());

            Assert.Equal("2 hata, 1 uyarı, 1 bilgi", viewModel.SummaryText);
            Assert.Equal("2 hata", viewModel.ErrorStatText);
            Assert.Equal("1 uyarı", viewModel.WarningStatText);
            Assert.Equal("1 bilgi", viewModel.InfoStatText);
            Assert.Equal("Sorunlar (4)", viewModel.ListHeading);
        }

        [Fact]
        public void Load_PreservesInputOrderWithinSameSeverity()
        {
            var viewModel = new ProblemReportViewModel();

            viewModel.Load(new[]
            {
                ProblemFactory.Create(code: ProblemCode.OpenContour, severity: ProblemSeverity.Error),
                ProblemFactory.Create(code: ProblemCode.SelfIntersection, severity: ProblemSeverity.Error),
            });

            Assert.Equal(ProblemCode.OpenContour, viewModel.Items[0].Code);
            Assert.Equal(ProblemCode.SelfIntersection, viewModel.Items[1].Code);
        }

        [Fact]
        public void ContourIndexes_AreOneBasedInTextAndZeroBasedInRequest()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(
                    code: ProblemCode.OpenContour,
                    contourIndex: 3,
                    sourceShapeId: 7,
                    sourceSubPathIndex: 2),
            });

            var entry = viewModel.Items[0];
            Assert.Contains("Kontur 4", entry.LocationText, StringComparison.Ordinal);
            Assert.Contains("Alt kontur 3", entry.LocationText, StringComparison.Ordinal);

            ContourHighlightRequest captured = null;
            viewModel.HighlightRequested += (sender, args) => captured = args.Request;

            viewModel.SelectedItem = entry;

            Assert.NotNull(captured);
            Assert.Equal(3, captured.ContourIndex.Value);
            Assert.Equal(7, captured.SourceShapeId.Value);
            Assert.Equal(2, captured.SourceSubPathIndex.Value);
            Assert.Equal(3, captured.PrimaryContourIndex.Value);
        }

        [Fact]
        public void RelatedContour_IsOneBasedInTextAndUsedAsRequestFallback()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(
                    code: ProblemCode.CrossingContours,
                    contourIndex: 3,
                    relatedContourIndex: 5,
                    sourceSubPathIndex: 2,
                    relatedSourceSubPathIndex: 4),
            });

            var entry = viewModel.Items[0];
            Assert.Contains("Kontur 4 ↔ 6", entry.LocationText, StringComparison.Ordinal);

            ContourHighlightRequest captured = null;
            viewModel.HighlightRequested += (sender, args) => captured = args.Request;

            viewModel.SelectedItem = entry;

            Assert.NotNull(captured);
            Assert.Equal(3, captured.ContourIndex.Value);
            Assert.Equal(5, captured.RelatedContourIndex.Value);
            Assert.Equal(3, captured.PrimaryContourIndex.Value);
        }

        [Fact]
        public void PrimaryContourIndex_FallsBackToRelatedContourWhenPrimaryIsMissing()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(contourIndex: null, relatedContourIndex: 5),
            });

            ContourHighlightRequest captured = null;
            viewModel.HighlightRequested += (sender, args) => captured = args.Request;

            viewModel.SelectedItem = viewModel.Items[0];

            Assert.NotNull(captured);
            Assert.Null(captured.ContourIndex);
            Assert.Equal(5, captured.PrimaryContourIndex.Value);
        }

        [Fact]
        public void ContourlessProblem_DoesNotRaiseHighlightRequest()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(
                    code: ProblemCode.UnsupportedObject,
                    message: "Grup desteklenmiyor."),
            });

            var sink = new RecordingHighlightSink();
            var eventCount = 0;
            viewModel.HighlightSink = sink;
            viewModel.HighlightRequested += (sender, args) => eventCount++;

            viewModel.SelectedItem = viewModel.Items[0];

            Assert.False(viewModel.CanHighlightSelected);
            Assert.False(viewModel.RequestHighlight());
            Assert.Equal(0, eventCount);
            Assert.Empty(sink.Requests);
            Assert.Contains("kontura bağlı değil", viewModel.HighlightHint, StringComparison.Ordinal);
        }

        [Fact]
        public void Selection_RaisesEventAndSinkWithSameRequestData()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(
                    code: ProblemCode.CrossingContours,
                    severity: ProblemSeverity.Error,
                    message: "Kontur 2 ile 4 kesişiyor.",
                    contourIndex: 1,
                    relatedContourIndex: 2,
                    sourceShapeId: 4,
                    sourceSubPathIndex: 0,
                    relatedSourceSubPathIndex: 2),
            });

            ContourHighlightRequest fromEvent = null;
            var sink = new RecordingHighlightSink();
            viewModel.HighlightRequested += (sender, args) => fromEvent = args.Request;
            viewModel.HighlightSink = sink;

            viewModel.SelectedItem = viewModel.Items[0];

            Assert.NotNull(fromEvent);
            Assert.Single(sink.Requests);
            Assert.Same(fromEvent, sink.Requests[0]);

            Assert.Equal(ProblemCode.CrossingContours, fromEvent.Code);
            Assert.Equal(ProblemSeverity.Error, fromEvent.Severity);
            Assert.Equal(1, fromEvent.ContourIndex.Value);
            Assert.Equal(2, fromEvent.RelatedContourIndex.Value);
            Assert.Equal(4, fromEvent.SourceShapeId.Value);
            Assert.Equal(0, fromEvent.SourceSubPathIndex.Value);
            Assert.Equal(2, fromEvent.RelatedSourceSubPathIndex.Value);
            Assert.True(fromEvent.HasTarget);
            Assert.Equal(1, fromEvent.PrimaryContourIndex.Value);
        }

        [Fact]
        public void SelectingSameEntryTwice_RaisesOnce_ExplicitRequestRaisesAgain()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(contourIndex: 0),
            });

            var requestCount = 0;
            viewModel.HighlightRequested += (sender, args) => requestCount++;

            var entry = viewModel.Items[0];
            viewModel.SelectedItem = entry;
            viewModel.SelectedItem = entry;

            Assert.Equal(1, requestCount);

            Assert.True(viewModel.RequestHighlight());
            Assert.Equal(2, requestCount);
        }

        [Fact]
        public void SelectFirst_SelectsFirstProblemAndRequestsHighlight()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(code: ProblemCode.DetachedComponent, severity: ProblemSeverity.Warning, contourIndex: 2),
                ProblemFactory.Create(code: ProblemCode.OpenContour, severity: ProblemSeverity.Error, contourIndex: 1),
            });

            var sink = new RecordingHighlightSink();
            viewModel.HighlightSink = sink;

            viewModel.SelectFirst();

            Assert.NotNull(viewModel.SelectedItem);
            Assert.Equal(ProblemCode.OpenContour, viewModel.SelectedItem.Code);
            Assert.Single(sink.Requests);
            Assert.Equal(1, sink.Requests[0].PrimaryContourIndex.Value);
        }

        [Fact]
        public void NoSelection_HintAsksUserToPickAProblem()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(contourIndex: 0),
            });

            Assert.Null(viewModel.SelectedItem);
            Assert.Contains("sorun seçin", viewModel.HighlightHint, StringComparison.Ordinal);
        }

        [Fact]
        public void Selection_HintShowsOneBasedContourNumber()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(contourIndex: 1),
            });

            viewModel.SelectedItem = viewModel.Items[0];

            Assert.Contains("Kontur 2", viewModel.HighlightHint, StringComparison.Ordinal);
            Assert.Contains("Enter", viewModel.HighlightHint, StringComparison.Ordinal);
        }

        [Fact]
        public void Load_ResetsPreviousSelectionWithoutRaisingRequest()
        {
            var viewModel = new ProblemReportViewModel();
            viewModel.Load(new[]
            {
                ProblemFactory.Create(contourIndex: 0),
            });

            var requestCount = 0;
            viewModel.HighlightRequested += (sender, args) => requestCount++;

            viewModel.SelectedItem = viewModel.Items[0];
            Assert.Equal(1, requestCount);

            viewModel.Load(new[]
            {
                ProblemFactory.Create(code: ProblemCode.SelfIntersection, contourIndex: 1),
            });

            Assert.Null(viewModel.SelectedItem);
            Assert.False(viewModel.HasSelection);
            Assert.Equal(1, requestCount);
        }
    }
}
