using System.Globalization;

namespace Corelad.Geometry.Diagnostics
{
    /// <summary>
    /// Raporlanan geometri problemi. Kaynak shape/subpath indeksleri kullanıcıya kadar
    /// izlenebilirliği sağlar; vurgulama için kontur indeksleri taşınır.
    /// </summary>
    public sealed class Problem
    {
        public Problem(
            ProblemCode code,
            ProblemSeverity severity,
            string message,
            int? contourIndex = null,
            int? relatedContourIndex = null,
            int? sourceShapeId = null,
            int? sourceSubPathIndex = null,
            int? relatedSourceSubPathIndex = null)
        {
            Code = code;
            Severity = severity;
            Message = message ?? string.Empty;
            ContourIndex = contourIndex;
            RelatedContourIndex = relatedContourIndex;
            SourceShapeId = sourceShapeId;
            SourceSubPathIndex = sourceSubPathIndex;
            RelatedSourceSubPathIndex = relatedSourceSubPathIndex;
        }

        public ProblemCode Code { get; }

        public ProblemSeverity Severity { get; }

        public string Message { get; }

        public int? ContourIndex { get; }

        public int? RelatedContourIndex { get; }

        public int? SourceShapeId { get; }

        public int? SourceSubPathIndex { get; }

        public int? RelatedSourceSubPathIndex { get; }

        public override string ToString()
        {
            string location = ContourIndex.HasValue
                ? string.Format(CultureInfo.InvariantCulture, " [kontur {0}{1}]", ContourIndex.Value,
                    RelatedContourIndex.HasValue ? " <-> " + RelatedContourIndex.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)
                : string.Empty;
            return string.Format(CultureInfo.InvariantCulture, "{0}: {1}{2}", Severity, Code, location) + " — " + Message;
        }
    }
}
