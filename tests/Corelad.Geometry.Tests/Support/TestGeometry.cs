using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using Corelad.Geometry.Analysis;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;

namespace Corelad.Geometry.Tests.Support
{
    /// <summary>
    /// Test girdileri. Sentetik şekiller elle tanımlanır; gerçek harf dış konturları
    /// GDI+ ile üretilir (yalnızca test; CorelDRAW açılmaz).
    /// </summary>
    internal static class TestGeometry
    {
        public static ContourSet Set(params Contour[] contours) => new ContourSet(contours);

        public static Contour Polygon(int shapeId, int subPath, params Point2[] points) =>
            Contour.FromPolygon(points, true, shapeId, subPath);

        public static Contour Rect(int shapeId, int subPath, double x, double y, double width, double height) =>
            Polygon(
                shapeId,
                subPath,
                new Point2(x, y),
                new Point2(x + width, y),
                new Point2(x + width, y + height),
                new Point2(x, y + height));

        /// <summary>Dört kübik bezier ile daire (yaklaşım hatası ~%0.03 alan).</summary>
        public static Contour Circle(int shapeId, int subPath, double cx, double cy, double radius)
        {
            const double k = 0.5522847498307936;
            double r = radius;
            double kr = k * r;
            var segments = new List<BezierSegment>
            {
                BezierSegment.Cubic(
                    new Point2(cx + r, cy),
                    new Point2(cx + r, cy + kr),
                    new Point2(cx + kr, cy + r),
                    new Point2(cx, cy + r)),
                BezierSegment.Cubic(
                    new Point2(cx, cy + r),
                    new Point2(cx - kr, cy + r),
                    new Point2(cx - r, cy + kr),
                    new Point2(cx - r, cy)),
                BezierSegment.Cubic(
                    new Point2(cx - r, cy),
                    new Point2(cx - r, cy - kr),
                    new Point2(cx - kr, cy - r),
                    new Point2(cx, cy - r)),
                BezierSegment.Cubic(
                    new Point2(cx, cy - r),
                    new Point2(cx + kr, cy - r),
                    new Point2(cx + r, cy - kr),
                    new Point2(cx + r, cy)),
            };

            return new Contour(segments, true, shapeId, subPath);
        }

        /// <summary>Halka: dış daire + iç daire (delik).</summary>
        public static ContourSet Ring(int shapeId, double cx, double cy, double outerRadius, double innerRadius) =>
            Set(
                Circle(shapeId, 0, cx, cy, outerRadius),
                Circle(shapeId, 1, cx, cy, innerRadius));

        public static ContourSet LetterO(int shapeId = 1) =>
            Set(
                Rect(shapeId, 0, 0, 0, 100, 100),
                Rect(shapeId, 1, 25, 25, 50, 50));

        public static ContourSet LetterA(int shapeId = 1) =>
            Set(
                Polygon(shapeId, 0,
                    new Point2(0, 0), new Point2(18, 100), new Point2(82, 100), new Point2(100, 0)),
                Polygon(shapeId, 1,
                    new Point2(50, 25), new Point2(38, 70), new Point2(62, 70)));

        public static ContourSet LetterB(int shapeId = 1) =>
            Set(
                Rect(shapeId, 0, 0, 0, 70, 100),
                Rect(shapeId, 1, 15, 55, 35, 30),
                Rect(shapeId, 2, 15, 15, 35, 30));

        public static ContourSet LetterP(int shapeId = 1) =>
            Set(
                Rect(shapeId, 0, 0, 0, 65, 100),
                Rect(shapeId, 1, 15, 55, 35, 32));

        /// <summary>R: gövde + üst delik + bacak (gövdenin parçası).</summary>
        public static ContourSet LetterR(int shapeId = 1) =>
            Set(
                Polygon(shapeId, 0,
                    new Point2(0, 0), new Point2(0, 100), new Point2(55, 100),
                    new Point2(65, 92), new Point2(65, 65), new Point2(55, 55),
                    new Point2(70, 0), new Point2(50, 0), new Point2(38, 50),
                    new Point2(20, 50), new Point2(20, 0)),
                Rect(shapeId, 1, 20, 60, 30, 25));

        /// <summary>İ: gövde + ayrı nokta (containment yok, bağlama yok).</summary>
        public static ContourSet LetterIWithDot(int shapeId = 1) =>
            Set(
                Rect(shapeId, 0, 0, 0, 20, 80),
                Rect(shapeId, 1, 0, 90, 20, 10));

        /// <summary>j: çengel + ayrı nokta.</summary>
        public static ContourSet LetterJWithDot(int shapeId = 1) =>
            Set(
                Polygon(shapeId, 0,
                    new Point2(0, 0), new Point2(30, 0), new Point2(30, 70),
                    new Point2(20, 70), new Point2(20, 10), new Point2(0, 10)),
                Rect(shapeId, 1, 20, 80, 10, 10));

        /// <summary>C biçimli gövde (sağa açık).</summary>
        public static Contour CShape(int shapeId, int subPath, double cx, double cy, double outerRadius, double innerRadius) =>
            ArcRing(shapeId, subPath, cx, cy, outerRadius, innerRadius, 45, 315, 48);

        public static Contour ArcRing(
            int shapeId,
            int subPath,
            double cx,
            double cy,
            double outerRadius,
            double innerRadius,
            double startDegrees,
            double endDegrees,
            int steps)
        {
            var points = new List<Point2>();
            for (int i = 0; i <= steps; i++)
            {
                double angle = (startDegrees + (endDegrees - startDegrees) * i / steps) * Math.PI / 180d;
                points.Add(new Point2(cx + outerRadius * Math.Cos(angle), cy + outerRadius * Math.Sin(angle)));
            }

            for (int i = steps; i >= 0; i--)
            {
                double angle = (startDegrees + (endDegrees - startDegrees) * i / steps) * Math.PI / 180d;
                points.Add(new Point2(cx + innerRadius * Math.Cos(angle), cy + innerRadius * Math.Sin(angle)));
            }

            return Polygon(shapeId, subPath, points.ToArray());
        }

        /// <summary>Ç: gövde + gövdeye bağlı olmayan çengel (ayrık).</summary>
        public static ContourSet LetterCWithCedilla(int shapeId = 1) =>
            Set(
                CShape(shapeId, 0, 50, 50, 50, 30),
                Polygon(shapeId, 1,
                    new Point2(40, -14), new Point2(60, -14), new Point2(54, -2), new Point2(46, -2)));

        /// <summary>Ğ: gövde + üstte ayrık breve.</summary>
        public static ContourSet LetterGWithBreve(int shapeId = 1) =>
            Set(
                CShape(shapeId, 0, 50, 50, 50, 30),
                Polygon(shapeId, 1,
                    new Point2(28, 108), new Point2(72, 108), new Point2(72, 116),
                    new Point2(64, 116), new Point2(61, 113), new Point2(39, 113),
                    new Point2(36, 116), new Point2(28, 116)));

        /// <summary>Birleşik yazı benzeri 7 mantıksal harf (MUSTAFA), iki A delikli.</summary>
        public static ContourSet SyntheticWordMustafa(int shapeId = 1)
        {
            var contours = new List<Contour>();
            double x = 0;

            contours.Add(Translate(BlockM(shapeId, 0), x));
            x += 50;
            contours.Add(Translate(BlockU(shapeId, 1), x));
            x += 50;
            contours.Add(Translate(BlockS(shapeId, 2), x));
            x += 46;
            contours.Add(Translate(BlockT(shapeId, 3), x));
            x += 46;
            contours.Add(Translate(BlockTrapezoidA(shapeId, 4), x));
            contours.Add(Translate(Rect(shapeId, 5, 18, 25, 12, 30), x));
            x += 50;
            contours.Add(Translate(BlockF(shapeId, 6), x));
            x += 46;
            contours.Add(Translate(BlockTrapezoidA(shapeId, 7), x));
            contours.Add(Translate(Rect(shapeId, 8, 18, 25, 12, 30), x));

            return new ContourSet(contours);
        }

        public static Contour Translate(Contour contour, double dx)
        {
            var segments = new List<BezierSegment>(contour.SegmentCount);
            foreach (var s in contour.Segments)
            {
                segments.Add(new BezierSegment(
                    new Point2(s.P0.X + dx, s.P0.Y),
                    new Point2(s.P1.X + dx, s.P1.Y),
                    new Point2(s.P2.X + dx, s.P2.Y),
                    new Point2(s.P3.X + dx, s.P3.Y),
                    s.Kind));
            }

            return new Contour(segments, contour.IsClosed, contour.SourceShapeId, contour.SourceSubPathIndex);
        }

        private static Contour BlockM(int shapeId, int subPath) =>
            Polygon(shapeId, subPath,
                new Point2(0, 0), new Point2(40, 0), new Point2(40, 80), new Point2(28, 80),
                new Point2(20, 35), new Point2(12, 80), new Point2(0, 80));

        private static Contour BlockU(int shapeId, int subPath) =>
            Polygon(shapeId, subPath,
                new Point2(0, 0), new Point2(0, 80), new Point2(12, 80), new Point2(12, 15),
                new Point2(28, 15), new Point2(28, 80), new Point2(40, 80), new Point2(40, 0));

        private static Contour BlockS(int shapeId, int subPath) =>
            Polygon(shapeId, subPath,
                new Point2(0, 0), new Point2(36, 0), new Point2(36, 10), new Point2(10, 10),
                new Point2(10, 32), new Point2(36, 32), new Point2(36, 80), new Point2(0, 80),
                new Point2(0, 70), new Point2(26, 70), new Point2(26, 42), new Point2(0, 42));

        private static Contour BlockT(int shapeId, int subPath) =>
            Polygon(shapeId, subPath,
                new Point2(0, 80), new Point2(36, 80), new Point2(36, 70), new Point2(23, 70),
                new Point2(23, 0), new Point2(13, 0), new Point2(13, 70), new Point2(0, 70));

        private static Contour BlockTrapezoidA(int shapeId, int subPath) =>
            Polygon(shapeId, subPath,
                new Point2(0, 0), new Point2(12, 80), new Point2(28, 80), new Point2(40, 0));

        private static Contour BlockF(int shapeId, int subPath) =>
            Polygon(shapeId, subPath,
                new Point2(0, 0), new Point2(0, 80), new Point2(36, 80), new Point2(36, 70),
                new Point2(10, 70), new Point2(10, 45), new Point2(32, 45), new Point2(32, 35),
                new Point2(10, 35), new Point2(10, 0));

        /// <summary>GDI+ font dış konturlarını POCO modele çevirir (Y yukarı çevrilir).</summary>
        public static ContourSet FromText(string text, string fontFamily = "Arial", float emSize = 200f, int shapeId = 1)
        {
            using (var family = new FontFamily(fontFamily))
            using (var path = new GraphicsPath())
            {
                path.AddString(
                    text,
                    family,
                    (int)FontStyle.Regular,
                    emSize,
                    new PointF(0f, 0f),
                    StringFormat.GenericTypographic);
                return FromGraphicsPath(path, shapeId);
            }
        }

        public static ContourSet FromGraphicsPath(GraphicsPath path, int shapeId = 1)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var points = path.PathPoints;
            var types = path.PathTypes;
            var contours = new List<Contour>();
            var segments = new List<BezierSegment>();

            bool inSubPath = false;
            bool closed = false;
            int subPathIndex = 0;
            Point2 current = Point2.Origin;

            void Flush()
            {
                if (inSubPath && segments.Count > 0)
                {
                    contours.Add(new Contour(segments, closed, shapeId, subPathIndex++));
                }

                segments = new List<BezierSegment>();
                inSubPath = false;
                closed = false;
            }

            for (int i = 0; i < points.Length;)
            {
                int rawType = types[i];
                int pointType = rawType & 0x07;
                bool closeFlag = (rawType & 0x80) != 0;

                if (pointType == 0) // Start
                {
                    Flush();
                    inSubPath = true;
                    current = ToPoint(points[i]);
                    i++;
                    continue;
                }

                if (!inSubPath)
                {
                    inSubPath = true;
                    current = ToPoint(points[i]);
                }

                if (pointType == 1) // Line
                {
                    var end = ToPoint(points[i]);
                    if (current.DistanceTo(end) > 1e-9)
                    {
                        segments.Add(BezierSegment.Line(current, end));
                    }

                    current = end;
                    i++;
                }
                else if (pointType == 3) // Cubic bezier üçlüsü
                {
                    var c1 = ToPoint(points[i]);
                    var c2 = ToPoint(points[i + 1]);
                    var end = ToPoint(points[i + 2]);
                    segments.Add(BezierSegment.Cubic(current, c1, c2, end));
                    current = end;
                    closeFlag = (types[i + 2] & 0x80) != 0;
                    i += 3;
                }
                else
                {
                    var end = ToPoint(points[i]);
                    segments.Add(BezierSegment.Line(current, end));
                    current = end;
                    i++;
                }

                if (closeFlag) closed = true;
            }

            Flush();
            return new ContourSet(contours);
        }

        private static Point2 ToPoint(PointF p) => new Point2(p.X, -p.Y);

        public static string Signature(SmartDivideAnalysis analysis)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < analysis.Candidates.Count; i++)
            {
                var candidate = analysis.Candidates[i];
                sb.Append("C").Append(candidate.Id)
                    .Append(":outer=").Append(candidate.OuterIndex)
                    .Append(":holes=").Append(string.Join(",", candidate.HoleIndices))
                    .Append(":depth=").Append(candidate.Depth)
                    .Append(';');
            }

            for (int i = 0; i < analysis.Problems.Count; i++)
            {
                var problem = analysis.Problems[i];
                sb.Append(problem.Code).Append('@').Append(problem.ContourIndex)
                    .Append('>').Append(problem.RelatedContourIndex)
                    .Append(';');
            }

            return sb.ToString();
        }

        public static int CountProblems(SmartDivideAnalysis analysis, ProblemCode code)
        {
            int count = 0;
            for (int i = 0; i < analysis.Problems.Count; i++)
            {
                if (analysis.Problems[i].Code == code) count++;
            }

            return count;
        }
    }
}
