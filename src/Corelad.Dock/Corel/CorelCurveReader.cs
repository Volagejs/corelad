using System;
using System.Collections.Generic;
using Corelad.Geometry;
using Corelad.Geometry.Model;
using Corel.Interop.VGCore;

namespace Corelad.Dock.Corel
{
    /// <summary>
    /// CorelDRAW eğrisi → POCO. Yalnızca okuma yapar; hiçbir kaynak nesne değiştirilmez
    /// (AGENTS.md §1, docs/P1-integration-plan.md §2).
    /// </summary>
    public static class CorelCurveReader
    {
        /// <summary>Seçili şeklin tüm subpath'lerini kontur kümesine çevirir.</summary>
        public static ContourSet Read(Document document, Shape shape, int shapeId, GeometryTolerances tolerances)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            if (tolerances == null) throw new ArgumentNullException(nameof(tolerances));

            if (shape.Type != cdrShapeType.cdrCurveShape)
            {
                throw new InvalidOperationException(
                    "Şekil eğri değil (Type = " + shape.Type + "); yalnızca cdrCurveShape okunur.");
            }

            var curve = shape.Curve;
            if (curve == null)
            {
                throw new InvalidOperationException("Şeklin Curve nesnesi okunamadı.");
            }

            return ReadCurve(document, curve, shapeId, tolerances);
        }

        /// <summary>Belgeye eklenmemiş (detached) bir Curve nesnesini de okuyabilir.</summary>
        public static ContourSet ReadCurve(Document document, Curve curve, int shapeId, GeometryTolerances tolerances)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (curve == null) throw new ArgumentNullException(nameof(curve));
            if (tolerances == null) throw new ArgumentNullException(nameof(tolerances));

            double epsilon = tolerances.ComparisonEpsilonMm;
            var contours = new List<Contour>();
            var subPaths = curve.SubPaths;

            for (int sp = 1; sp <= subPaths.Count; sp++)
            {
                var subPath = subPaths[sp];
                var segments = new List<BezierSegment>();
                var segmentCollection = subPath.Segments;

                for (int s = 1; s <= segmentCollection.Count; s++)
                {
                    var segment = segmentCollection[s];
                    var startPoint = ReadNode(document, segment.StartNode);
                    var endPoint = ReadNode(document, segment.EndNode);

                    if (segment.Type == cdrSegmentType.cdrCurveSegment)
                    {
                        var control1 = new Point2(
                            CorelUnits.ToMillimeters(document, segment.StartingControlPointX),
                            CorelUnits.ToMillimeters(document, segment.StartingControlPointY));
                        var control2 = new Point2(
                            CorelUnits.ToMillimeters(document, segment.EndingControlPointX),
                            CorelUnits.ToMillimeters(document, segment.EndingControlPointY));

                        segments.Add(BezierSegment.Cubic(startPoint, control1, control2, endPoint));
                    }
                    else if (segment.Type == cdrSegmentType.cdrLineSegment)
                    {
                        segments.Add(BezierSegment.Line(startPoint, endPoint));
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "Desteklenmeyen segment türü: " + segment.Type +
                            ". Geometri sessizce çizgiye çevrilmez.");
                    }
                }

                bool closed = subPath.Closed;

                // CorelDRAW kapalı subpath'te kapanış segmentini açıkça listelemeyebilir.
                // Bu durumda kapanış düz çizgidir; geometri onarımı değil, modelin doğal karşılığıdır.
                if (closed && segments.Count > 0)
                {
                    var first = segments[0].P0;
                    var last = segments[segments.Count - 1].P3;
                    if (last.DistanceTo(first) > epsilon)
                    {
                        segments.Add(BezierSegment.Line(last, first));
                    }
                }

                contours.Add(new Contour(segments, closed, shapeId, sp - 1));
            }

            return new ContourSet(contours);
        }

        private static Point2 ReadNode(Document document, Node node)
        {
            return new Point2(
                CorelUnits.ToMillimeters(document, node.PositionX),
                CorelUnits.ToMillimeters(document, node.PositionY));
        }
    }
}
