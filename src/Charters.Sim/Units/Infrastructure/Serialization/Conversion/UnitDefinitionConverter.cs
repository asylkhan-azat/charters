using Charters.Sim.Units.Definitions;
using Charters.Sim.Units.Infrastructure.Serialization.Dto;

namespace Charters.Sim.Units.Infrastructure.Serialization.Conversion;

internal static class UnitDefinitionConverter
{
    public static UnitDefinition[] Convert(IReadOnlyList<UnitDto> units)
    {
        var result = new UnitDefinition[units.Count];
        for (var i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            result[i] = new UnitDefinition(
                unit.Id!,
                unit.Name!,
                []);
        }

        return result;
    }
}
