using System;
using System.Globalization;
using System.Text;
using Corelad.Geometry;
using Corelad.Geometry.Model;
using Corel.Interop.VGCore;

namespace Corelad.Dock.Corel
{
    /// <summary>
    /// Kaynak şeklin değişmediğini doğrulamak için deterministik geometri imzası üretir.
    /// İmza yalnızca okuma yapar; kaynak nesneye dokunmaz.
    /// </summary>
    public static class CorelShapeSnapshot
    {
        public static string Capture(Document document, Shape shape, GeometryTolerances tolerances)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            if (tolerances == null) throw new ArgumentNullException(nameof(tolerances));

            var contours = CorelCurveReader.Read(document, shape, 0, tolerances);
            var builder = new StringBuilder();
            builder.Append("contours=").Append(contours.Count).Append(';');

            for (int c = 0; c < contours.Count; c++)
            {
                var contour = contours.Contours[c];
                builder.Append(contour.IsClosed ? "C" : "O")
                    .Append(contour.SegmentCount)
                    .Append('[');

                for (int s = 0; s < contour.SegmentCount; s++)
                {
                    var segment = contour.Segments[s];
                    builder.Append(segment.Kind == SegmentKind.Line ? "L" : "B");
                    AppendPoint(builder, segment.P0);
                    if (segment.Kind == SegmentKind.Cubic)
                    {
                        AppendPoint(builder, segment.P1);
                        AppendPoint(builder, segment.P2);
                    }

                    AppendPoint(builder, segment.P3);
                    builder.Append(';');
                }

                builder.Append(']');
            }

            return builder.ToString();
        }

        private static void AppendPoint(StringBuilder builder, Point2 point)
        {
            AppendValue(builder, point.X);
            builder.Append(',');
            AppendValue(builder, point.Y);
            builder.Append(' ');
        }

        private static void AppendValue(StringBuilder builder, double value)
        {
            double rounded = Math.Round(value, CorelTolerances.SignatureDecimals, MidpointRounding.AwayFromZero);
            if (rounded == 0d) rounded = 0d; // -0 ile +0 aynı imzayı üretsin.
            builder.Append(rounded.ToString("0.####", CultureInfo.InvariantCulture));
        }
    }
}
