using Corelad.Geometry.Diagnostics;

namespace Corelad.Dock.Tests.Support
{
    /// <summary>Testlerde Problem kurmayı tek yerde toplar; geometri hesabı yapmaz.</summary>
    internal static class ProblemFactory
    {
        public static Problem Create(
            ProblemCode code = ProblemCode.OpenContour,
            ProblemSeverity severity = ProblemSeverity.Error,
            string message = "test mesajı",
            int? contourIndex = null,
            int? relatedContourIndex = null,
            int? sourceShapeId = null,
            int? sourceSubPathIndex = null,
            int? relatedSourceSubPathIndex = null)
        {
            return new Problem(
                code,
                severity,
                message,
                contourIndex,
                relatedContourIndex,
                sourceShapeId,
                sourceSubPathIndex,
                relatedSourceSubPathIndex);
        }
    }
}
