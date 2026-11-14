using System;
using System.Globalization;
using System.Linq;
using Corelad.Dock.Corel;
using Corelad.Geometry;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;
using Corel.Interop.VGCore;

namespace Corelad.SmokeTest
{
    /// <summary>
    /// P1 smoke senaryosu (docs/P1-integration-plan.md §5). Yeni bir belge açar,
    /// yalnızca onu kapatır; mevcut belgelere ve dosyalara dokunmaz, hiçbir şey kaydetmez.
    /// </summary>
    internal static class Runner
    {
        private static int _failures;

        public static int Run()
        {
            var tolerances = new GeometryTolerances();
            tolerances.Validate();

            Console.WriteLine("=== Corelad P1 smoke (CorelDRAW) ===");

            var application = CorelApplicationFactory.Create();
            Console.WriteLine("CorelDRAW sürümü: " + application.Version);
            Console.WriteLine("Platform: " + application.PlatformVersionMajor + "." + application.PlatformVersionMinor);
            try
            {
                application.Visible = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine("UYARI: Application.Visible ayarlanamadı: " + exception.Message);
            }

            Document document = null;
            try
            {
                document = application.CreateDocument();
                Check(document != null, "Yeni belge oluşturuldu");
                if (document == null) return Finish();

                document.Unit = cdrUnit.cdrMillimeter;

                int initialShapeCount = document.ActivePage.Shapes.Count;
                Console.WriteLine("Başlangıç şekil sayısı: " + initialShapeCount);

                // 1) Kaynak geometri (GDI+ Arial) hazırla.
                var sourceContours = FontOutlineSource.FromText("MUSTAFA");
                Check(sourceContours.Count == 9,
                    "GDI+ MUSTAFA kontur sayısı 9", sourceContours.Count);

                // 2) Kaynağı CorelDRAW'a yaz (bu da yeni nesne; orijinal dosya yok).
                var sourceCurve = CorelCurveWriter.BuildCurve(
                    document, sourceContours.Contours, tolerances);
                var sourceShape = document.ActiveLayer.CreateCurve(sourceCurve);
                Check(sourceShape != null, "Kaynak eğri belgeye yazıldı");
                if (sourceShape == null) return Finish();

                Check(sourceShape.Type == cdrShapeType.cdrCurveShape,
                    "Kaynak şekil cdrCurveShape", sourceShape.Type);

                int afterSourceCount = document.ActivePage.Shapes.Count;
                Check(afterSourceCount == initialShapeCount + 1,
                    "Kaynak yazıldıktan sonra şekil sayısı +1", afterSourceCount);

                // 3) Round-trip: geri oku, kontrol noktalarını karşılaştır.
                var readBack = CorelCurveReader.Read(document, sourceShape, 1, tolerances);
                Check(ContoursMatch(sourceContours, readBack, CorelTolerances.RoundTripMm),
                    "Round-trip geometri eşleşmesi (segment + kontrol noktaları)");
                Check(readBack.Count == 9, "Geri okunan subpath sayısı 9", readBack.Count);

                // 4) Kaynak imzası (değişmediğini kanıtlamak için).
                string sourceSignature = CorelShapeSnapshot.Capture(document, sourceShape, tolerances);

                // 5) Smart Divide.
                var service = new SmartDivideService(tolerances);
                var result = service.Run(document, sourceShape);

                Console.WriteLine();
                Console.WriteLine("--- Analiz ---");
                if (result.Analysis != null)
                {
                    Console.WriteLine(result.Analysis.Describe());
                }

                Console.WriteLine("--- Sonuç ---");
                Console.WriteLine(result.Message);
                Console.WriteLine();

                Check(result.Applied, "Smart Divide uygulandı (yeni nesneler yazıldı)");
                Check(result.Analysis != null, "Analiz sonucu mevcut");
                if (result.Analysis == null) return Finish();

                Check(result.Analysis.Candidates.Count == 7,
                    "Mantıksal harf adayı sayısı 7", result.Analysis.Candidates.Count);
                Check(result.Analysis.Candidates.Sum(c => c.HoleIndices.Count) == 2,
                    "Toplam delik sayısı 2", result.Analysis.Candidates.Sum(c => c.HoleIndices.Count));
                Check(result.Analysis.CountProblems(ProblemSeverity.Error) == 0,
                    "Error sayısı 0", result.Analysis.CountProblems(ProblemSeverity.Error));
                Check(result.CreatedShapeCount == 7,
                    "Yazılan yeni şekil sayısı 7", result.CreatedShapeCount);
                Check(result.CreatedShapes.All(s => s.Type == cdrShapeType.cdrCurveShape),
                    "Yazılan tüm şekiller cdrCurveShape");

                int afterRunCount = document.ActivePage.Shapes.Count;
                Check(afterRunCount == afterSourceCount + 7,
                    "Smart Divide sonrası şekil sayısı +7", afterRunCount);

                string afterRunSignature = CorelShapeSnapshot.Capture(document, sourceShape, tolerances);
                Check(string.Equals(sourceSignature, afterRunSignature, StringComparison.Ordinal),
                    "Kaynak geometrisi değişmedi (imza aynı)");

                // 6) Tek undo: yeni 7 nesne gider, kaynak kalır.
                document.Undo(1);
                int afterUndoCount = document.ActivePage.Shapes.Count;
                Check(afterUndoCount == afterSourceCount,
                    "Undo(1) sonrası yalnızca kaynak kaldı", afterUndoCount);

                string afterUndoSignature = CorelShapeSnapshot.Capture(document, sourceShape, tolerances);
                Check(string.Equals(sourceSignature, afterUndoSignature, StringComparison.Ordinal),
                    "Undo sonrası kaynak geometrisi değişmedi");

                // P2: eğri olmayan, desteklenmeyen bir nesne hiçbir şekilde yazılmamalı.
                var unsupported = document.ActiveLayer.CreateRectangle(0, 0, 10, 10, 0, 0, 0, 0);
                Check(unsupported != null, "Desteklenmeyen dikdörtgen nesne oluşturuldu");
                if (unsupported != null)
                {
                    int beforeUnsupportedRun = document.ActivePage.Shapes.Count;
                    var unsupportedResult = service.Run(document, unsupported);
                    int afterUnsupportedRun = document.ActivePage.Shapes.Count;

                    Check(unsupported.Type != cdrShapeType.cdrCurveShape,
                        "Test girdisi cdrCurveShape değil", unsupported.Type);
                    Check(unsupportedResult.Skipped && !unsupportedResult.Applied,
                        "Desteklenmeyen nesne güvenli biçimde atlandı");
                    Check(unsupportedResult.SkipCode == ProblemCode.UnsupportedObject,
                        "Atlama kodu UnsupportedObject", unsupportedResult.SkipCode);
                    Check(beforeUnsupportedRun == afterUnsupportedRun,
                        "Desteklenmeyen nesne çalıştırması şekil eklemedi", afterUnsupportedRun);
                }
            }
            catch (Exception exception)
            {
                _failures++;
                Console.WriteLine("[FAIL] Smoke senaryosu istisna ile durdu:");
                Console.WriteLine(exception);
            }
            finally
            {
                if (document != null)
                {
                    try
                    {
                        // Belge KAYDEDİLMEZ: yalnızca kapatılır.
                        document.Dirty = false;
                        document.Close();
                        Console.WriteLine("Belge kaydedilmeden kapatıldı.");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("UYARI: Belge kapatılamadı: " + exception.Message);
                    }
                }
            }

            return Finish();
        }

        private static bool ContoursMatch(ContourSet expected, ContourSet actual, double toleranceMm)
        {
            if (expected.Count != actual.Count) return false;

            for (int c = 0; c < expected.Count; c++)
            {
                var e = expected.Contours[c];
                var a = actual.Contours[c];
                if (e.IsClosed != a.IsClosed) return false;
                if (e.SegmentCount != a.SegmentCount) return false;

                for (int s = 0; s < e.SegmentCount; s++)
                {
                    var es = e.Segments[s];
                    var asg = a.Segments[s];
                    if (es.Kind != asg.Kind) return false;
                    if (es.P0.DistanceTo(asg.P0) > toleranceMm) return false;
                    if (es.P1.DistanceTo(asg.P1) > toleranceMm) return false;
                    if (es.P2.DistanceTo(asg.P2) > toleranceMm) return false;
                    if (es.P3.DistanceTo(asg.P3) > toleranceMm) return false;
                }
            }

            return true;
        }

        private static void Check(bool condition, string label, object actual = null)
        {
            if (condition)
            {
                Console.WriteLine("PASS  " + label +
                    (actual == null ? string.Empty : "  [" + Convert.ToString(actual, CultureInfo.InvariantCulture) + "]"));
                return;
            }

            _failures++;
            Console.WriteLine("FAIL  " + label +
                (actual == null ? string.Empty : "  [" + Convert.ToString(actual, CultureInfo.InvariantCulture) + "]"));
        }

        private static int Finish()
        {
            Console.WriteLine();
            Console.WriteLine(_failures == 0
                ? "SMOKE SONUCU: PASS"
                : "SMOKE SONUCU: FAIL (" + _failures + " kontrol)");
            return _failures == 0 ? 0 : 1;
        }
    }
}
