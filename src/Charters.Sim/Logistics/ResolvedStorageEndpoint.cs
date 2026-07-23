using Charters.Sim.Charters;
using Charters.Sim.Hexes;
using Charters.Sim.Items;

namespace Charters.Sim.Logistics;

internal readonly record struct ResolvedStorageEndpoint(
    StorageEndpoint Endpoint,
    Ownership Owner,
    HexAddress Location,
    IItemContainer Container);
