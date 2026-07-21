using Arch.Core;
using Charters.Game.Worlds;
using Charters.Sim.Core;
using Charters.Sim.Movement.Components;
using Charters.Sim.Units.Components;
using Godot;

namespace Charters.Game.Visuals;

/// <summary>Draws every unit as a small colored rectangle at its hex position.</summary>
public sealed partial class UnitRenderer : MultiMeshInstance3D
{
    private const float MarkerSize = 0.38f;
    private const float MarkerHeight = 0.02f;

    [Export]
    public Shared.ColorPalette? UnitPalette { get; set; }

    private static readonly QueryDescription UnitQuery = new QueryDescription().WithAll<UnitIdentity, Position>();

    public void Render(Simulation simulation)
    {
        var unitCount = simulation.Entities.CountEntities(in UnitQuery);
        if (Multimesh is null || Multimesh.InstanceCount != unitCount)
        {
            Multimesh = CreateMarkerMultiMesh(unitCount);
        }

        var multiMesh = Multimesh;
        var instanceIndex = 0;
        simulation.Entities.Query(
            in UnitQuery,
            (ref UnitIdentity identity, ref Position position) =>
            {
                multiMesh.SetInstanceTransform(
                    instanceIndex,
                    new Transform3D(
                        Basis.Identity,
                        HexLayout.CenterOf(position.Address) + Vector3.Up * MarkerHeight));
                multiMesh.SetInstanceColor(instanceIndex, UnitPalette?.ColorOf(identity.Type.Id) ?? Colors.Magenta);
                instanceIndex++;
            });
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
}
