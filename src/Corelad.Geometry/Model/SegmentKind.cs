namespace Corelad.Geometry.Model
{
    /// <summary>Segment türü. Line ise yalnızca P0 ve P3 anlamlıdır (P1 = P0, P2 = P3).</summary>
    public enum SegmentKind
    {
        Line = 0,
        Cubic = 1,
    }
}
