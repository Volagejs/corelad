using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Corelad.Geometry.Model;

namespace Corelad.SmokeTest
{
    /// <summary>
    /// GDI+ Arial dış konturlarını POCO modele çevirir. Yalnızca smoke girdisi üretir;
    /// CorelDRAW'a yazılan geometri bu bezier'lerin aynısıdır (poligonlaştırma yok).
    /// </summary>
    internal static class FontOutlineSource
    {
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

        private static ContourSet FromGraphicsPath(GraphicsPath path, int shapeId)
        {
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
                    // Kapalı konturda kapanış örtükse düz çizgi olarak açık hale getir
                    // (modelin kapalılık semantiği; CorelDRAW yazıcısı da aynı kuralı uygular).
                    var lastPoint = segments[segments.Count - 1].P3;
                    var firstPoint = segments[0].P0;
                    if (closed && lastPoint.DistanceTo(firstPoint) > 1e-9)
                    {
                        segments.Add(BezierSegment.Line(lastPoint, firstPoint));
                    }

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

                if (pointType == 0)
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

                if (pointType == 1)
                {
                    var end = ToPoint(points[i]);
                    if (current.DistanceTo(end) > 1e-9)
                    {
                        segments.Add(BezierSegment.Line(current, end));
                    }

                    current = end;
                    i++;
                }
                else if (pointType == 3)
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

        private static Point2 ToPoint(PointF p)
        {
            // GDI+ Y aşağı pozitif; modele Y yukarı pozitif olarak girer.
            return new Point2(p.X, -p.Y);
        }
    }
}
