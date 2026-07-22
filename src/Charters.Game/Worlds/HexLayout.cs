using Charters.Sim.Hexes;
using Charters.Sim.Map;
using Godot;

namespace Charters.Game.Worlds;

/// <summary>Pointy-top axial hex layout: sim coordinates to world positions on the XZ plane.</summary>
internal static class HexLayout
{
    public const float HexSize = 1f;

    private static readonly float Sqrt3 = Mathf.Sqrt(3f);

    public static Vector3 CenterOf(HexAddress coordinate)
    {
        return new Vector3(
            HexSize * Sqrt3 * (coordinate.Q + coordinate.R * 0.5f),
            0f,
            HexSize * 1.5f * coordinate.R);
    }

    public static Vector3 CornerOffset(int corner)
    {
        var angle = Mathf.DegToRad(60f * corner - 30f);
        return new Vector3(HexSize * Mathf.Cos(angle), 0f, HexSize * Mathf.Sin(angle));
    }

    public static (Vector3 Center, float Extent) BoundsOf(WorldMap map)
    {
        Vector3 minimum = new(float.MaxValue, 0f, float.MaxValue);
        Vector3 maximum = new(float.MinValue, 0f, float.MinValue);
        for (var i = 0; i < map.Count; i++)
        {
            var center = CenterOf(map.HexAt(i).Address);
            minimum = new Vector3(Mathf.Min(minimum.X, center.X), 0f, Mathf.Min(minimum.Z, center.Z));
            maximum = new Vector3(Mathf.Max(maximum.X, center.X), 0f, Mathf.Max(maximum.Z, center.Z));
        }

        var extent = Mathf.Max(maximum.X - minimum.X, maximum.Z - minimum.Z) + 2f * HexSize;
        return ((minimum + maximum) * 0.5f, extent);
    }
}