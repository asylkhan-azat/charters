using Charters.Sim.Charters;
using Charters.Sim.Core;
using Charters.Sim.Facilities.Definitions;
using Charters.Sim.Hexes;
using Charters.Sim.Items;

namespace Charters.Sim.Facilities.Models;

public sealed class Facility : IIdentifiable<FacilityId>
{
    private BatchPhase _phase = BatchPhase.AwaitingInputs;

    public Facility(
        FacilityId id,
        FacilityTypeDefinition type,
        CharterId owner,
        HexAddress location,
        RecipeDefinition recipe)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(recipe);

        Id = id;
        Type = type;
        Owner = owner;
        Location = location;
        Stockpile = new Stockpile();
        CurrentRecipe = recipe;
    }

    public FacilityId Id { get; }

    public FacilityTypeDefinition Type { get; }

    public CharterId Owner { get; private set; }

    public HexAddress Location { get; }

    public Stockpile Stockpile { get; }

    public RecipeDefinition CurrentRecipe { get; private set; }

    public int ProgressTicks { get; private set; }

    public FacilityStatus LastStatus { get; private set; } = FacilityStatus.Unstaffed;

    public bool HasCompletedBatch => _phase == BatchPhase.Completed;

    // No retooling cost, but switching mid-batch would let a Charter discard consumed inputs or
    // in-progress work for free, so it's only legal between batches.
    public bool CanSwitchRecipe => _phase == BatchPhase.AwaitingInputs;

    // Recomputed every tick from scratch; a facility never remembers which workers staffed it.
    internal int ClaimedSpots { get; private set; }

    /// <summary>Changes the owner in place. Callers decide whether the embedded stock stays or is evicted first.</summary>
    internal void ChangeOwner(CharterId newOwner)
    {
        Owner = newOwner;
    }

    public void SwitchRecipe(RecipeDefinition recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (!Type.AllowedRecipes.Contains(recipe))
        {
            throw new SimulationInvariantException(
                $"Recipe '{recipe.Id}' is not in the allowed set for facility type '{Type.Id}'.");
        }

        if (!CanSwitchRecipe)
        {
            throw new SimulationInvariantException(
                "Cannot switch recipe while a batch is in progress or awaiting output space.");
        }

        CurrentRecipe = recipe;
    }

    internal void ResetClaimedSpots()
    {
        ClaimedSpots = 0;
    }

    // Slots are capped by the facility type regardless of how many workers are assigned to it.
    internal bool TryClaimSpot()
    {
        if (ClaimedSpots >= Type.WorkerSlots)
        {
            return false;
        }

        ClaimedSpots++;
        return true;
    }

    // Ignored outside InProgress: a worker can claim a spot the same tick inputs are consumed and
    // the batch begins, but that tick's work is spent starting the batch, not advancing it. This is
    // why staffing a facility from empty costs one tick before any work shows up.
    internal void AddWork(int workAmount)
    {
        if (_phase != BatchPhase.InProgress)
        {
            return;
        }

        ProgressTicks = Math.Min(ProgressTicks + workAmount, CurrentRecipe.WorkRequired);
        if (ProgressTicks >= CurrentRecipe.WorkRequired)
        {
            _phase = BatchPhase.Completed;
        }
    }

    public FacilityTickOutcome RunProductionTick()
    {
        RecipeDefinition? consumedRecipe = null;
        RecipeDefinition? producedRecipe = null;

        // Try to hand off a finished batch first: this can free storage a blocked facility needed,
        // letting it fall straight into starting its next batch in the same tick.
        if (HasCompletedBatch)
        {
            if (TryInsertOutput())
            {
                producedRecipe = CurrentRecipe;
                _phase = BatchPhase.AwaitingInputs;
                ProgressTicks = 0;
            }
            else
            {
                LastStatus = FacilityStatus.OutputBlocked;
                return new FacilityTickOutcome(consumedRecipe, producedRecipe);
            }
        }

        if (ClaimedSpots == 0)
        {
            LastStatus = FacilityStatus.Unstaffed;
            return new FacilityTickOutcome(consumedRecipe, producedRecipe);
        }

        if (_phase == BatchPhase.AwaitingInputs)
        {
            if (Stockpile.HasAll(CurrentRecipe.Inputs.AsSpan()))
            {
                Stockpile.TakeAll(CurrentRecipe.Inputs.AsSpan());
                consumedRecipe = CurrentRecipe;
                _phase = BatchPhase.InProgress;
                ProgressTicks = 0;
            }
            else
            {
                LastStatus = FacilityStatus.MissingInputs;
                return new FacilityTickOutcome(consumedRecipe, producedRecipe);
            }
        }

        LastStatus = FacilityStatus.Producing;
        return new FacilityTickOutcome(consumedRecipe, producedRecipe);
    }

    private bool TryInsertOutput()
    {
        if (!Stockpile.CanAcceptAll(CurrentRecipe.Outputs.AsSpan()))
        {
            return false;
        }

        Stockpile.PutAll(CurrentRecipe.Outputs.AsSpan());
        return true;
    }

    private enum BatchPhase
    {
        AwaitingInputs,
        InProgress,
        Completed
    }
}
