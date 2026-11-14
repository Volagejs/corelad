using System;
using System.Collections.Generic;
using Corelad.Geometry;
using Corelad.Geometry.Analysis;
using Corelad.Geometry.Diagnostics;
using CorelShape = Corel.Interop.VGCore.Shape;

namespace Corelad.Dock.Corel
{
    /// <summary>Smart Divide çalıştırmasının sonucu ve kullanıcıya raporlanacak özet.</summary>
    public sealed class SmartDivideRunResult
    {
        internal SmartDivideRunResult(
            SmartDivideAnalysis analysis,
            IReadOnlyList<CorelShape> createdShapes,
            bool applied,
            ProblemCode? skipCode,
            string message)
        {
            Analysis = analysis;
            CreatedShapes = createdShapes;
            Applied = applied;
            SkipCode = skipCode;
            Message = message ?? string.Empty;
        }

        /// <summary>Analiz sonucu; okunamayan nesnede null olabilir.</summary>
        public SmartDivideAnalysis Analysis { get; }

        /// <summary>Belgeye eklenen yeni şekiller (kaynak değil).</summary>
        public IReadOnlyList<CorelShape> CreatedShapes { get; }

        /// <summary>Yeni şekiller yazıldı mı?</summary>
        public bool Applied { get; }

        /// <summary>Yazma yapılmadıysa neden (varsa).</summary>
        public ProblemCode? SkipCode { get; }

        public string Message { get; }

        public bool Skipped => !Applied;

        public int CreatedShapeCount => CreatedShapes.Count;

        internal static SmartDivideRunResult Written(
            SmartDivideAnalysis analysis,
            IReadOnlyList<CorelShape> shapes)
        {
            return new SmartDivideRunResult(
                analysis,
                shapes,
                true,
                null,
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0} mantıksal harf yeni nesne olarak yazıldı; tek undo ile geri alınabilir.",
                    shapes.Count));
        }

        internal static SmartDivideRunResult SkippedFor(
            SmartDivideAnalysis analysis,
            ProblemCode? code,
            string message)
        {
            return new SmartDivideRunResult(
                analysis,
                Array.Empty<CorelShape>(),
                false,
                code,
                message);
        }
    }
}
