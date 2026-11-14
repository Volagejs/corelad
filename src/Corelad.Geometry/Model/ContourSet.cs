using System;
using System.Collections.Generic;

namespace Corelad.Geometry.Model
{
    /// <summary>Bir nesnenin tüm alt konturları (subpath'leri).</summary>
    public sealed class ContourSet
    {
        private readonly Contour[] _contours;

        public ContourSet(IEnumerable<Contour> contours)
        {
            if (contours == null) throw new ArgumentNullException(nameof(contours));

            var buffer = new List<Contour>();
            foreach (var contour in contours)
            {
                if (contour == null) throw new ArgumentException("Kontur null olamaz.", nameof(contours));
                buffer.Add(contour);
            }

            _contours = buffer.ToArray();
        }

        public IReadOnlyList<Contour> Contours => _contours;

        public int Count => _contours.Length;

        public static ContourSet Empty => new ContourSet(Array.Empty<Contour>());

        public ContourSet WithContours(IEnumerable<Contour> contours) => new ContourSet(contours);
    }
}
