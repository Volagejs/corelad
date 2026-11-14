using System;
using System.Collections.Generic;
using Corelad.Geometry.Model;

namespace Corelad.Geometry.Analysis
{
    /// <summary>Nokta-kontur konumu. Boundary = tolerans bandında (belirsiz).</summary>
    internal enum PointLocation
    {
        Outside = 0,
        Inside = 1,
        Boundary = 2,
    }

    /// <summary>İki kenar arasındaki temas türü.</summary>
    internal enum SegmentContact
    {
        None = 0,
        Touch = 1,
        Cross = 2,
    }

    /// <summary>
    /// Poligon (analiz kopyası) üzerinde saf geometri yardımcıları.
    /// Buradaki hiçbir işlem sonucu CorelDRAW'a yazılmaz.
    /// </summary>
    internal static class Polygon2
    {
        public static double SignedArea(IReadOnlyList<Point2> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count < 3) return 0d;

            double sum = 0d;
            for (int i = 0; i < points.Count; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];
                sum += a.X * b.Y - b.X * a.Y;
            }

            return sum * 0.5d;
        }

        public static double DistancePointToSegment(Point2 p, Point2 a, Point2 b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lengthSquared = dx * dx + dy * dy;
            if (lengthSquared <= 1e-24) return p.DistanceTo(a);

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared;
            if (t < 0d) t = 0d;
            else if (t > 1d) t = 1d;

            double x = a.X + t * dx;
            double y = a.Y + t * dy;
            double ex = p.X - x;
            double ey = p.Y - y;
            return Math.Sqrt(ex * ex + ey * ey);
        }

        public static PointLocation Locate(IReadOnlyList<Point2> polygon, Point2 point, double band)
        {
            if (polygon == null) throw new ArgumentNullException(nameof(polygon));
            if (polygon.Count < 3) return PointLocation.Outside;

            bool inside = false;
            int count = polygon.Count;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                var a = polygon[j];
                var b = polygon[i];

                if (DistancePointToSegment(point, a, b) <= band) return PointLocation.Boundary;

                if ((a.Y > point.Y) != (b.Y > point.Y))
                {
                    double x = a.X + (point.Y - a.Y) * (b.X - a.X) / (b.Y - a.Y);
                    if (x > point.X) inside = !inside;
                }
            }

            return inside ? PointLocation.Inside : PointLocation.Outside;
        }

        public static bool HasSelfIntersection(
            IReadOnlyList<Point2> polygon,
            double epsilon,
            out int edgeA,
            out int edgeB)
        {
            edgeA = -1;
            edgeB = -1;
            int count = polygon.Count;
            if (count < 4) return false;

            for (int i = 0; i < count; i++)
            {
                var a0 = polygon[i];
                var a1 = polygon[(i + 1) % count];
                double aMinX = Math.Min(a0.X, a1.X) - epsilon;
                double aMaxX = Math.Max(a0.X, a1.X) + epsilon;
                double aMinY = Math.Min(a0.Y, a1.Y) - epsilon;
                double aMaxY = Math.Max(a0.Y, a1.Y) + epsilon;

                for (int j = i + 1; j < count; j++)
                {
                    if (j == i + 1) continue;
                    if (i == 0 && j == count - 1) continue;

                    var b0 = polygon[j];
                    var b1 = polygon[(j + 1) % count];
                    if (Math.Max(b0.X, b1.X) < aMinX || Math.Min(b0.X, b1.X) > aMaxX) continue;
                    if (Math.Max(b0.Y, b1.Y) < aMinY || Math.Min(b0.Y, b1.Y) > aMaxY) continue;

                    if (ClassifyContact(a0, a1, b0, b1, epsilon) != SegmentContact.None)
                    {
                        edgeA = i;
                        edgeB = j;
                        return true;
                    }
                }
            }

            return false;
        }

        public static SegmentContact ClassifyContact(Point2 a0, Point2 a1, Point2 b0, Point2 b1, double epsilon)
        {
            int s1 = Side(a0, a1, b0, epsilon);
            int s2 = Side(a0, a1, b1, epsilon);
            int s3 = Side(b0, b1, a0, epsilon);
            int s4 = Side(b0, b1, a1, epsilon);

            if (s1 * s2 < 0 && s3 * s4 < 0) return SegmentContact.Cross;

            if (s1 == 0 && OnSegment(a0, a1, b0, epsilon)) return SegmentContact.Touch;
            if (s2 == 0 && OnSegment(a0, a1, b1, epsilon)) return SegmentContact.Touch;
            if (s3 == 0 && OnSegment(b0, b1, a0, epsilon)) return SegmentContact.Touch;
            if (s4 == 0 && OnSegment(b0, b1, a1, epsilon)) return SegmentContact.Touch;

            return SegmentContact.None;
        }

        /// <summary>
        /// İki kapalı konturun sınır teması. Kesin kesişme varsa <see cref="SegmentContact.Cross"/>,
        /// yoksa tolerans bandı içinde temas varsa <see cref="SegmentContact.Touch"/>.
        /// <paramref name="minDistance"/> kesişme yoksa sınırlar arası minimum mesafedir.
        /// </summary>
        public static SegmentContact ClassifyBoundaryContact(
            IReadOnlyList<Point2> a,
            IReadOnlyList<Point2> b,
            double band,
            out double minDistance)
        {
            minDistance = double.PositiveInfinity;
            bool touch = false;
            int na = a.Count;
            int nb = b.Count;

            for (int i = 0; i < na; i++)
            {
                var a0 = a[i];
                var a1 = a[(i + 1) % na];
                double aMinX = Math.Min(a0.X, a1.X) - band;
                double aMaxX = Math.Max(a0.X, a1.X) + band;
                double aMinY = Math.Min(a0.Y, a1.Y) - band;
                double aMaxY = Math.Max(a0.Y, a1.Y) + band;

                for (int j = 0; j < nb; j++)
                {
                    var b0 = b[j];
                    var b1 = b[(j + 1) % nb];
                    if (Math.Max(b0.X, b1.X) < aMinX || Math.Min(b0.X, b1.X) > aMaxX) continue;
                    if (Math.Max(b0.Y, b1.Y) < aMinY || Math.Min(b0.Y, b1.Y) > aMaxY) continue;

                    var contact = ClassifyContact(a0, a1, b0, b1, band);
                    if (contact == SegmentContact.Cross)
                    {
                        minDistance = 0d;
                        return SegmentContact.Cross;
                    }

                    if (contact == SegmentContact.Touch) touch = true;
                    double d = MinSegmentDistance(a0, a1, b0, b1);
                    if (d < minDistance) minDistance = d;
                }
            }

            return touch || minDistance <= band ? SegmentContact.Touch : SegmentContact.None;
        }

        public static double MinSegmentDistance(Point2 a0, Point2 a1, Point2 b0, Point2 b1)
        {
            double d = DistancePointToSegment(a0, b0, b1);
            double candidate = DistancePointToSegment(a1, b0, b1);
            if (candidate < d) d = candidate;
            candidate = DistancePointToSegment(b0, a0, a1);
            if (candidate < d) d = candidate;
            candidate = DistancePointToSegment(b1, a0, a1);
            if (candidate < d) d = candidate;
            return d;
        }

        /// <summary>
        /// Konturun içinde, sınırdan en az <paramref name="band"/> uzakta güvenilir bir nokta arar.
        /// Ağırlık merkezi konkav konturlarda yetersiz olduğu için tarama çizgisi yedeği kullanılır.
        /// </summary>
        public static bool TryFindInteriorPoint(
            IReadOnlyList<Point2> polygon,
            Rect2 bounds,
            double band,
            out Point2 interior)
        {
            interior = Point2.Origin;
            if (polygon.Count < 3 || bounds.IsEmpty) return false;

            double area = SignedArea(polygon);
            if (Math.Abs(area) > 0d)
            {
                double cx = 0d, cy = 0d;
                for (int i = 0; i < polygon.Count; i++)
                {
                    var a = polygon[i];
                    var b = polygon[(i + 1) % polygon.Count];
                    double cross = a.X * b.Y - b.X * a.Y;
                    cx += (a.X + b.X) * cross;
                    cy += (a.Y + b.Y) * cross;
                }

                cx /= 6d * area;
                cy /= 6d * area;
                var centroid = new Point2(cx, cy);
                if (Locate(polygon, centroid, band) == PointLocation.Inside)
                {
                    interior = centroid;
                    return true;
                }
            }

            if (TryScanlines(polygon, bounds, band, horizontal: true, out interior)) return true;
            if (TryScanlines(polygon, bounds, band, horizontal: false, out interior)) return true;
            return false;
        }

        private static bool TryScanlines(
            IReadOnlyList<Point2> polygon,
            Rect2 bounds,
            double band,
            bool horizontal,
            out Point2 interior)
        {
            interior = Point2.Origin;
            double span = horizontal ? bounds.Height : bounds.Width;
            if (span <= 0d) return false;

            var crossings = new List<double>(16);
            double bestWidth = 0d;
            bool found = false;

            for (int k = 1; k <= 9; k++)
            {
                double level = horizontal
                    ? bounds.MinY + span * k / 10d
                    : bounds.MinX + span * k / 10d;

                crossings.Clear();
                for (int i = 0; i < polygon.Count; i++)
                {
                    var a = polygon[i];
                    var b = polygon[(i + 1) % polygon.Count];
                    double aLevel = horizontal ? a.Y : a.X;
                    double bLevel = horizontal ? b.Y : b.X;
                    if ((aLevel > level) == (bLevel > level)) continue;

                    double t = (level - aLevel) / (bLevel - aLevel);
                    double coord = horizontal
                        ? a.X + t * (b.X - a.X)
                        : a.Y + t * (b.Y - a.Y);
                    crossings.Add(coord);
                }

                if (crossings.Count < 2) continue;
                crossings.Sort();

                for (int i = 0; i + 1 < crossings.Count; i += 2)
                {
                    double width = crossings[i + 1] - crossings[i];
                    if (width <= 2d * band) continue;

                    double middle = (crossings[i] + crossings[i + 1]) * 0.5d;
                    var candidate = horizontal
                        ? new Point2(middle, level)
                        : new Point2(level, middle);

                    if (Locate(polygon, candidate, band) != PointLocation.Inside) continue;
                    if (!found || width > bestWidth)
                    {
                        bestWidth = width;
                        interior = candidate;
                        found = true;
                    }
                }
            }

            return found;
        }

        private static int Side(Point2 a, Point2 b, Point2 p, double epsilon)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            double cross = dx * (p.Y - a.Y) - dy * (p.X - a.X);
            double threshold = epsilon * Math.Max(length, 1e-12);
            if (cross > threshold) return 1;
            if (cross < -threshold) return -1;
            return 0;
        }

        private static bool OnSegment(Point2 a, Point2 b, Point2 p, double epsilon)
        {
            if (p.X < Math.Min(a.X, b.X) - epsilon || p.X > Math.Max(a.X, b.X) + epsilon) return false;
            if (p.Y < Math.Min(a.Y, b.Y) - epsilon || p.Y > Math.Max(a.Y, b.Y) + epsilon) return false;
            return DistancePointToSegment(p, a, b) <= epsilon;
        }
    }
}
