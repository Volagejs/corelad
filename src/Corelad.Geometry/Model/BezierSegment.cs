using System;

namespace Corelad.Geometry.Model
{
    /// <summary>
    /// Tek bezier segment. Kübik; çizgi için <see cref="SegmentKind.Line"/> kullanılır.
    /// Geometri asla burada sadeleştirilmez veya onarılmaz.
    /// </summary>
    public sealed class BezierSegment
    {
        public BezierSegment(Point2 p0, Point2 p1, Point2 p2, Point2 p3, SegmentKind kind)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
            Kind = kind;
        }

        public Point2 P0 { get; }

        public Point2 P1 { get; }

        public Point2 P2 { get; }

        public Point2 P3 { get; }

        public SegmentKind Kind { get; }

        public static BezierSegment Line(Point2 start, Point2 end) =>
            new BezierSegment(start, start, end, end, SegmentKind.Line);

        public static BezierSegment Cubic(Point2 p0, Point2 p1, Point2 p2, Point2 p3) =>
            new BezierSegment(p0, p1, p2, p3, SegmentKind.Cubic);

        public Point2 Evaluate(double t)
        {
            if (Kind == SegmentKind.Line)
            {
                return new Point2(
                    P0.X + (P3.X - P0.X) * t,
                    P0.Y + (P3.Y - P0.Y) * t);
            }

            double mt = 1d - t;
            double a = mt * mt * mt;
            double b = 3d * mt * mt * t;
            double c = 3d * mt * t * t;
            double d = t * t * t;
            return new Point2(
                a * P0.X + b * P1.X + c * P2.X + d * P3.X,
                a * P0.Y + b * P1.Y + c * P2.Y + d * P3.Y);
        }

        /// <summary>Kontrol noktalarının kirişten maksimum sapması (düzleştirme kriteri).</summary>
        public double Flatness
        {
            get
            {
                if (Kind == SegmentKind.Line) return 0d;
                return Math.Max(DistanceToChord(P1), DistanceToChord(P2));
            }
        }

        public void Split(double t, out BezierSegment left, out BezierSegment right)
        {
            if (Kind == SegmentKind.Line)
            {
                var mid = Evaluate(t);
                left = Line(P0, mid);
                right = Line(mid, P3);
                return;
            }

            Point2 p01 = Lerp(P0, P1, t);
            Point2 p12 = Lerp(P1, P2, t);
            Point2 p23 = Lerp(P2, P3, t);
            Point2 p012 = Lerp(p01, p12, t);
            Point2 p123 = Lerp(p12, p23, t);
            Point2 p0123 = Lerp(p012, p123, t);

            left = Cubic(P0, p01, p012, p0123);
            right = Cubic(p0123, p123, p23, P3);
        }

        private double DistanceToChord(Point2 p)
        {
            double dx = P3.X - P0.X;
            double dy = P3.Y - P0.Y;
            double lenSquared = dx * dx + dy * dy;
            if (lenSquared <= double.Epsilon) return P0.DistanceTo(p);

            double cross = Math.Abs((p.X - P0.X) * dy - (p.Y - P0.Y) * dx);
            return cross / Math.Sqrt(lenSquared);
        }

        private static Point2 Lerp(Point2 a, Point2 b, double t) =>
            new Point2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
    }
}
