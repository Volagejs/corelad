using System;
using System.Globalization;

namespace Corelad.Geometry.Model
{
    /// <summary>
    /// 2B nokta. Kanonik birim mm, Y yukarı pozitif (ADR-0005).
    /// </summary>
    public readonly struct Point2 : IEquatable<Point2>
    {
        public static readonly Point2 Origin = new Point2(0d, 0d);

        public Point2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }

        public double Y { get; }

        public bool IsFinite =>
            !double.IsNaN(X) && !double.IsInfinity(X) &&
            !double.IsNaN(Y) && !double.IsInfinity(Y);

        public double DistanceTo(Point2 other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public double DistanceSquaredTo(Point2 other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return dx * dx + dy * dy;
        }

        public static Point2 operator +(Point2 a, Point2 b) => new Point2(a.X + b.X, a.Y + b.Y);

        public static Point2 operator -(Point2 a, Point2 b) => new Point2(a.X - b.X, a.Y - b.Y);

        public static Point2 operator *(Point2 a, double scale) => new Point2(a.X * scale, a.Y * scale);

        public static Point2 operator /(Point2 a, double scale) => new Point2(a.X / scale, a.Y / scale);

        public bool Equals(Point2 other) => X.Equals(other.X) && Y.Equals(other.Y);

        public override bool Equals(object? obj) => obj is Point2 other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public static bool operator ==(Point2 a, Point2 b) => a.Equals(b);

        public static bool operator !=(Point2 a, Point2 b) => !a.Equals(b);

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0:0.####}, {1:0.####})", X, Y);
    }
}
