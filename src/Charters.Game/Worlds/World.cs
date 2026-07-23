using Charters.Game.Visuals;
using Charters.Sim.Core;
using Godot;

namespace Charters.Game.Worlds;

/// <summary>Scene root: boots the sim and hands live state to the view nodes.</summary>
public sealed partial class World : Node3D
{
    private Simulation? _simulation;
    private EventLogPanel? _eventLog;
    private UnitRenderer? _units;
    private TickDriver? _driver;
    private Label? _tickLabel;

    public override void _Ready()
    {
        try
        {
            var boot = SimulationBootstrapper.Boot();
            _simulation = boot.Simulation;
            GetNode<RoadRenderer>("Roads").Render(boot.Roads);
        }
        catch (Exception exception)
        {
            GD.PushError($"Simulation boot failed: {exception.Message}");
            return;
        }

        GetNode<HexMapRenderer>("Map").Render(_simulation);
        GetNode<StructureRenderer>("Structures").Render(_simulation);
        _units = GetNode<UnitRenderer>("Units");
        _units.Render(_simulation);
        _eventLog = GetNode<EventLogPanel>("Ui/EventLog");
        _eventLog.UpdateFrom(_simulation);
        var (center, extent) = HexLayout.BoundsOf(_simulation.Map);
        GetNode<FreeCameraController>("Camera").Frame(center, extent);

        _tickLabel = GetNode<Label>("Ui/TickLabel");
        _driver = GetNode<TickDriver>("TickDriver");
        _driver.Simulation = _simulation;
        _driver.TickEnded += UpdateEventLog;
        _driver.Changed += Redraw;
        UpdateTickLabel();
    }

    private void Redraw()
    {
        _units!.Render(_simulation!);
        GetNode<StructureRenderer>("Structures").Render(_simulation!);
        UpdateTickLabel();
    }

    private void UpdateEventLog()
    {
        _eventLog!.UpdateFrom(_simulation!);
    }

    private void UpdateTickLabel()
    {
        var pace = _driver!.Paused ? "paused" : _driver.SpeedLabel;
        _tickLabel!.Text = $"tick {_simulation!.Tick} - {pace}  |  [E] event log";
    }
}
