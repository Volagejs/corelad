namespace Corelad.Dock.Diagnostics
{
    /// <summary>
    /// Kontur vurgu isteğini tüketen entegrasyon arayüzü. Uygulama CorelDRAW katmanında
    /// yapılır; bu görev kapsamında hiçbir uygulama yoktur ve COM çağrısı yapılmaz.
    /// </summary>
    public interface IContourHighlightRequestSink
    {
        /// <summary>
        /// UI thread'inde çağrılır. Uygulama hızlı dönmeli, uzun işi arka plana bırakmalıdır.
        /// </summary>
        void HandleHighlightRequest(ContourHighlightRequest request);
    }
}
