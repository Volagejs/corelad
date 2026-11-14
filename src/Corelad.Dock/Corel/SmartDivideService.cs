using System;
using System.Collections.Generic;
using System.Globalization;
using Corelad.Geometry;
using Corelad.Geometry.Analysis;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;
using Corel.Interop.VGCore;

namespace Corelad.Dock.Corel
{
    /// <summary>
    /// Smart Divide dikey dilimi: seçili eğriyi oku → saf çekirdekte analiz et →
    /// her mantıksal harf adayını YENİ şekil olarak yaz → tek undo grubu.
    ///
    /// Kaynak şekil değiştirilmez, silinmez, gizlenmez; belge kaydedilmez (ADR-0006).
    /// Geometri algoritması burada yoktur (AGENTS.md §2).
    /// </summary>
    public sealed class SmartDivideService
    {
        private readonly ContainmentAnalyzer _analyzer;

        public SmartDivideService(GeometryTolerances tolerances = null)
        {
            Tolerances = tolerances ?? GeometryTolerances.Default;
            Tolerances.Validate();
            _analyzer = new ContainmentAnalyzer(Tolerances);
        }

        public GeometryTolerances Tolerances { get; }

        public SmartDivideRunResult Run(Document document, Shape sourceShape)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (sourceShape == null) throw new ArgumentNullException(nameof(sourceShape));

            if (sourceShape.Type != cdrShapeType.cdrCurveShape)
            {
                return SmartDivideRunResult.SkippedFor(
                    null,
                    ProblemCode.UnsupportedObject,
                    string.Format(CultureInfo.InvariantCulture,
                        "Seçili nesne eğri değil (Type = {0}). Yalnızca tek bir cdrCurveShape desteklenir; " +
                        "grup/PowerClip/bitmap/efekt otomatik açılmaz.",
                        sourceShape.Type));
            }

            var contours = CorelCurveReader.Read(document, sourceShape, 1, Tolerances);
            var analysis = _analyzer.Analyze(contours);

            if (analysis.HasErrors)
            {
                return SmartDivideRunResult.SkippedFor(
                    analysis,
                    FirstErrorCode(analysis),
                    string.Format(CultureInfo.InvariantCulture,
                        "Analiz {0} hata üretti; hiçbir nesne yazılmadı. Sorunlar kullanıcıya raporlanmalı.",
                        analysis.CountProblems(ProblemSeverity.Error)));
            }

            if (!analysis.CanApply)
            {
                return SmartDivideRunResult.SkippedFor(
                    analysis,
                    null,
                    "Analiz uygulanabilir aday üretmedi; hiçbir nesne yazılmadı.");
            }

            // 1) Tüm eğrileri detached olarak kur ve round-trip doğrula.
            //    Doğrulama başarısızsa belgeye HİÇBİR şey eklenmez.
            var pending = new List<Curve>(analysis.Candidates.Count);
            try
            {
                for (int i = 0; i < analysis.Candidates.Count; i++)
                {
                    var candidate = analysis.Candidates[i];
                    var selected = SelectCandidateContours(analysis.Source, candidate);
                    pending.Add(CorelCurveWriter.BuildCurve(document, selected, Tolerances));
                }
            }
            catch (Exception exception)
            {
                return SmartDivideRunResult.SkippedFor(
                    analysis,
                    ProblemCode.WriteValidationFailed,
                    "Geometri yazma doğrulaması başarısız; hiçbir nesne eklenmedi. " + exception.Message);
            }

            // 2) Tek undo grubunda yaz.
            var created = new List<Shape>(pending.Count);
            document.BeginCommandGroup("Corelad Smart Divide");
            try
            {
                var layer = document.ActiveLayer;
                for (int i = 0; i < pending.Count; i++)
                {
                    var shape = layer.CreateCurve(pending[i]);
                    if (shape == null)
                    {
                        throw new InvalidOperationException(
                            string.Format(CultureInfo.InvariantCulture,
                                "Layer.CreateCurve null döndü (aday {0}).", i));
                    }

                    created.Add(shape);
                }

                document.EndCommandGroup();
            }
            catch
            {
                // Yarım kalan grubu tek undo ile geri al; kaynak yine dokunulmaz.
                try
                {
                    document.Undo(1);
                }
                catch
                {
                    // Undo başarısızsa özgün istisna korunur.
                }

                throw;
            }

            return SmartDivideRunResult.Written(analysis, created);
        }

        private static ProblemCode? FirstErrorCode(SmartDivideAnalysis analysis)
        {
            for (int i = 0; i < analysis.Problems.Count; i++)
            {
                if (analysis.Problems[i].Severity == ProblemSeverity.Error)
                {
                    return analysis.Problems[i].Code;
                }
            }

            return null;
        }

        private static IReadOnlyList<Contour> SelectCandidateContours(ContourSet source, LetterCandidate candidate)
        {
            var selected = new List<Contour>(1 + candidate.HoleIndices.Count)
            {
                source.Contours[candidate.OuterIndex],
            };

            for (int i = 0; i < candidate.HoleIndices.Count; i++)
            {
                selected.Add(source.Contours[candidate.HoleIndices[i]]);
            }

            return selected;
        }
    }
}
