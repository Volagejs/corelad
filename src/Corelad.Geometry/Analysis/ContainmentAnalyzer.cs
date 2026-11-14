using System;
using System.Collections.Generic;
using System.Globalization;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;

namespace Corelad.Geometry.Analysis
{
    /// <summary>
    /// Smart Divide'ın geometri çekirdeği: her dış konturu, containment ilişkisiyle ona ait
    /// iç konturlarla birlikte mantıksal harf adayı olarak sınıflandırır.
    ///
    /// Kurallar:
    /// - Rol (Outer/Hole) yön ile değil, gerçek nokta-içinde testiyle belirlenir.
    /// - Belirsiz geometri <see cref="Problem"/> olarak raporlanır; sessiz onarım yok.
    /// - Ayrık parçalar (İ noktası, j noktası, Ç/Ğ işaretleri) asla otomatik bağlanmaz.
    /// - Çıktı adayları orijinal <see cref="Contour"/> nesnelerine indeksle bağlıdır;
    ///   böylece Dock katmanı bezier geometrisini aynen yazabilir.
    /// </summary>
    public sealed class ContainmentAnalyzer
    {
        private readonly GeometryTolerances _tolerances;

        public ContainmentAnalyzer(GeometryTolerances? tolerances = null)
        {
            _tolerances = tolerances ?? GeometryTolerances.Default;
            _tolerances.Validate();
        }

        public GeometryTolerances Tolerances => _tolerances;

        public SmartDivideAnalysis Analyze(ContourSet contourSet)
        {
            if (contourSet == null) throw new ArgumentNullException(nameof(contourSet));

            int count = contourSet.Contours.Count;
            var flat = new FlattenedContour[count];
            var interior = new Point2[count];
            var hasInterior = new bool[count];
            var excluded = new bool[count];
            var problems = new List<Problem>();
            var perContour = new List<Problem>[count];
            for (int i = 0; i < count; i++) perContour[i] = new List<Problem>();

            double epsilon = _tolerances.ComparisonEpsilonMm;
            double band = _tolerances.ContainmentBoundaryBandMm;

            // 1) Düzleştirme + geometrik doğrulama.
            for (int i = 0; i < count; i++)
            {
                var contour = contourSet.Contours[i];
                flat[i] = BezierFlattener.Flatten(contour, i, _tolerances);

                if (!contour.IsClosed)
                {
                    Report(contourSet, problems, perContour, i, null, ProblemCode.OpenContour, ProblemSeverity.Error,
                        "Kontur kapalı değil; kapalı olmayan kontur otomatik kapatılmaz.");
                    excluded[i] = true;
                    continue;
                }

                if (flat[i].HasBrokenChain)
                {
                    Report(contourSet, problems, perContour, i, null, ProblemCode.OpenContour, ProblemSeverity.Error,
                        "Segment zinciri kopuk (ardışık segmentler birbirine bağlı değil); otomatik onarım yapılmaz.");
                    excluded[i] = true;
                    continue;
                }

                if (flat[i].Points.Count < 3 || flat[i].AreaMm2 <= _tolerances.DegenerateAreaEpsilonMm2)
                {
                    Report(contourSet, problems, perContour, i, null, ProblemCode.DegenerateContour, ProblemSeverity.Error,
                        "Kontur dejenere (üçten az nokta veya sıfıra yakın alan).");
                    excluded[i] = true;
                    continue;
                }

                if (flat[i].HitFlatteningDepthLimit)
                {
                    Report(contourSet, problems, perContour, i, null, ProblemCode.TooComplex, ProblemSeverity.Warning,
                        "Bezier düzleştirme derinlik sınırına ulaştı; analiz yaklaşıktır, geometri değiştirilmedi.");
                }

                if (Polygon2.HasSelfIntersection(flat[i].Points, epsilon, out int edgeA, out int edgeB))
                {
                    Report(contourSet, problems, perContour, i, null, ProblemCode.SelfIntersection, ProblemSeverity.Error,
                        string.Format(CultureInfo.InvariantCulture,
                            "Kontur kendini kesiyor (segment {0} ve {1}); otomatik onarım yapılmaz.", edgeA, edgeB));
                    excluded[i] = true;
                    continue;
                }

                if (!Polygon2.TryFindInteriorPoint(flat[i].Points, flat[i].Bounds, band, out interior[i]))
                {
                    Report(contourSet, problems, perContour, i, null, ProblemCode.NoInteriorPoint, ProblemSeverity.Error,
                        "Kontur içinde güvenilir bir iç nokta bulunamadı (çok ince veya dejenere).");
                    excluded[i] = true;
                    continue;
                }

                hasInterior[i] = true;
            }

            // 2) Sınır ilişkileri: kesişme, teğetlik, containment.
            var inside = new bool[count, count];
            var crossingA = new List<int>();
            var crossingB = new List<int>();
            for (int i = 0; i < count; i++)
            {
                if (excluded[i]) continue;

                for (int j = i + 1; j < count; j++)
                {
                    if (excluded[j]) continue;
                    if (flat[i].Bounds.DistanceTo(flat[j].Bounds) > band) continue;

                    var contact = Polygon2.ClassifyBoundaryContact(flat[i].Points, flat[j].Points, band, out _);
                    if (contact == SegmentContact.Cross)
                    {
                        crossingA.Add(i);
                        crossingB.Add(j);
                        continue;
                    }

                    if (contact == SegmentContact.Touch)
                    {
                        Report(contourSet, problems, perContour, i, j, ProblemCode.TangentialContact, ProblemSeverity.Warning,
                            string.Format(CultureInfo.InvariantCulture,
                                "Konturlar teğet veya {0} mm tolerans bandında temas ediyor; ilişki belirsiz.", band));
                        continue;
                    }

                    // Sınır teması yoksa, bir konturun sınır noktası diğerinin kesinlikle
                    // içinde veya dışındadır. Ağırlık merkezi delik içine düşebileceği için
                    // temsilî nokta olarak konturun kendi sınır noktası kullanılır.
                    var locationJ = Polygon2.Locate(flat[i].Points, flat[j].Points[0], band);
                    var locationI = Polygon2.Locate(flat[j].Points, flat[i].Points[0], band);

                    if (locationJ == PointLocation.Boundary || locationI == PointLocation.Boundary)
                    {
                        Report(contourSet, problems, perContour, i, j, ProblemCode.TangentialContact, ProblemSeverity.Warning,
                            string.Format(CultureInfo.InvariantCulture,
                                "Temsilî iç nokta karşı konturun {0} mm sınır bandında; containment belirsiz.", band));
                        continue;
                    }

                    bool jInsideI = locationJ == PointLocation.Inside;
                    bool iInsideJ = locationI == PointLocation.Inside;

                    if (jInsideI && iInsideJ)
                    {
                        Report(contourSet, problems, perContour, i, j, ProblemCode.AmbiguousParent, ProblemSeverity.Error,
                            "İki kontur birbirini kapsıyor gibi görünüyor; containment belirsiz.");
                        continue;
                    }

                    inside[j, i] = jInsideI;
                    inside[i, j] = iInsideJ;
                }
            }

            // 3) Ebeveyn listeleri ve zincir tutarlılığı.
            var containers = new List<int>[count];
            for (int i = 0; i < count; i++) containers[i] = new List<int>();

            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int j = 0; j < count; j++)
                {
                    containers[j].Clear();
                    if (excluded[j]) continue;
                    for (int i = 0; i < count; i++)
                    {
                        if (i == j || excluded[i]) continue;
                        if (!inside[j, i]) continue;
                        if (!hasInterior[i]) continue;
                        containers[j].Add(i);
                    }
                }

                for (int j = 0; j < count; j++)
                {
                    if (excluded[j] || containers[j].Count < 2) continue;

                    containers[j].Sort((a, b) => flat[a].AreaMm2.CompareTo(flat[b].AreaMm2));
                    for (int k = 0; k + 1 < containers[j].Count; k++)
                    {
                        if (inside[containers[j][k], containers[j][k + 1]]) continue;
                        Report(contourSet, problems, perContour, j, containers[j][k], ProblemCode.AmbiguousParent, ProblemSeverity.Error,
                            "Kontur birden fazla iç içe olmayan ebeveyn tarafından kapsanıyor; ebeveyn otomatik seçilmedi.");
                        excluded[j] = true;
                        changed = true;
                        break;
                    }
                }
            }

            // 4) Derinlik, ebeveyn, çocuklar.
            var depth = new int[count];
            var parent = new int?[count];
            var children = new List<int>[count];
            for (int i = 0; i < count; i++) children[i] = new List<int>();

            for (int j = 0; j < count; j++)
            {
                if (excluded[j]) continue;
                depth[j] = containers[j].Count;
                if (containers[j].Count > 0)
                {
                    int smallest = containers[j][0];
                    for (int k = 1; k < containers[j].Count; k++)
                    {
                        if (flat[containers[j][k]].AreaMm2 < flat[smallest].AreaMm2) smallest = containers[j][k];
                    }

                    parent[j] = smallest;
                    children[smallest].Add(j);
                }
            }

            // 4.5) Sınır kesişmelerini raporla.
            for (int c = 0; c < crossingA.Count; c++)
            {
                int a = crossingA[c];
                int b = crossingB[c];
                Report(contourSet, problems, perContour, a, b,
                    ProblemCode.CrossingContours,
                    ProblemSeverity.Error,
                    string.Format(CultureInfo.InvariantCulture,
                        "Kontur sınırları kesişiyor (kontur {0} ve {1}); containment belirsiz, otomatik birleştirme yapılmaz.",
                        a, b));
            }

            // 5) Delik içindeki ayrı gövde (ada) bilgisi.
            for (int i = 0; i < count; i++)
            {
                if (excluded[i]) continue;
                if (depth[i] >= 2 && depth[i] % 2 == 0)
                {
                    Report(contourSet, problems, perContour, i, parent[i], ProblemCode.NestedIsland, ProblemSeverity.Warning,
                        "Delik içinde ayrı gövde (ada); ana gövdeye bağlı değil, ayrı parça olarak değerlendirildi.");
                }
            }

            // 6) Ayrık parça adayları: bağlama YOK, yalnızca rapor.
            DetectDetachedComponents(contourSet, flat, depth, excluded, hasInterior, problems, perContour);

            // 7) Sınıflandırma tablosu.
            var classifications = new ContourClassification[count];
            for (int i = 0; i < count; i++)
            {
                ContourRole role;
                if (excluded[i]) role = ContourRole.Unknown;
                else role = depth[i] % 2 == 0 ? ContourRole.Outer : ContourRole.Hole;

                classifications[i] = new ContourClassification(
                    i,
                    parent[i],
                    children[i].AsReadOnly(),
                    depth[i],
                    role,
                    hasInterior[i] ? interior[i] : Point2.Origin,
                    flat[i].AreaMm2,
                    flat[i].Bounds,
                    excluded[i],
                    perContour[i].AsReadOnly());
            }

            // 8) Mantıksal harf adayları: çift derinlikteki her gövde + doğrudan delikleri.
            var candidates = new List<LetterCandidate>();
            int candidateId = 0;
            var seenProblems = new HashSet<Problem>();

            for (int i = 0; i < count; i++)
            {
                if (excluded[i]) continue;
                if (depth[i] % 2 != 0) continue;

                var holes = new List<int>();
                for (int k = 0; k < children[i].Count; k++)
                {
                    int child = children[i][k];
                    if (!excluded[child] && depth[child] == depth[i] + 1)
                    {
                        holes.Add(child);
                    }
                }

                holes.Sort();
                var candidateProblems = new List<Problem>();
                seenProblems.Clear();
                AddProblems(candidateProblems, seenProblems, perContour[i]);
                for (int h = 0; h < holes.Count; h++)
                {
                    AddProblems(candidateProblems, seenProblems, perContour[holes[h]]);
                }

                candidates.Add(new LetterCandidate(
                    candidateId++,
                    i,
                    holes.AsReadOnly(),
                    depth[i],
                    flat[i].AreaMm2,
                    flat[i].Bounds,
                    candidateProblems.AsReadOnly()));
            }

            return new SmartDivideAnalysis(contourSet, classifications, candidates.AsReadOnly(), problems);
        }

        private void DetectDetachedComponents(
            ContourSet contourSet,
            FlattenedContour[] flat,
            int[] depth,
            bool[] excluded,
            bool[] hasInterior,
            List<Problem> problems,
            List<Problem>[] perContour)
        {
            double band = _tolerances.ContainmentBoundaryBandMm;
            int count = flat.Length;

            for (int small = 0; small < count; small++)
            {
                if (excluded[small] || !hasInterior[small] || depth[small] != 0) continue;

                var smallBounds = flat[small].Bounds;
                var smallArea = flat[small].AreaMm2;
                if (smallArea <= _tolerances.DegenerateAreaEpsilonMm2) continue;

                int bestPartner = -1;
                double bestGap = double.PositiveInfinity;

                for (int large = 0; large < count; large++)
                {
                    if (large == small || excluded[large] || !hasInterior[large] || depth[large] != 0) continue;

                    var largeBounds = flat[large].Bounds;
                    var largeArea = flat[large].AreaMm2;
                    if (smallArea > largeArea * _tolerances.DetachedMaxAreaRatio) continue;
                    if (smallBounds.Height > largeBounds.Height * _tolerances.DetachedMaxHeightRatio) continue;

                    double overlapX = smallBounds.HorizontalOverlap(largeBounds);
                    double minWidth = Math.Min(smallBounds.Width, largeBounds.Width);
                    if (minWidth <= 0d || overlapX < minWidth * _tolerances.DetachedMinHorizontalOverlapRatio) continue;

                    double gap = smallBounds.VerticalGap(largeBounds);
                    if (gap <= band) continue;
                    if (gap > largeBounds.Height * _tolerances.DetachedMaxGapRatio) continue;

                    if (gap < bestGap)
                    {
                        bestGap = gap;
                        bestPartner = large;
                    }
                }

                if (bestPartner < 0) continue;

                Report(contourSet, problems, perContour, small, bestPartner, ProblemCode.DetachedComponent, ProblemSeverity.Warning,
                    string.Format(CultureInfo.InvariantCulture,
                        "Ayrık parça adayı: kontur {0}, kontur {1} gövdesine geometrik olarak bağlı değil, " +
                        "{2:0.###} mm dikey boşluk var. Otomatik birleştirilmedi; kullanıcı onayı gerekir.",
                        small, bestPartner, bestGap));
            }
        }

        private static void AddProblems(List<Problem> target, HashSet<Problem> seen, List<Problem> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (seen.Add(source[i])) target.Add(source[i]);
            }
        }

        private static Problem Report(
            ContourSet contourSet,
            List<Problem> all,
            List<Problem>[] perContour,
            int contourIndex,
            int? relatedContourIndex,
            ProblemCode code,
            ProblemSeverity severity,
            string message)
        {
            var contour = contourSet.Contours[contourIndex];
            int? relatedSubPath = relatedContourIndex.HasValue
                ? (int?)contourSet.Contours[relatedContourIndex.Value].SourceSubPathIndex
                : null;

            var problem = new Problem(
                code,
                severity,
                message,
                contourIndex,
                relatedContourIndex,
                contour.SourceShapeId,
                contour.SourceSubPathIndex,
                relatedSubPath);

            all.Add(problem);
            perContour[contourIndex].Add(problem);
            if (relatedContourIndex.HasValue) perContour[relatedContourIndex.Value].Add(problem);
            return problem;
        }
    }
}
