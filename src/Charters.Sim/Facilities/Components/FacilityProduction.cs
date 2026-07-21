using System.Diagnostics.CodeAnalysis;
using Charters.Sim.Facilities.Definitions;
using Charters.Sim.Items.Components;

namespace Charters.Sim.Facilities.Components;

public struct FacilityProduction
{
    public int ProgressTicks { get; private set; }

    public int WorkerCount { get; private set; }

    public RecipeDefinition? CurrentRecipe { get; private set; }
    public FacilityProductionState State { get; private set; }

    public bool Active => WorkerCount > 0;

    public bool CanProduce
    {
        get
        {
            if (CurrentRecipe is null || State is not FacilityProductionState.Producing)
            {
                return false;
            }

            return ProgressTicks >= CurrentRecipe.ProgressTicks;
        }
    }

    public bool CanConsume
    {
        get
        {
            if (CurrentRecipe is null || State is not FacilityProductionState.WaitingForInputs)
            {
                return false;
            }

            return true;
        }
    }

    public void AddWork(int workAmount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workAmount);
        ProgressTicks += workAmount;
    }
    
    [MemberNotNull(nameof(CurrentRecipe))]
    public bool TryConsumeInputs(ref Stockpile stockpile)
    {
        if (CurrentRecipe is null)
        {
            throw new InvalidOperationException("No active recipe");
        }

        if (State is not FacilityProductionState.WaitingForInputs)
        {
            throw new InvalidOperationException("Not waiting for inputs");
        }

        if (stockpile.HasAtLeast(CurrentRecipe.Inputs.AsSpan()))
        {
            stockpile.Take(CurrentRecipe.Inputs.AsSpan());
            State = FacilityProductionState.Producing;
            return true;
        }

        return false;
    }

    [MemberNotNull(nameof(CurrentRecipe))]
    public void Produce(ref Stockpile stockpile)
    {
        if (CurrentRecipe is null)
        {
            throw new InvalidOperationException("No active recipe");
        }

        if (State is not FacilityProductionState.Producing)
        {
            throw new InvalidOperationException("Not producing item");
        }

        stockpile.Put(CurrentRecipe.Outputs.AsSpan());
        ProgressTicks = 0;
        State = FacilityProductionState.WaitingForInputs;
    }

    public void SwitchRecipe(RecipeDefinition recipe)
    {
        if (!Equals(recipe, CurrentRecipe))
        {
            ProgressTicks = 0;
        }

        CurrentRecipe = recipe;
        State = FacilityProductionState.WaitingForInputs;
    }
}