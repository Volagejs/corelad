using System;
using System.Collections.Generic;
using System.Globalization;
using Corelad.Geometry;
using Corelad.Geometry.Model;
using Corel.Interop.VGCore;

namespace Corelad.Dock.Corel
{
    /// <summary>
    /// POCO konturları → yeni CorelDRAW eğrisi. Geometri orijinal bezier'dir;
    /// düzleştirilmiş analiz kopyası asla yazılmaz (AGENTS.md §1).
    ///
    /// Yazma öncesi detached <see cref="Curve"/> geri okunur ve girişle karşılaştırılır;
    /// eşleşmezse hiçbir şekil belgeye eklenmez (sessiz onarım yok).
    /// </summary>
    public static class CorelCurveWriter
    {
        public static Curve BuildCurve(
            Document document,
            IReadOnlyList<Contour> contours,
            GeometryTolerances tolerances)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (contours == null) throw new ArgumentNullException(nameof(contours));
            if (tolerances == null) throw new ArgumentNullException(nameof(tolerances));
            if (contours.Count == 0) throw new ArgumentException("En az bir kontur gerekir.", nameof(contours));

            // Kapalı konturda kapanış segmenti örtük olabilir (GDI+ / bazı okuyucular).
            // Modele göre kapalılık düz çizgi kapanışı demektir; yazmadan önce açık hale getir.
            var normalized = NormalizeClosedContours(contours, tolerances);
            var elements = new List<CurveElement>();

            for (int i = 0; i < normalized.Count; i++)
            {
                var contour = normalized[i];
                if (contour == null) throw new ArgumentException("Kontur null olamaz.", nameof(contours));
                if (!contour.IsClosed)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture,
                            "Kontur {0} kapalı değil; kapalı olmayan kontur yazılmaz.", i));
                }

                if (contour.SegmentCount == 0)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture, "Kontur {0} segment içermiyor.", i));
                }

                var segments = contour.Segments;
                elements.Add(CreateElement(document, segments[0].P0, cdrCurveElementType.cdrElementStart));

                for (int s = 0; s < segments.Count; s++)
                {
                    var segment = segments[s];
                    if (segment.Kind == SegmentKind.Line)
                    {
                        elements.Add(CreateElement(document, segment.P3, cdrCurveElementType.cdrElementLine));
                    }
                    else
                    {
                        elements.Add(CreateElement(document, segment.P1, cdrCurveElementType.cdrElementControl));
                        elements.Add(CreateElement(document, segment.P2, cdrCurveElementType.cdrElementControl));
                        elements.Add(CreateElement(document, segment.P3, cdrCurveElementType.cdrElementCurve));
                    }
                }
            }

            var array = elements.ToArray();
            var curve = document.CreateCurveFromArray(ref array, array.Length);
            if (curve == null)
            {
                throw new InvalidOperationException("CorelDRAW eğri oluşturamadı (CreateCurveFromArray null döndü).");
            }

            // Ampirik (v27.2): CreateCurveFromArray, başlangıç = bitiş olsa bile subpath'i
            // AÇIK üretir. Kapanışı açıkça işaretle; Corel yinelenen uç düğümünü atar ve
            // kapanış segmentini korur (docs/VERIFICATION.md — V4 probe).
            var subPaths = curve.SubPaths;
            for (int i = 1; i <= subPaths.Count; i++)
            {
                subPaths[i].Closed = true;
            }

            ValidateRoundTrip(document, curve, normalized, tolerances);
            return curve;
        }

        private static List<Contour> NormalizeClosedContours(
            IReadOnlyList<Contour> contours,
            GeometryTolerances tolerances)
        {
            var normalized = new List<Contour>(contours.Count);
            double epsilon = tolerances.ComparisonEpsilonMm;

            for (int i = 0; i < contours.Count; i++)
            {
                var contour = contours[i];
                if (contour == null) throw new ArgumentException("Kontur null olamaz.", nameof(contours));

                if (!contour.IsClosed || contour.SegmentCount == 0)
                {
                    normalized.Add(contour);
                    continue;
                }

                var start = contour.Segments[0].P0;
                var end = contour.Segments[contour.SegmentCount - 1].P3;
                if (end.DistanceTo(start) <= epsilon)
                {
                    normalized.Add(contour);
                    continue;
                }

                var segments = new List<BezierSegment>(contour.SegmentCount + 1);
                for (int s = 0; s < contour.SegmentCount; s++)
                {
                    segments.Add(contour.Segments[s]);
                }

                segments.Add(BezierSegment.Line(end, start));
                normalized.Add(new Contour(
                    segments,
                    true,
                    contour.SourceShapeId,
                    contour.SourceSubPathIndex,
                    contour.Role));
            }

            return normalized;
        }

        /// <summary>
        /// Yazılan eğriyi geri okuyup girişle karşılaştırır. Fark varsa yazma iptal edilir.
        /// </summary>
        public static void ValidateRoundTrip(
            Document document,
            Curve curve,
            IReadOnlyList<Contour> expected,
            GeometryTolerances tolerances)
        {
            var actual = CorelCurveReader.ReadCurve(document, curve, 0, tolerances);
            if (actual.Count != expected.Count)
            {
                throw Mismatch(string.Format(CultureInfo.InvariantCulture,
                    "Subpath sayısı uyuşmuyor: beklenen {0}, yazılan {1}.", expected.Count, actual.Count));
            }

            for (int i = 0; i < expected.Count; i++)
            {
                var expectedContour = expected[i];
                var actualContour = actual.Contours[i];

                if (actualContour.IsClosed != expectedContour.IsClosed)
                {
                    throw Mismatch(string.Format(CultureInfo.InvariantCulture,
                        "Kontur {0}: Closed uyuşmuyor (beklenen {1}, yazılan {2}).",
                        i, expectedContour.IsClosed, actualContour.IsClosed));
                }

                if (actualContour.SegmentCount != expectedContour.SegmentCount)
                {
                    throw Mismatch(string.Format(CultureInfo.InvariantCulture,
                        "Kontur {0}: segment sayısı uyuşmuyor (beklenen {1}, yazılan {2}). Beklenen: {3} Yazılan: {4}",
                        i, expectedContour.SegmentCount, actualContour.SegmentCount,
                        DescribeKinds(expectedContour), DescribeKinds(actualContour)));
                }

                for (int s = 0; s < expectedContour.SegmentCount; s++)
                {
                    var e = expectedContour.Segments[s];
                    var a = actualContour.Segments[s];
                    if (e.Kind != a.Kind)
                    {
                        throw Mismatch(string.Format(CultureInfo.InvariantCulture,
                            "Kontur {0}, segment {1}: tür uyuşmuyor (beklenen {2}, yazılan {3}).",
                            i, s, e.Kind, a.Kind));
                    }

                    RequireClose(i, s, "P0", e.P0, a.P0);
                    RequireClose(i, s, "P1", e.P1, a.P1);
                    RequireClose(i, s, "P2", e.P2, a.P2);
                    RequireClose(i, s, "P3", e.P3, a.P3);
                }
            }
        }

        private static string DescribeKinds(Contour contour)
        {
            var builder = new System.Text.StringBuilder(contour.SegmentCount);
            for (int i = 0; i < contour.SegmentCount; i++)
            {
                builder.Append(contour.Segments[i].Kind == SegmentKind.Line ? 'L' : 'B');
            }

            return builder.ToString();
        }

        private static void RequireClose(int contourIndex, int segmentIndex, string label, Point2 expected, Point2 actual)
        {
            if (expected.DistanceTo(actual) <= CorelTolerances.RoundTripMm) return;

            throw Mismatch(string.Format(CultureInfo.InvariantCulture,
                "Kontur {0}, segment {1}, {2}: nokta uyuşmuyor (beklenen {3}, yazılan {4}, fark {5:0.######} mm).",
                contourIndex, segmentIndex, label, expected, actual, expected.DistanceTo(actual)));
        }

        private static CurveElement CreateElement(
            Document document,
            Point2 point,
            cdrCurveElementType elementType)
        {
            return new CurveElement
            {
                PositionX = CorelUnits.FromMillimeters(document, point.X),
                PositionY = CorelUnits.FromMillimeters(document, point.Y),
                ElementType = elementType,
                NodeType = cdrNodeType.cdrCuspNode,
                Flags = 0,
            };
        }

        private static InvalidOperationException Mismatch(string message)
        {
            return new InvalidOperationException("CorelDRAW geometri round-trip doğrulaması başarısız: " + message);
        }
    }
}
