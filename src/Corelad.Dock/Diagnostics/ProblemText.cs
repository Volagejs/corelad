using Corelad.Geometry.Diagnostics;

namespace Corelad.Dock.Diagnostics
{
    /// <summary>
    /// <see cref="ProblemCode"/> → Türkçe başlık/açıklama ve <see cref="ProblemSeverity"/> →
    /// görsel etiket eşlemesi. Yalnızca sunum metni üretir; geometri kararı vermez (AGENTS.md §2).
    ///
    /// Metin kuralı: "ne oldu + neden + ne yapmalı" (ui-ux-desktop §5).
    /// Renk tek başına anlam taşımaz; her önem derecesi ayrıca sözcük ve simge ile işaretlenir.
    /// </summary>
    public static class ProblemText
    {
        /// <summary>Kullanıcıya gösterilen kısa önem etiketi.</summary>
        public static string SeverityLabel(ProblemSeverity severity)
        {
            switch (severity)
            {
                case ProblemSeverity.Error:
                    return "Hata";
                case ProblemSeverity.Warning:
                    return "Uyarı";
                default:
                    return "Bilgi";
            }
        }

        /// <summary>Renk körlüğüne dayanıklı, metin tabanlı görsel işaret.</summary>
        public static string SeverityGlyph(ProblemSeverity severity)
        {
            switch (severity)
            {
                case ProblemSeverity.Error:
                    return "×";
                case ProblemSeverity.Warning:
                    return "!";
                default:
                    return "i";
            }
        }

        /// <summary>Sorun kodunun kısa Türkçe başlığı.</summary>
        public static string Title(ProblemCode code)
        {
            switch (code)
            {
                case ProblemCode.OpenContour:
                    return "Kontur kapalı değil";
                case ProblemCode.SelfIntersection:
                    return "Kontur kendini kesiyor";
                case ProblemCode.TangentialContact:
                    return "Kontur teğet temas ediyor";
                case ProblemCode.AmbiguousParent:
                    return "Birden fazla olası ebeveyn";
                case ProblemCode.CrossingContours:
                    return "Konturlar kesişiyor";
                case ProblemCode.DetachedComponent:
                    return "Ayrık parça adayı";
                case ProblemCode.NoInteriorPoint:
                    return "Güvenilir iç nokta yok";
                case ProblemCode.DegenerateContour:
                    return "Dejenere kontur";
                case ProblemCode.TooComplex:
                    return "Kontur çok karmaşık";
                case ProblemCode.NestedIsland:
                    return "Delik içinde ayrı gövde";
                case ProblemCode.UnsupportedObject:
                    return "Desteklenmeyen nesne";
                case ProblemCode.OutsideSheet:
                    return "Levha dışına taşma";
                case ProblemCode.Collision:
                    return "Parçalar çakışıyor";
                case ProblemCode.WriteValidationFailed:
                    return "Yazma doğrulaması başarısız";
                default:
                    return "Bilinmeyen sorun";
            }
        }

        /// <summary>Sorun kodunun Türkçe açıklaması ve önerilen kullanıcı eylemi.</summary>
        public static string Description(ProblemCode code)
        {
            switch (code)
            {
                case ProblemCode.OpenContour:
                    return "Kontur zinciri başladığı noktaya dönmüyor. Açık kontur harf gövdesi olarak " +
                           "değerlendirilemez. Çizimi kapatın veya bu harfi atlayın; otomatik onarım yapılmaz.";
                case ProblemCode.SelfIntersection:
                    return "Kontur kendi üzerinden geçiyor; kapalı alan belirsiz. Kesişen düğümleri " +
                           "temizleyin. Geometri sessizce düzeltilmez.";
                case ProblemCode.TangentialContact:
                    return "İki kontur teğet veya tolerans bandında temas ediyor; hangi gövdeye ait oldukları " +
                           "belirsiz. Konturları birbirinden ayırın veya sonucu elle doğrulayın.";
                case ProblemCode.AmbiguousParent:
                    return "Kontur birden fazla gövdenin içinde kalıyor; otomatik ebeveyn seçilmedi. " +
                           "Doğru harfi elle belirleyin ya da iç içe geçmeyi kaldırın.";
                case ProblemCode.CrossingContours:
                    return "Kontur sınırları birbirini kesiyor; iç/dış ilişkisi güvenilir değil. " +
                           "Kesişimi kaldırın ve analizi yeniden çalıştırın.";
                case ProblemCode.DetachedComponent:
                    return "Küçük parça gövdeye geometrik olarak bağlı değil (ör. İ noktası veya ayrık " +
                           "vurgu). Otomatik birleştirilmedi; birleştirme için kullanıcı onayı gerekir.";
                case ProblemCode.NoInteriorPoint:
                    return "Kontur çok ince veya dejenere; güvenilir bir iç nokta bulunamadı. Konturu " +
                           "kalınlaştırın ya da gereksiz düğümleri sadeleştirin.";
                case ProblemCode.DegenerateContour:
                    return "Kontur sıfır alana sahip veya tekrar eden noktalar içeriyor. Boş konturu " +
                           "silin; analiz bu konturu gövde olarak kullanmaz.";
                case ProblemCode.TooComplex:
                    return "Düzleştirme derinlik sınırına ulaşıldı; sonuç yaklaşıktır. Düğüm sayısını " +
                           "azaltın veya konturu basitleştirin.";
                case ProblemCode.NestedIsland:
                    return "Başka bir gövdenin deliği içinde ayrı bir gövde var; ayrı parça olarak " +
                           "değerlendirildi. Doğruysa onaylayın, değilse geometriyi düzeltin.";
                case ProblemCode.UnsupportedObject:
                    return "Seçili nesne eğri değil (grup, PowerClip, bitmap veya efekt). Yalnızca tek bir " +
                           "eğri nesnesi desteklenir. Grubu çözün veya nesneyi eğriye dönüştürün.";
                case ProblemCode.OutsideSheet:
                    return "Parça levha sınırının dışına çıkıyor. Levha ölçüsünü veya yerleşimi düzeltin; " +
                           "sınır ihlali olan yerleşim uygulanmaz.";
                case ProblemCode.Collision:
                    return "İki parça üst üste geliyor. Yerleşimi yeniden deneyin veya parçalar arası " +
                           "güvenli mesafeyi artırın; çakışan yerleşim uygulanmaz.";
                case ProblemCode.WriteValidationFailed:
                    return "CorelDRAW'a yazmadan önceki round-trip doğrulaması geçmedi; hiçbir nesne " +
                           "eklenmedi. Ayrıntıyı raporlayın ve çizimi yeniden deneyin.";
                default:
                    return "Tanımlanmamış sorun kodu. Ayrıntıyı geliştiriciye bildirin; geometri " +
                           "otomatik onarılmaz.";
            }
        }
    }
}
