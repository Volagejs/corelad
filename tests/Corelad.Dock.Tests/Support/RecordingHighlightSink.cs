using System;
using System.Collections.Generic;
using Corelad.Dock.Diagnostics;

namespace Corelad.Dock.Tests.Support
{
    /// <summary>Vurgu isteklerini kaydeden test sink'i.</summary>
    internal sealed class RecordingHighlightSink : IContourHighlightRequestSink
    {
        private readonly Action<ContourHighlightRequest> _onRequest;

        public RecordingHighlightSink(Action<ContourHighlightRequest> onRequest = null)
        {
            _onRequest = onRequest;
        }

        public List<ContourHighlightRequest> Requests { get; } = new List<ContourHighlightRequest>();

        public void HandleHighlightRequest(ContourHighlightRequest request)
        {
            Requests.Add(request);
            _onRequest?.Invoke(request);
        }
    }
}
