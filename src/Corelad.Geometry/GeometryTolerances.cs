using System;

namespace Corelad.Geometry
{
    /// <summary>
    /// Tek tolerans yapılandırması (ADR-0005). Sihirli sayı kullanılmaz; analiz
    /// parametreleri burada adlandırılır. Değerler mm cinsindendir.
    /// </summary>
    public sealed class GeometryTolerances
    {
        /// <summary>Nokta/uzunluk karşılaştırma epsilonu (mm).</summary>
        public double ComparisonEpsilonMm { get; set; } = 1e-4;

        /// <summary>Bu bant içinde kalan containment sonucu belirsiz sayılır (mm).</summary>
        public double ContainmentBoundaryBandMm { get; set; } = 1e-3;

        /// <summary>Bezier düzleştirme toleransı (mm). Analiz kopyası içindir.</summary>
        public double FlatteningToleranceMm { get; set; } = 0.05;

        /// <summary>Açı toleransı (rad).</summary>
        public double AngleToleranceRad { get; set; } = 1e-6;

        /// <summary>Alan karşılaştırmasında göreli tolerans.</summary>
        public double RelativeAreaTolerance { get; set; } = 1e-6;

        /// <summary>Bunun altındaki alan dejenere kabul edilir (mm²).</summary>
        public double DegenerateAreaEpsilonMm2 { get; set; } = 1e-6;

        /// <summary>Özyinelemeli düzleştirmede maksimum derinlik.</summary>
        public int MaxFlatteningDepth { get; set; } = 24;

        /// <summary>Ayrık parça adayı: küçük konturun alan oranı üst sınırı.</summary>
        public double DetachedMaxAreaRatio { get; set; } = 0.35;

        /// <summary>Ayrık parça adayı: küçük konturun yükseklik oranı üst sınırı.</summary>
        public double DetachedMaxHeightRatio { get; set; } = 0.55;

        /// <summary>Ayrık parça adayı: yatay örtüşme oranı alt sınırı.</summary>
        public double DetachedMinHorizontalOverlapRatio { get; set; } = 0.30;

        /// <summary>Ayrık parça adayı: dikey boşluk / büyük kontur yüksekliği üst sınırı.</summary>
        public double DetachedMaxGapRatio { get; set; } = 0.75;

        public static GeometryTolerances Default => new GeometryTolerances();

        public void Validate()
        {
            RequirePositive(ComparisonEpsilonMm, nameof(ComparisonEpsilonMm));
            RequirePositive(ContainmentBoundaryBandMm, nameof(ContainmentBoundaryBandMm));
            RequirePositive(FlatteningToleranceMm, nameof(FlatteningToleranceMm));
            RequirePositive(AngleToleranceRad, nameof(AngleToleranceRad));
            RequireNonNegative(RelativeAreaTolerance, nameof(RelativeAreaTolerance));
            RequireNonNegative(DegenerateAreaEpsilonMm2, nameof(DegenerateAreaEpsilonMm2));

            if (MaxFlatteningDepth < 1 || MaxFlatteningDepth > 64)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxFlatteningDepth), "1..64 aralığında olmalıdır.");
            }

            RequireRatio(DetachedMaxAreaRatio, nameof(DetachedMaxAreaRatio));
            RequireRatio(DetachedMaxHeightRatio, nameof(DetachedMaxHeightRatio));
            RequireRatio(DetachedMinHorizontalOverlapRatio, nameof(DetachedMinHorizontalOverlapRatio));
            RequireRatio(DetachedMaxGapRatio, nameof(DetachedMaxGapRatio));
        }

        private static void RequirePositive(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0d)
            {
                throw new ArgumentOutOfRangeException(name, "Pozitif ve sonlu olmalıdır.");
            }
        }

        private static void RequireNonNegative(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d)
            {
                throw new ArgumentOutOfRangeException(name, "Negatif olmayan ve sonlu olmalıdır.");
            }
        }

        private static void RequireRatio(double value, string name)
        {
            if (double.IsNaN(value) || value < 0d || value > 1d)
            {
                throw new ArgumentOutOfRangeException(name, "0..1 aralığında olmalıdır.");
            }
        }
    }
}
