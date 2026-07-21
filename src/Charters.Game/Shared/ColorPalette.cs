using Godot;

namespace Charters.Game.Shared;

/// <summary>Editor-authored id-to-color mapping; ids without an entry render the fallback color.</summary>
[GlobalClass]
public sealed partial class ColorPalette : Resource
{
    [Export]
    public Godot.Collections.Dictionary<string, Color> Colors { get; set; } = [];

    [Export]
    public Color Fallback { get; set; } = new(1f, 0f, 1f); // loud magenta so unmapped ids are obvious

    public Color ColorOf(string id)
    {
        return Colors.GetValueOrDefault(id, Fallback);
    }
}