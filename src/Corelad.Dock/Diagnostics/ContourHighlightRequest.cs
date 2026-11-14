using System;
using Corelad.Geometry.Diagnostics;

namespace Corelad.Dock.Diagnostics
{
    /// <summary>
    /// Bir sorun seçildiğinde CorelDRAW tarafında ilgili konturun vurgulanmasını isteyen
    /// saf veri taşıyıcısı. COM çağrısı, şekil seçimi veya geometri hesabı içermez
    /// (AGENTS.md §2); isteği tüketen entegrasyon katmanıdır.
    ///
    /// İndeksler çekirdekle aynıdır: <see cref="ContourIndex"/> ve
    /// <see cref="SourceSubPathIndex"/> 0 tabanlıdır. Kullanıcıya gösterilen numaralar
    /// <c>ProblemReportEntry</c> içinde 1 tabanlı olarak üretilir.
    /// </summary>
    public sealed class ContourHighlightRequest
    {
        public ContourHighlightRequest(Problem problem)
        {
            if (problem == null) throw new ArgumentNullException(nameof(problem));

            Code = problem.Code;
            Severity = problem.Severity;
            ContourIndex = problem.ContourIndex;
            RelatedContourIndex = problem.RelatedContourIndex;
            SourceShapeId = problem.SourceShapeId;
            SourceSubPathIndex = problem.SourceSubPathIndex;
            RelatedSourceSubPathIndex = problem.RelatedSourceSubPathIndex;
        }

        public ProblemCode Code { get; }

        public ProblemSeverity Severity { get; }

        /// <summary>Çekirdek kontur indeksi (0 tabanlı).</summary>
        public int? ContourIndex { get; }

        /// <summary>İlişkili kontur indeksi (0 tabanlı), varsa.</summary>
        public int? RelatedContourIndex { get; }

        public int? SourceShapeId { get; }

        /// <summary>Kaynak alt kontur indeksi (0 tabanlı).</summary>
        public int? SourceSubPathIndex { get; }

        public int? RelatedSourceSubPathIndex { get; }

        /// <summary>Vurgulanabilecek bir kontur var mı.</summary>
        public bool HasTarget => ContourIndex.HasValue || RelatedContourIndex.HasValue;

        /// <summary>Vurgu için öncelikli kontur indeksi (0 tabanlı); hedef yoksa null.</summary>
        public int? PrimaryContourIndex => ContourIndex ?? RelatedContourIndex;
    }
}
