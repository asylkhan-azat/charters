using Charters.Sim.Core;
using Charters.Sim.Hexes;
using Godot;

namespace Charters.Game.Worlds;

/// <summary>Draws facilities, neutral depots, and any ground piles from read projections.</summary>
public sealed partial class StructureRenderer : Node3D
{
    [Export]
    public Charters.Game.Shared.ColorPalette? CharterPalette { get; set; }

    public void Render(Simulation simulation)
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }

        foreach (var facility in simulation.Views.State.Facilities())
        {
            AddMarker(facility.Location, FacilityColor(facility.TypeId, OwnerColor(facility.Owner)), 0.57f, 0.09f);
        }

        foreach (var depot in simulation.Views.State.DepotCompartments().Where(static depot => depot.CharterId is null))
        {
            AddMarker(depot.Location, new Color(0.72f, 0.72f, 0.68f), 0.46f, 0.07f);
        }

        foreach (var pile in simulation.Views.State.GroundStockpiles())
        {
            AddMarker(pile.Location, OwnerColor(pile.Owner), 0.25f, 0.12f);
        }
    }

    private Color OwnerColor(Charters.Sim.Charters.Ownership owner)
    {
        return CharterPalette?.ColorOf(owner.CharterId?.Value.ToString() ?? "charterless") ?? Colors.Magenta;
    }

    private static Color FacilityColor(string type, Color owner) => type switch
    {
        "mine" => owner.Lightened(0.12f),
        "refinery" => owner.Darkened(0.12f),
        "factory" => owner,
        "farm" => owner.Lerp(new Color(0.35f, 0.72f, 0.30f), 0.35f),
        _ => owner
    };

    private void AddMarker(HexAddress address, Color color, float size, float height)
    {
        var marker = new MeshInstance3D
        {
            Mesh = new CylinderMesh
            {
                TopRadius = size, BottomRadius = size, Height = height
            }
        };
        marker.Position = HexLayout.CenterOf(address) + Vector3.Up * (height * 0.5f);
        marker.MaterialOverride = new StandardMaterial3D
            { AlbedoColor = color, ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
        AddChild(marker);
    }
}