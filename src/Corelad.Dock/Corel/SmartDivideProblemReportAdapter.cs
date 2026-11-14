using System;
using System.Collections.Generic;
using Corelad.Geometry.Diagnostics;

namespace Corelad.Dock.Corel
{
    /// <summary>
    /// Smart Divide çalışma sonucunu rapor panelinin alabileceği Problem listesine çevirir.
    /// COM çağrısı yapmaz; SmartDivideService ile WPF sunum katmanı arasındaki adaptördür.
    /// </summary>
    public static class SmartDivideProblemReportAdapter
    {
        public static IReadOnlyList<Problem> ToProblems(SmartDivideRunResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var problems = new List<Problem>();
            if (result.Analysis != null)
            {
                for (int i = 0; i < result.Analysis.Problems.Count; i++)
                {
                    problems.Add(result.Analysis.Problems[i]);
                }
            }

            if (result.Skipped && result.SkipCode.HasValue && !ContainsCode(problems, result.SkipCode.Value))
            {
                // Tür/ebeveyn kontrolünde analiz hiç başlamayabilir; nedeni yine panelde görünmeli.
                problems.Add(new Problem(
                    result.SkipCode.Value,
                    ProblemSeverity.Error,
                    result.Message));
            }

            return problems.AsReadOnly();
        }

        private static bool ContainsCode(IReadOnlyList<Problem> problems, ProblemCode code)
        {
            for (int i = 0; i < problems.Count; i++)
            {
                if (problems[i].Code == code) return true;
            }

            return false;
        }
    }
}
