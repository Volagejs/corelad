using System;
using System.Collections.Generic;
using System.Globalization;
using Corelad.Geometry.Diagnostics;

namespace Corelad.Dock.Diagnostics
{
    /// <summary>
    /// Tek bir <see cref="Problem"/> için salt okunur sunum modeli. Türkçe başlık/açıklama,
    /// önem işareti ve kullanıcıya gösterilen (1 tabanlı) kontur/alt kontur numaralarını üretir.
    ///
    /// Çekirdek indeksler değiştirilmez: <see cref="HighlightContourIndex"/> ve
    /// <see cref="SourceSubPathIndex"/> 0 tabanlıdır; yalnızca metin gösterimi 1 tabanlıdır.
    /// </summary>
    public sealed class ProblemReportEntry
    {
        public ProblemReportEntry(Problem problem)
        {
            if (problem == null) throw new ArgumentNullException(nameof(problem));

            Source = problem;
            LocationText = BuildLocationText(problem);
            AccessibleName = BuildAccessibleName(problem, LocationText);
            AccessibleHelpText = ProblemText.Description(problem.Code) +
                                 (string.IsNullOrWhiteSpace(problem.Message)
                                     ? string.Empty
                                     : " Ayrıntı: " + problem.Message);
        }

        public Problem Source { get; }

        public ProblemSeverity Severity => Source.Severity;

        public ProblemCode Code => Source.Code;

        public bool IsError => Source.Severity == ProblemSeverity.Error;

        public bool IsWarning => Source.Severity == ProblemSeverity.Warning;

        public bool IsInfo => Source.Severity == ProblemSeverity.Info;

        public string SeverityLabel => ProblemText.SeverityLabel(Source.Severity);

        /// <summary>Renk körlüğüne dayanıklı görsel işaret; ekran okuyucu tarafından okunmaz.</summary>
        public string SeverityGlyph => ProblemText.SeverityGlyph(Source.Severity);

        /// <summary>Teknik sorun kodu (ör. "Kod: UnsupportedObject").</summary>
        public string CodeText => "Kod: " + Source.Code.ToString();

        public string Title => ProblemText.Title(Source.Code);

        public string Description => ProblemText.Description(Source.Code);

        /// <summary>Analizin ürettiği örneğe özgü Türkçe ayrıntı.</summary>
        public string DetailText => Source.Message ?? string.Empty;

        public string LocationText { get; }

        /// <summary>Vurgulanacak kontur indeksi (0 tabanlı); hedef yoksa null.</summary>
        public int? HighlightContourIndex => Source.ContourIndex ?? Source.RelatedContourIndex;

        /// <summary>Kaynak alt kontur indeksi (0 tabanlı).</summary>
        public int? SourceSubPathIndex => Source.SourceSubPathIndex;

        public bool HasContourTarget => Source.ContourIndex.HasValue || Source.RelatedContourIndex.HasValue;

        public string AccessibleName { get; }

        public string AccessibleHelpText { get; }

        public override string ToString()
        {
            return Details();
        }

        private string Details()
        {
            return SeverityLabel + ": " + Title + (LocationText.Length == 0 ? string.Empty : ". " + LocationText);
        }

        private static string BuildLocationText(Problem problem)
        {
            var parts = new List<string>(3);

            if (problem.ContourIndex.HasValue)
            {
                var text = "Kontur " + Format(problem.ContourIndex.Value);
                if (problem.RelatedContourIndex.HasValue)
                {
                    text += " ↔ " + Format(problem.RelatedContourIndex.Value);
                }

                parts.Add(text);
            }

            if (problem.SourceSubPathIndex.HasValue)
            {
                var text = "Alt kontur " + Format(problem.SourceSubPathIndex.Value);
                if (problem.RelatedSourceSubPathIndex.HasValue)
                {
                    text += " ↔ " + Format(problem.RelatedSourceSubPathIndex.Value);
                }

                parts.Add(text);
            }
            else if (problem.RelatedSourceSubPathIndex.HasValue)
            {
                parts.Add("Alt kontur " + Format(problem.RelatedSourceSubPathIndex.Value));
            }

            return string.Join(" · ", parts);
        }

        private static string BuildAccessibleName(Problem problem, string locationText)
        {
            var prefix = ProblemText.SeverityLabel(problem.Severity) + ": " + ProblemText.Title(problem.Code);
            var location = locationText.Length == 0 ? "Kontur bilgisi yok" : locationText;
            var shape = problem.SourceShapeId.HasValue
                ? ", şekil " + Format(problem.SourceShapeId.Value)
                : string.Empty;

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}. {1}{2}. Sorun kodu {3}.",
                prefix,
                location,
                shape,
                problem.Code);
        }

        /// <summary>Çekirdek indeksini kullanıcıya gösterilen 1 tabanlı numaraya çevirir.</summary>
        private static string Format(int zeroBasedIndex)
        {
            return (zeroBasedIndex + 1).ToString(CultureInfo.CurrentCulture);
        }
    }
}
