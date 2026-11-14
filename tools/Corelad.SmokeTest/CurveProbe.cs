using System;
using System.Collections.Generic;
using System.Globalization;
using Corelad.Dock.Corel;
using Corelad.Geometry;
using Corel.Interop.VGCore;

namespace Corelad.SmokeTest
{
    /// <summary>
    /// Ampirik API probe'u: <c>CreateCurveFromArray</c> ile oluşturulan eğride
    /// <c>Closed</c> davranışını ve kapanış segmentinin nasıl temsil edildiğini ölçer.
    /// Belgeyi kaydetmez.
    /// </summary>
    internal static class CurveProbe
    {
        public static int Run()
        {
            var tolerances = new GeometryTolerances();
            var application = CorelApplicationFactory.Create();
            Document document = null;

            try
            {
                document = application.CreateDocument();
                document.Unit = cdrUnit.cdrMillimeter;

                Console.WriteLine("--- Probe 1: acikca kapanis cizgisi iceren kare (5 eleman) ---");
                var square = CreateSquare(document, 0, 0, 10, withExplicitClosingLine: true);
                Dump(document, square, tolerances, "explicit-close");
                square.SubPaths[1].Closed = true;
                Console.WriteLine("[Closed=true atandi]");
                Dump(document, square, tolerances, "explicit-close+Closed");

                Console.WriteLine();
                Console.WriteLine("--- Probe 2: kapanis cizgisi olmayan kare (4 eleman) ---");
                var square2 = CreateSquare(document, 20, 0, 30, withExplicitClosingLine: false);
                Dump(document, square2, tolerances, "no-close");
                square2.SubPaths[1].Closed = true;
                Console.WriteLine("[Closed=true atandi]");
                Dump(document, square2, tolerances, "no-close+Closed");

                Console.WriteLine();
                Console.WriteLine("--- Probe 3: 4 kubik daire (kapanis kubigi dahil) ---");
                var circle = CreateCircle(document, 50, 50, 10);
                Dump(document, circle, tolerances, "circle");
                circle.SubPaths[1].Closed = true;
                Console.WriteLine("[Closed=true atandi]");
                Dump(document, circle, tolerances, "circle+Closed");
            }
            catch (Exception exception)
            {
                Console.WriteLine("[FAIL] Probe istisna: " + exception);
                return 1;
            }
            finally
            {
                if (document != null)
                {
                    try
                    {
                        document.Dirty = false;
                        document.Close();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("UYARI: Belge kapatılamadı: " + exception.Message);
                    }
                }
            }

            return 0;
        }

        private static Curve CreateSquare(Document document, double x0, double y0, double size, bool withExplicitClosingLine)
        {
            var elements = new List<CurveElement>
            {
                Element(document, x0, y0, cdrCurveElementType.cdrElementStart),
                Element(document, x0 + size, y0, cdrCurveElementType.cdrElementLine),
                Element(document, x0 + size, y0 + size, cdrCurveElementType.cdrElementLine),
                Element(document, x0, y0 + size, cdrCurveElementType.cdrElementLine),
            };

            if (withExplicitClosingLine)
            {
                elements.Add(Element(document, x0, y0, cdrCurveElementType.cdrElementLine));
            }

            var array = elements.ToArray();
            return document.CreateCurveFromArray(ref array, array.Length);
        }

        private static Curve CreateCircle(Document document, double cx, double cy, double radius)
        {
            const double kappa = 0.5522847498307936;
            double k = kappa * radius;
            var elements = new List<CurveElement>
            {
                Element(document, cx + radius, cy, cdrCurveElementType.cdrElementStart),
                Element(document, cx + radius, cy + k, cdrCurveElementType.cdrElementControl),
                Element(document, cx + k, cy + radius, cdrCurveElementType.cdrElementControl),
                Element(document, cx, cy + radius, cdrCurveElementType.cdrElementCurve),
                Element(document, cx - k, cy + radius, cdrCurveElementType.cdrElementControl),
                Element(document, cx - radius, cy + k, cdrCurveElementType.cdrElementControl),
                Element(document, cx - radius, cy, cdrCurveElementType.cdrElementCurve),
                Element(document, cx - radius, cy - k, cdrCurveElementType.cdrElementControl),
                Element(document, cx - k, cy - radius, cdrCurveElementType.cdrElementControl),
                Element(document, cx, cy - radius, cdrCurveElementType.cdrElementCurve),
                Element(document, cx + k, cy - radius, cdrCurveElementType.cdrElementControl),
                Element(document, cx + radius, cy - k, cdrCurveElementType.cdrElementControl),
                Element(document, cx + radius, cy, cdrCurveElementType.cdrElementCurve),
            };

            var array = elements.ToArray();
            return document.CreateCurveFromArray(ref array, array.Length);
        }

        private static CurveElement Element(Document document, double x, double y, cdrCurveElementType type)
        {
            return new CurveElement
            {
                PositionX = x,
                PositionY = y,
                ElementType = type,
                NodeType = cdrNodeType.cdrCuspNode,
                Flags = 0,
            };
        }

        private static void Dump(Document document, Curve curve, GeometryTolerances tolerances, string label)
        {
            Console.WriteLine("[" + label + "] subpaths=" + curve.SubPaths.Count);
            for (int sp = 1; sp <= curve.SubPaths.Count; sp++)
            {
                var subPath = curve.SubPaths[sp];
                Console.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "  subpath {0}: Closed={1} Nodes={2} Segments={3} Start=({4:0.###},{5:0.###}) End=({6:0.###},{7:0.###})",
                    sp,
                    subPath.Closed,
                    subPath.Nodes.Count,
                    subPath.Segments.Count,
                    CorelUnits.ToMillimeters(document, subPath.StartNode.PositionX),
                    CorelUnits.ToMillimeters(document, subPath.StartNode.PositionY),
                    CorelUnits.ToMillimeters(document, subPath.EndNode.PositionX),
                    CorelUnits.ToMillimeters(document, subPath.EndNode.PositionY)));

                for (int s = 1; s <= subPath.Segments.Count; s++)
                {
                    var segment = subPath.Segments[s];
                    string line = string.Format(
                        CultureInfo.InvariantCulture,
                        "    seg {0}: {1} ({2:0.###},{3:0.###}) -> ({4:0.###},{5:0.###})",
                        s,
                        segment.Type,
                        CorelUnits.ToMillimeters(document, segment.StartNode.PositionX),
                        CorelUnits.ToMillimeters(document, segment.StartNode.PositionY),
                        CorelUnits.ToMillimeters(document, segment.EndNode.PositionX),
                        CorelUnits.ToMillimeters(document, segment.EndNode.PositionY));

                    if (segment.Type == cdrSegmentType.cdrCurveSegment)
                    {
                        line += string.Format(
                            CultureInfo.InvariantCulture,
                            " c1=({0:0.###},{1:0.###}) c2=({2:0.###},{3:0.###})",
                            CorelUnits.ToMillimeters(document, segment.StartingControlPointX),
                            CorelUnits.ToMillimeters(document, segment.StartingControlPointY),
                            CorelUnits.ToMillimeters(document, segment.EndingControlPointX),
                            CorelUnits.ToMillimeters(document, segment.EndingControlPointY));
                    }

                    Console.WriteLine(line);
                }
            }

            var read = CorelCurveReader.ReadCurve(document, curve, 1, tolerances);
            Console.WriteLine("  reader: contours=" + read.Count +
                " closed=" + read.Contours[0].IsClosed +
                " segments=" + read.Contours[0].SegmentCount);
        }
    }
}
