using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;

namespace Corelad.Geometry.Analysis
{
    /// <summary>Smart Divide analiz sonucu: sınıflandırma, adaylar, problemler.</summary>
    public sealed class SmartDivideAnalysis
    {
        private readonly List<Problem> _problems;

        internal SmartDivideAnalysis(
            ContourSet source,
            IReadOnlyList<ContourClassification> contours,
            IReadOnlyList<LetterCandidate> candidates,
            List<Problem> problems)
        {
            Source = source;
            Contours = contours;
            Candidates = candidates;
            _problems = problems;
        }

        public ContourSet Source { get; }

        public IReadOnlyList<ContourClassification> Contours { get; }

        public IReadOnlyList<LetterCandidate> Candidates { get; }

        public IReadOnlyList<Problem> Problems => _problems;

        public bool HasErrors
        {
            get
            {
                for (int i = 0; i < _problems.Count; i++)
                {
                    if (_problems[i].Severity == ProblemSeverity.Error) return true;
                }

                return false;
            }
        }

        public bool HasWarnings
        {
            get
            {
                for (int i = 0; i < _problems.Count; i++)
                {
                    if (_problems[i].Severity == ProblemSeverity.Warning) return true;
                }

                return false;
            }
        }

        /// <summary>Error varsa uygulama engellenir (ARCHITECTURE §7).</summary>
        public bool CanApply => !HasErrors && Candidates.Count > 0;

        public int CountProblems(ProblemSeverity severity)
        {
            int count = 0;
            for (int i = 0; i < _problems.Count; i++)
            {
                if (_problems[i].Severity == severity) count++;
            }

            return count;
        }

        public IReadOnlyList<Problem> ProblemsForContour(int contourIndex)
        {
            var result = new List<Problem>();
            for (int i = 0; i < _problems.Count; i++)
            {
                var problem = _problems[i];
                if (problem.ContourIndex == contourIndex || problem.RelatedContourIndex == contourIndex)
                {
                    result.Add(problem);
                }
            }

            return result;
        }

        public LetterCandidate? FindCandidateByOuter(int outerIndex)
        {
            for (int i = 0; i < Candidates.Count; i++)
            {
                if (Candidates[i].OuterIndex == outerIndex) return Candidates[i];
            }

            return null;
        }

        public string Describe()
        {
            var sb = new StringBuilder();
            sb.AppendFormat(
                CultureInfo.InvariantCulture,
                "Kontur: {0}, aday: {1}, problem: {2} (E{3}/W{4})",
                Contours.Count,
                Candidates.Count,
                _problems.Count,
                CountProblems(ProblemSeverity.Error),
                CountProblems(ProblemSeverity.Warning));
            sb.AppendLine();

            for (int i = 0; i < Candidates.Count; i++)
            {
                sb.Append("  ");
                sb.AppendLine(Candidates[i].ToString());
            }

            for (int i = 0; i < _problems.Count; i++)
            {
                sb.Append("  ");
                sb.AppendLine(_problems[i].ToString());
            }

            return sb.ToString().TrimEnd();
        }
    }
}
