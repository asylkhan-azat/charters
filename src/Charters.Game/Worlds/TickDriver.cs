using Charters.Sim.Core;
using Godot;

namespace Charters.Game.Worlds;

/// <summary>
/// Advances the sim in real time. Space toggles pause; 1/2/3 select speed steps.
/// View-side only: the sim never knows whether a driver, a test, or the headless CLI is ticking it.
/// </summary>
public sealed partial class TickDriver : Node
{
    private static readonly (float TicksPerSecond, string Label)[] Speeds =
    [
        (8f, "x1"),
        (32f, "x4"),
        (128f, "x16")
    ];

    private const int MaxTicksPerFrame = 32;

    private int _speedIndex;
    private double _pendingTicks;

    public Simulation? Simulation { get; set; }
    public bool Paused { get; private set; }
    public string SpeedLabel => Speeds[_speedIndex].Label;

    public event Action? TickEnded;
    public event Action? Changed;

    public override void _Process(double delta)
    {
        if (Simulation is null || Paused)
        {
            return;
        }

        _pendingTicks += delta * Speeds[_speedIndex].TicksPerSecond;
        var ticks = (int)_pendingTicks;
        if (ticks <= 0)
        {
            return;
        }

        if (ticks > MaxTicksPerFrame)
        {
            ticks = MaxTicksPerFrame;
            _pendingTicks = 0;
        }
        else
        {
            _pendingTicks -= ticks;
        }

        for (var i = 0; i < ticks; i++)
        {
            Simulation.Advance();
            TickEnded?.Invoke();
        }

        Changed?.Invoke();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key)
        {
            return;
        }

        switch (key.PhysicalKeycode)
        {
            case Key.Space:
                Paused = !Paused;
                _pendingTicks = 0;
                Changed?.Invoke();
                break;
            case Key.Key1:
                SelectSpeed(0);
                break;
            case Key.Key2:
                SelectSpeed(1);
                break;
            case Key.Key3:
                SelectSpeed(2);
                break;
        }
    }

    private void SelectSpeed(int speedIndex)
    {
        if (_speedIndex == speedIndex)
        {
            return;
        }

        _speedIndex = speedIndex;
        Changed?.Invoke();
    }
}
