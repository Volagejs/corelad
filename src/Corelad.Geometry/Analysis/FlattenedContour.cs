using System.Collections.Generic;
using Corelad.Geometry.Model;

namespace Corelad.Geometry.Analysis
{
    /// <summary>
    /// Bir konturun analiz için düzleştirilmiş kopyası. Yalnızca analizde kullanılır;
    /// CorelDRAW'a yazılan geometri orijinal bezier'dir.
    /// </summary>
    public sealed class FlattenedContour
    {
        internal FlattenedContour(
            int index,
            IReadOnlyList<Point2> points,
            bool isClosed,
            double signedAreaMm2,
            double areaMm2,
            Rect2 bounds,
            bool hitFlatteningDepthLimit,
            bool hasBrokenChain)
        {
            Index = index;
            Points = points;
            IsClosed = isClosed;
            SignedAreaMm2 = signedAreaMm2;
            AreaMm2 = areaMm2;
            Bounds = bounds;
            HitFlatteningDepthLimit = hitFlatteningDepthLimit;
            HasBrokenChain = hasBrokenChain;
        }

        public int Index { get; }

        /// <summary>Kapalı konturda son nokta ilk noktayı tekrar etmez; kapanış örtüktür.</summary>
        public IReadOnlyList<Point2> Points { get; }

        public bool IsClosed { get; }

        public double SignedAreaMm2 { get; }

        public double AreaMm2 { get; }

        public Rect2 Bounds { get; }

        public bool HitFlatteningDepthLimit { get; }

        public bool HasBrokenChain { get; }

        /// <summary>Y yukarı pozitif sistemde saat yönü tersi = pozitif alan.</summary>
        public bool IsCounterClockwise => SignedAreaMm2 > 0d;
    }
}
