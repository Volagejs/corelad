using System;

namespace Corelad.Dock.Diagnostics
{
    /// <summary><see cref="ContourHighlightRequest"/> olay verisi.</summary>
    public sealed class ContourHighlightRequestedEventArgs : EventArgs
    {
        public ContourHighlightRequestedEventArgs(ContourHighlightRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            Request = request;
        }

        public ContourHighlightRequest Request { get; }
    }
}
