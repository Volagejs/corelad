namespace Corelad.Geometry.Diagnostics
{
    /// <summary>
    /// Sessizce çözülmeyen geometri durumları. Kodlar kullanıcıya raporlanır;
    /// otomatik geometri onarımı yapılmaz.
    /// </summary>
    public enum ProblemCode
    {
        /// <summary>Kontur kapalı değil.</summary>
        OpenContour = 0,

        /// <summary>Kontur kendini kesiyor.</summary>
        SelfIntersection = 1,

        /// <summary>İki kontur teğet veya tolerans bandında temas ediyor.</summary>
        TangentialContact = 2,

        /// <summary>Birden fazla olası ebeveyn; otomatik seçim yapılmadı.</summary>
        AmbiguousParent = 3,

        /// <summary>Kontur sınırları kesişiyor; containment belirsiz.</summary>
        CrossingContours = 4,

        /// <summary>Başka bir gövdeye geometrik olarak bağlı olmayan ayrık parça adayı.</summary>
        DetachedComponent = 6,

        /// <summary>Güvenilir iç nokta bulunamadı (çok ince/dejenere kontur).</summary>
        NoInteriorPoint = 7,

        /// <summary>Sıfır alan / tekrar eden nokta.</summary>
        DegenerateContour = 8,

        /// <summary>Düzleştirme derinlik sınırına ulaştı; analiz yaklaşık.</summary>
        TooComplex = 9,

        /// <summary>Delik içinde ayrı gövde (ada): ayrı parça olarak değerlendirildi.</summary>
        NestedIsland = 10,

        /// <summary>Grup/PowerClip/bitmap/efekt gibi desteklenmeyen nesne (Dock katmanı üretir).</summary>
        UnsupportedObject = 11,

        /// <summary>Levha dışına taşma (Nesting).</summary>
        OutsideSheet = 12,

        /// <summary>Çakışma (Nesting).</summary>
        Collision = 13,

        /// <summary>CorelDRAW'a yazma öncesi round-trip doğrulaması başarısız (Dock katmanı üretir).</summary>
        WriteValidationFailed = 14,
    }
}
