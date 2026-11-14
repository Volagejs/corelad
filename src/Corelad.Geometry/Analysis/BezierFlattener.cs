using System;
using System.Collections.Generic;
using Corelad.Geometry.Model;

namespace Corelad.Geometry.Analysis
{
    /// <summary>
    /// Adaptif kübik bezier düzleştirici. Yalnızca analiz kopyası üretir; çıktı geometrisi
    /// her zaman orijinal bezier'dir (AGENTS.md §1).
    /// </summary>
    public static class BezierFlattener
    {
        public static FlattenedContour Flatten(Contour contour, int index, GeometryTolerances tolerances)
        {
            if (contour == null) throw new ArgumentNullException(nameof(contour));
            if (tolerances == null) throw new ArgumentNullException(nameof(tolerances));
            tolerances.Validate();

            double epsilon = tolerances.ComparisonEpsilonMm;
            var points = new List<Point2>(64);
            bool hitLimit = false;
            bool broken = false;

            if (contour.SegmentCount == 0)
            {
                return new FlattenedContour(index, points, contour.IsClosed, 0d, 0d, Rect2.Empty, false, false);
            }

            var current = contour.Start;
            points.Add(current);

            foreach (var segment in contour.Segments)
            {
                if (segment.P0.DistanceTo(current) > epsilon) broken = true;

                if (segment.Kind == SegmentKind.Line)
                {
                    AddDistinct(points, segment.P3, epsilon);
                }
                else
                {
                    FlattenCubic(
                        segment,
                        tolerances.FlatteningToleranceMm,
                        tolerances.MaxFlatteningDepth,
                        epsilon,
                        0,
                        points,
                        ref hitLimit);
                }

                current = segment.P3;
            }

            if (contour.IsClosed && points.Count > 1 &&
                points[0].DistanceTo(points[points.Count - 1]) <= epsilon)
            {
                points.RemoveAt(points.Count - 1);
            }

            if (points.Count < 3)
            {
                return new FlattenedContour(index, points, contour.IsClosed, 0d, 0d, Rect2.Empty, hitLimit, broken);
            }

            double signedArea = Polygon2.SignedArea(points);
            var bounds = Rect2.FromPoints(points);
            return new FlattenedContour(
                index,
                points,
                contour.IsClosed,
                signedArea,
                Math.Abs(signedArea),
                bounds,
                hitLimit,
                broken);
        }

        private static void FlattenCubic(
            BezierSegment segment,
            double tolerance,
            int maxDepth,
            double epsilon,
            int depth,
            List<Point2> points,
            ref bool hitLimit)
        {
            if (depth >= maxDepth)
            {
                if (segment.Flatness > tolerance) hitLimit = true;
                AddDistinct(points, segment.P3, epsilon);
                return;
            }

            if (segment.Flatness <= tolerance)
            {
                AddDistinct(points, segment.P3, epsilon);
                return;
            }

            segment.Split(0.5d, out var left, out var right);
            FlattenCubic(left, tolerance, maxDepth, epsilon, depth + 1, points, ref hitLimit);
            FlattenCubic(right, tolerance, maxDepth, epsilon, depth + 1, points, ref hitLimit);
        }

        private static void AddDistinct(List<Point2> points, Point2 candidate, double epsilon)
        {
            if (points.Count > 0)
            {
                var last = points[points.Count - 1];
                if (last.DistanceSquaredTo(candidate) <= epsilon * epsilon) return;
            }

            points.Add(candidate);
        }
    }
}
