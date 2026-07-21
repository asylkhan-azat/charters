using Godot;

namespace Charters.Game;

/// <summary>Top-down free camera: WASD/arrow pan, mouse-wheel zoom, middle/right-drag pan.</summary>
public sealed partial class FreeCameraController : Camera3D
{
    private const float MinHeight = 8f;
    private const float MaxHeight = 600f;
    private const float ZoomStepFactor = 1.15f;
    private const float KeyPanHeightsPerSecond = 1.2f;
    private const float FrameMargin = 1.1f;

    private bool _dragging;

    /// <summary>Positions the camera to see the whole map extent at once.</summary>
    public void Frame(Vector3 center, float extent)
    {
        var height = Mathf.Clamp(
            extent * FrameMargin * 0.5f / Mathf.Tan(Mathf.DegToRad(Fov) * 0.5f),
            MinHeight,
            MaxHeight);
        Position = new Vector3(center.X, height, center.Z);
        RotationDegrees = new Vector3(-90f, 0f, 0f);
    }

    public override void _Process(double delta)
    {
        var pan = Vector2.Zero;
        if (Input.IsPhysicalKeyPressed(Key.W) || Input.IsPhysicalKeyPressed(Key.Up)) pan.Y -= 1f;
        if (Input.IsPhysicalKeyPressed(Key.S) || Input.IsPhysicalKeyPressed(Key.Down)) pan.Y += 1f;
        if (Input.IsPhysicalKeyPressed(Key.A) || Input.IsPhysicalKeyPressed(Key.Left)) pan.X -= 1f;
        if (Input.IsPhysicalKeyPressed(Key.D) || Input.IsPhysicalKeyPressed(Key.Right)) pan.X += 1f;
        if (pan == Vector2.Zero) return;

        pan = pan.Normalized() * (Position.Y * KeyPanHeightsPerSecond * (float)delta);
        Position += new Vector3(pan.X, 0f, pan.Y);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseButton { ButtonIndex: MouseButton.WheelUp, Pressed: true }:
                Zoom(1f / ZoomStepFactor);
                break;
            case InputEventMouseButton { ButtonIndex: MouseButton.WheelDown, Pressed: true }:
                Zoom(ZoomStepFactor);
                break;
            case InputEventMouseButton { ButtonIndex: MouseButton.Middle or MouseButton.Right } drag:
                _dragging = drag.Pressed;
                break;
            case InputEventMouseMotion motion when _dragging:
                var worldPerPixel = 2f * Position.Y * Mathf.Tan(Mathf.DegToRad(Fov) * 0.5f)
                                    / GetViewport().GetVisibleRect().Size.Y;
                Position -= new Vector3(motion.Relative.X * worldPerPixel, 0f, motion.Relative.Y * worldPerPixel);
                break;
        }
    }

    private void Zoom(float factor)
    {
        Position = Position with { Y = Mathf.Clamp(Position.Y * factor, MinHeight, MaxHeight) };
    }
}