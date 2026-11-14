using System.Collections.Generic;
using Corelad.Geometry.Diagnostics;
using Corelad.Geometry.Model;

namespace Corelad.Geometry.Analysis
{
    /// <summary>Bir konturun analiz sonucu: derinlik, rol, ebeveyn/çocuklar, problemler.</summary>
    public sealed class ContourClassification
    {
        internal ContourClassification(
            int index,
            int? parentIndex,
            IReadOnlyList<int> childIndices,
            int depth,
            ContourRole role,
            Point2 interiorPoint,
            double areaMm2,
            Rect2 bounds,
            bool isExcluded,
            IReadOnlyList<Problem> problems)
        {
            Index = index;
            ParentIndex = parentIndex;
            ChildIndices = childIndices;
            Depth = depth;
            Role = role;
            InteriorPoint = interiorPoint;
            AreaMm2 = areaMm2;
            Bounds = bounds;
            IsExcluded = isExcluded;
            Problems = problems;
        }

        public int Index { get; }

        public int? ParentIndex { get; }

        public IReadOnlyList<int> ChildIndices { get; }

        /// <summary>Containment derinliği: 0 = dış gövde, 1 = delik, 2 = ada.</summary>
        public int Depth { get; }

        public ContourRole Role { get; }

        public Point2 InteriorPoint { get; }

        public double AreaMm2 { get; }

        public Rect2 Bounds { get; }

        /// <summary>Geometrik olarak geçersiz olduğu için analiz dışı bırakıldı.</summary>
        public bool IsExcluded { get; }

        public IReadOnlyList<Problem> Problems { get; }

        public bool IsProblematic => Problems.Count > 0;
    }
}
