using System;
using System.Collections.Generic;
using System.Globalization;

namespace Corelad.Geometry.Model
{
    /// <summary>Eksen hizalı 2B kutu. Kanonik birim mm.</summary>
    public readonly struct Rect2
    {
        public Rect2(double minX, double minY, double maxX, double maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public static Rect2 Empty =>
            new Rect2(double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity);

        public double MinX { get; }

        public double MinY { get; }

        public double MaxX { get; }

        public double MaxY { get; }

        public double Width => MaxX - MinX;

        public double Height => MaxY - MinY;

        public double Area => Width * Height;

        public bool IsEmpty => MinX > MaxX || MinY > MaxY;

        public Point2 Center => new Point2((MinX + MaxX) * 0.5, (MinY + MaxY) * 0.5);

        public static Rect2 FromPoints(IReadOnlyList<Point2> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count == 0) return Empty;

            double minX = points[0].X, minY = points[0].Y;
            double maxX = minX, maxY = minY;
            for (int i = 1; i < points.Count; i++)
            {
                var p = points[i];
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            return new Rect2(minX, minY, maxX, maxY);
        }

        public Rect2 Inflate(double amount) => new Rect2(MinX - amount, MinY - amount, MaxX + amount, MaxY + amount);

        public bool Contains(Point2 p) => p.X >= MinX && p.X <= MaxX && p.Y >= MinY && p.Y <= MaxY;

        public bool Contains(Rect2 other) =>
            !other.IsEmpty &&
            other.MinX >= MinX && other.MaxX <= MaxX &&
            other.MinY >= MinY && other.MaxY <= MaxY;

        public bool Overlaps(Rect2 other) =>
            !IsEmpty && !other.IsEmpty &&
            MaxX >= other.MinX && other.MaxX >= MinX &&
            MaxY >= other.MinY && other.MaxY >= MinY;

        public double HorizontalOverlap(Rect2 other) =>
            Math.Min(MaxX, other.MaxX) - Math.Max(MinX, other.MinX);

        /// <summary>Dikey ayrılma mesafesi. Kutular dikey olarak örtüşüyorsa negatif değer döner.</summary>
        public double VerticalGap(Rect2 other)
        {
            if (MinY > other.MaxY) return MinY - other.MaxY;
            if (other.MinY > MaxY) return other.MinY - MaxY;
            return -Math.Min(MaxY - other.MinY, other.MaxY - MinY);
        }

        /// <summary>İki kutu arasındaki Öklid mesafesi (örtüşüyorsa 0).</summary>
        public double DistanceTo(Rect2 other)
        {
            double dx = Math.Max(0d, Math.Max(MinX - other.MaxX, other.MinX - MaxX));
            double dy = Math.Max(0d, Math.Max(MinY - other.MaxY, other.MinY - MaxY));
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static Rect2 Union(Rect2 a, Rect2 b)
        {
            if (a.IsEmpty) return b;
            if (b.IsEmpty) return a;
            return new Rect2(
                Math.Min(a.MinX, b.MinX),
                Math.Min(a.MinY, b.MinY),
                Math.Max(a.MaxX, b.MaxX),
                Math.Max(a.MaxY, b.MaxY));
        }

        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "[{0:0.###}, {1:0.###}] - [{2:0.###}, {3:0.###}]",
                MinX,
                MinY,
                MaxX,
                MaxY);
    }
}
