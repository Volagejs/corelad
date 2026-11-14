using System;
using System.Collections.Generic;

namespace Corelad.Geometry.Model
{
    /// <summary>
    /// Bağlı segment zinciri. Geometri orijinal bezier olarak taşınır; poligonlaştırma
    /// yalnızca analiz kopyasında yapılır. Kaynak izlenebilirliği için shape/subpath indeksi tutulur.
    /// </summary>
    public sealed class Contour
    {
        private readonly BezierSegment[] _segments;

        public Contour(
            IEnumerable<BezierSegment> segments,
            bool isClosed,
            int sourceShapeId = 0,
            int sourceSubPathIndex = 0,
            ContourRole role = ContourRole.Unknown)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            var buffer = new List<BezierSegment>();
            foreach (var segment in segments)
            {
                if (segment == null) throw new ArgumentException("Segment null olamaz.", nameof(segments));
                buffer.Add(segment);
            }

            _segments = buffer.ToArray();
            IsClosed = isClosed;
            SourceShapeId = sourceShapeId;
            SourceSubPathIndex = sourceSubPathIndex;
            Role = role;
        }

        public IReadOnlyList<BezierSegment> Segments => _segments;

        public bool IsClosed { get; }

        public int SourceShapeId { get; }

        public int SourceSubPathIndex { get; }

        /// <summary>Çağıranın ipucu. Otoriter rol analiz sonucundadır.</summary>
        public ContourRole Role { get; }

        public bool IsEmpty => _segments.Length == 0;

        public Point2 Start => _segments.Length == 0 ? Point2.Origin : _segments[0].P0;

        public Point2 End => _segments.Length == 0 ? Point2.Origin : _segments[_segments.Length - 1].P3;

        public int SegmentCount => _segments.Length;

        public static Contour FromPolygon(
            IReadOnlyList<Point2> points,
            bool isClosed = true,
            int sourceShapeId = 0,
            int sourceSubPathIndex = 0)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count < 2) throw new ArgumentException("En az 2 nokta gerekir.", nameof(points));

            var segments = new List<BezierSegment>(points.Count);
            for (int i = 0; i + 1 < points.Count; i++)
            {
                segments.Add(BezierSegment.Line(points[i], points[i + 1]));
            }

            if (isClosed)
            {
                var last = points[points.Count - 1];
                if (last.DistanceTo(points[0]) > 0d)
                {
                    segments.Add(BezierSegment.Line(last, points[0]));
                }
            }

            return new Contour(segments, isClosed, sourceShapeId, sourceSubPathIndex);
        }
    }
}
