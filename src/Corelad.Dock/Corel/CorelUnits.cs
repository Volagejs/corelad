using System;
using Corel.Interop.VGCore;

namespace Corelad.Dock.Corel
{
    /// <summary>
    /// Belge birimi &lt;-&gt; kanonik mm dönüşümü. Tek dönüşüm noktası (ADR-0005);
    /// geometri çekirdeği yalnızca mm görür.
    /// </summary>
    public static class CorelUnits
    {
        public static double ToMillimeters(Document document, double documentUnits)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            return document.Unit == cdrUnit.cdrMillimeter
                ? documentUnits
                : document.FromUnits(documentUnits, cdrUnit.cdrMillimeter);
        }

        public static double FromMillimeters(Document document, double millimeters)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            return document.Unit == cdrUnit.cdrMillimeter
                ? millimeters
                : document.ToUnits(millimeters, cdrUnit.cdrMillimeter);
        }
    }
}
