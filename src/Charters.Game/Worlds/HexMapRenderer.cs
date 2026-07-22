using Charters.Sim.Core;
using Charters.Sim.Map;
using Godot;

namespace Charters.Game.Worlds;

/// <summary>Draws the hex grid as one multimesh of flat terrain-colored tiles.</summary>
public sealed partial class HexMapRenderer : MultiMeshInstance3D
{
    private const float TileScale = 0.95f; // slight gap so individual hexes read

    [Export]
    public Shared.ColorPalette? TerrainPalette { get; set; }

    private static readonly Color MissingPaletteColor = new(1f, 0f, 1f);

    public void Render(Simulation simulation)
    {
        Multimesh = BuildTileMultiMesh(simulation.Map);
    }

    private MultiMesh BuildTileMultiMesh(WorldMap map)
    {
        var multiMesh = CreateTileMultiMesh(map.Count);
        var tileBasis = Basis.Identity.Scaled(new Vector3(TileScale, 1f, TileScale));
        for (var i = 0; i < map.Count; i++)
        {
            var hex = map.HexAt(i);
            multiMesh.SetInstanceTransform(
                i,
                new Transform3D(tileBasis, HexLayout.CenterOf(hex.Address)));
            multiMesh.SetInstanceColor(
                i,
                TerrainPalette?.ColorOf(hex.Terrain.Id) ?? MissingPaletteColor);
        }

        return multiMesh;
    }

    private static MultiMesh CreateTileMultiMesh(int instanceCount)
    {
        MultiMesh multiMesh = new()
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseColors = true,
            Mesh = BuildTileMesh()
        };
        multiMesh.InstanceCount = instanceCount;
        return multiMesh;
    }

    private static Mesh BuildTileMesh()
    {
        SurfaceTool surfaceTool = new();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool.SetColor(Colors.White);
        surfaceTool.SetNormal(Vector3.Up);
        for (var corner = 0; corner < 6; corner++)
        {
            surfaceTool.AddVertex(Vector3.Zero);
            surfaceTool.AddVertex(HexLayout.CornerOffset(corner + 1));
            surfaceTool.AddVertex(HexLayout.CornerOffset(corner));
        }

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
