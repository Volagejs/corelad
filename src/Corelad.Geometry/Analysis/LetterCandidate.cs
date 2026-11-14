using System.Collections.Generic;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;

namespace Corelad.Geometry.Analysis
{
    /// <summary>
    /// Mantıksal harf adayı: bir dış gövde + containment ile ona ait delikler.
    /// Ayrık parçalar (İ noktası, j noktası, Ç/Ğ işaretleri) buraya otomatik katılmaz.
    /// </summary>
    public sealed class LetterCandidate
    {
        internal LetterCandidate(
            int id,
            int outerIndex,
            IReadOnlyList<int> holeIndices,
            int depth,
            double outerAreaMm2,
            Rect2 bounds,
            IReadOnlyList<Problem> problems)
        {
            Id = id;
            OuterIndex = outerIndex;
            HoleIndices = holeIndices;
            Depth = depth;
            OuterAreaMm2 = outerAreaMm2;
            Bounds = bounds;
            Problems = problems;
        }

        public int Id { get; }

        public int OuterIndex { get; }

        public IReadOnlyList<int> HoleIndices { get; }

        public int Depth { get; }

        public double OuterAreaMm2 { get; }

        public Rect2 Bounds { get; }

        public IReadOnlyList<Problem> Problems { get; }

        public bool IsProblematic => Problems.Count > 0;

        public int ContourCount => 1 + HoleIndices.Count;

        public override string ToString() =>
            string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "Aday {0}: gövde {1}, delik {2}, derinlik {3}{4}",
                Id,
                OuterIndex,
                HoleIndices.Count,
                Depth,
                IsProblematic ? " (problemli)" : string.Empty);
    }
}
