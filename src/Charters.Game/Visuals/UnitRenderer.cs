using Charters.Game.Worlds;
using Charters.Sim.Core;
using Charters.Sim.Units;
using Godot;

namespace Charters.Game.Visuals;

/// <summary>Draws every unit as a small colored rectangle at its hex position.</summary>
public sealed partial class UnitRenderer : MultiMeshInstance3D
{
    private const float MarkerSize = 0.38f;
    private const float MarkerHeight = 0.02f;

    [Export]
    public Shared.ColorPalette? UnitPalette { get; set; }

    public void Render(Simulation simulation)
    {
        var unitCount = simulation.Views.Units.UnitCount;

        if (Multimesh is null || Multimesh.InstanceCount != unitCount)
        {
            Multimesh = CreateMarkerMultiMesh(unitCount);
        }

        var state = new UnitDrawState
        {
            Index = 0,
            ColorPalette = UnitPalette,
            Mesh = Multimesh
        };
        
        simulation.Views.Units.ForEachUnit(OnUnitDraw, ref state);
    }

    private static void OnUnitDraw(UnitView unit, ref UnitDrawState state)
    {
        var transform = new Transform3D(
            Basis.Identity,
            HexLayout.CenterOf(unit.Position) + Vector3.Up * MarkerHeight);
        state.Mesh.SetInstanceTransform(state.Index, transform);

        var color = state.ColorPalette?.ColorOf(unit.Owner.CharterId?.Value.ToString() ?? "charterless") ?? Colors.Magenta;
        state.Mesh.SetInstanceColor(state.Index, color);
        state.Index++;
    }

    private struct UnitDrawState
    {
        public int Index;
        public MultiMesh Mesh;
        public Shared.ColorPalette? ColorPalette;
    }

    private static Mesh BuildMarkerMesh()
    {
        var half = MarkerSize * 0.5f;
        SurfaceTool surfaceTool = new();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool.SetColor(Colors.White);
        surfaceTool.SetNormal(Vector3.Up);
        surfaceTool.AddVertex(new Vector3(-half, 0f, -half));
        surfaceTool.AddVertex(new Vector3(half, 0f, half));
        surfaceTool.AddVertex(new Vector3(half, 0f, -half));
        surfaceTool.AddVertex(new Vector3(-half, 0f, -half));
        surfaceTool.AddVertex(new Vector3(-half, 0f, half));
        surfaceTool.AddVertex(new Vector3(half, 0f, half));

        var mesh = surfaceTool.Commit();
        mesh.SurfaceSetMaterial(0,
            new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                VertexColorUseAsAlbedo = true,
                CullMode = BaseMaterial3D.CullModeEnum.Disabled
            });
        return mesh;
    }

    private static MultiMesh CreateMarkerMultiMesh(int instanceCount)
    {
        MultiMesh multiMesh = new()
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseColors = true,
            Mesh = BuildMarkerMesh()
        };
        multiMesh.InstanceCount = instanceCount;
        return multiMesh;
    }
}
