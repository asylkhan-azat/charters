using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Facilities.Definitions;
using Charters.Sim.Facilities.Models;
using Charters.Sim.Hexes;

namespace Charters.Sim.Facilities;

public sealed class FacilityFactory
{
    private readonly Simulation _simulation;
    private long _idCounter;

    internal FacilityFactory(Simulation simulation)
    {
        _simulation = simulation;
        foreach (var facility in simulation.Registries.Facilities)
        {
            _idCounter = Math.Max(_idCounter, checked(facility.Id.Value + 1));
        }
    }

    public FacilityId Register(
        FacilityTypeDefinition type,
        Ownership owner,
        HexAddress location,
        RecipeDefinition recipe)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(recipe);
        _simulation.ValidateOwnership(owner);

        if (!type.AllowedRecipes.Contains(recipe))
        {
            throw new SimulationInvariantException(
                $"Recipe '{recipe.Id}' is not in the allowed set for facility type '{type.Id}'.");
        }

        var id = new FacilityId(_idCounter++);
        var facility = new Facility(id, type, owner, location, recipe);
        _simulation.Registries.Facilities.Add(facility);
        return id;
    }

    public FacilityId Register(
        FacilityTypeDefinition type,
        CharterId owner,
        HexAddress location,
        RecipeDefinition recipe)
    {
        var charter = _simulation.Registries.Charters[owner];
        return Register(type, new Ownership(charter.Nation, charter.Id), location, recipe);
    }
}
