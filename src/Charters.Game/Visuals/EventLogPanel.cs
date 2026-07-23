using Charters.Sim.Core;
using Charters.Sim.Core.Diagnostics;
using Godot;

namespace Charters.Game.Visuals;

/// <summary>Displays the presentation-event feed without exposing simulation state to the UI.</summary>
public sealed partial class EventLogPanel : PanelContainer
{
    private RichTextLabel? _entries;
    private long _lastSequence = -1;

    public override void _Ready()
    {
        _entries = GetNode<RichTextLabel>("Margin/Layout/Entries");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey
            {
                Pressed: true,
                Echo: false,
                PhysicalKeycode: Key.E
            })
        {
            return;
        }

        Visible = !Visible;
        GetViewport().SetInputAsHandled();
    }

    public void UpdateFrom(Simulation simulation)
    {
        var state = new AppendState
        {
            Panel = this
        };
        simulation.Views.Diagnostics.ForEachPresentationEvent(AppendNewEvent, ref state);
    }

    private static void AppendNewEvent(PresentationEvent occurrence, ref AppendState state)
    {
        if (occurrence.Sequence <= state.Panel._lastSequence)
        {
            return;
        }

        state.Panel._entries!.AppendText(Format(occurrence));
        state.Panel._lastSequence = occurrence.Sequence;
    }

    private static string Format(PresentationEvent occurrence)
    {
        var message = occurrence.Kind switch
        {
            PresentationEventKind.FacilityInputsConsumed =>
                $"Facility {occurrence.FacilityId} consumed batch inputs.",
            PresentationEventKind.FacilityOutputsProduced =>
                $"Facility {occurrence.FacilityId} completed a batch.",
            PresentationEventKind.FacilityStatusRecorded =>
                $"Facility {occurrence.FacilityId}: {occurrence.FacilityStatus}.",
            PresentationEventKind.FacilityOwnershipChanged =>
                $"Facility {occurrence.FacilityId} changed ownership.",
            PresentationEventKind.CharterDissolved =>
                $"Charter {occurrence.CharterId} dissolved.",
            PresentationEventKind.GroundStockpileExpired =>
                $"Ground stockpile {occurrence.GroundStockpileId} expired.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                occurrence.Kind,
                "Unknown presentation event kind.")
        };

        return $"[tick {occurrence.Tick}] {message}\n";
    }

    private struct AppendState
    {
        public required EventLogPanel Panel;
    }
}
