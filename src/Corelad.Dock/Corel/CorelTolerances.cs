namespace Corelad.Dock.Corel
{
    /// <summary>
    /// COM katmanına özgü toleranslar. Geometri toleransları
    /// <see cref="Corelad.Geometry.GeometryTolerances"/> içindedir; buradakiler yalnızca
    /// CorelDRAW yaz/oku round-trip doğrulaması içindir.
    /// </summary>
    public static class CorelTolerances
    {
        /// <summary>
        /// Yaz→oku round-trip karşılaştırma toleransı (mm). Belge birimi dönüşümünde
        /// oluşabilecek kayan nokta hatası için geometrik epsilonun üstünde tutulur.
        /// </summary>
        public const double RoundTripMm = 1e-3;

        /// <summary>İmza (snapshot) karşılaştırmasında kullanılan ondalık basamak sayısı.</summary>
        public const int SignatureDecimals = 4;
    }
}
