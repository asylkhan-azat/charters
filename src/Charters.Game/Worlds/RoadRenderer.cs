using Charters.Sim.Scenarios;
using Godot;

namespace Charters.Game.Worlds;

/// <summary>Renders authored road segments as neutral infrastructure lines.</summary>
public sealed partial class RoadRenderer : MeshInstance3D
{
    public void Render(IReadOnlyList<ResolvedRoadSegment> roads)
    {
        var surface = new SurfaceTool();
        surface.Begin(Mesh.PrimitiveType.Lines);
        surface.SetColor(new Color(0.42f, 0.44f, 0.47f));
        foreach (var road in roads)
        {
            for (var i = 1; i < road.Hexes.Count; i++)
            {
                surface.AddVertex(HexLayout.CenterOf(road.Hexes[i - 1]) + Vector3.Up * 0.025f);
                surface.AddVertex(HexLayout.CenterOf(road.Hexes[i]) + Vector3.Up * 0.025f);
            }
        }

        Mesh = surface.Commit();
    }
}
