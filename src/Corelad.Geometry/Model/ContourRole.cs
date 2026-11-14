namespace Corelad.Geometry.Model
{
    /// <summary>
    /// Konturun geometrik rolü. <see cref="Unknown"/> varsayılandır; rolü
    /// <c>ContainmentAnalyzer</c> belirler (yön/renk değil, gerçek containment).
    /// </summary>
    public enum ContourRole
    {
        Unknown = 0,
        Outer = 1,
        Hole = 2,
    }
}
