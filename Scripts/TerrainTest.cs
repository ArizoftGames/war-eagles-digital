using Godot;
using System;
using System.Linq;

namespace WarEaglesDigital.Scripts
{
    public partial class TerrainTest : Node3D
    {
        private Camera3D _camera;
        private float _moveSpeed = 5.0f; // Adjustable camera move speed
        private float _panSpeed = 0.5f;  // Adjustable pan speed
        private float _zoomSpeed = 0.1f; // Adjustable zoom speed

        public override void _Ready()
        {
            _camera = GetNode<Camera3D>("Camera3D"); // Assumes a Camera3D child; adjust path if needed
            if (_camera == null)
            {
                GD.PrintErr("Camera3D not found in terrain_test.tscn. Please add one.");
                return;
            }
            GD.Print("TerrainTest ready. Controller input active for camera control.");
        }

        public override void _Process(double delta)
        {
            try
            {
                // Check if controller is connected
                if (!Input.GetConnectedJoypads().Any())
                {
                    GD.Print("No controller detected. Using keyboard/mouse as fallback.");
                    return;
                }

                // Camera movement with D-pad and LS
                float moveForward = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up"); // dpdown:b12, dpup:b11
                float strafe = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");   // dpright:b14, dpleft:b13
                float panX = Input.GetAxis("ui_left", "ui_right"); // leftx:a0 (-1 to 1)
                float panY = Input.GetAxis("ui_up", "ui_down");    // lefty:a1 (-1 to 1)

                Vector3 movement = new Vector3(strafe, 0, moveForward) * _moveSpeed * (float)delta;
                _camera.Translate(movement);

                Vector3 pan = new Vector3(panX, -panY, 0) * _panSpeed * (float)delta; // Invert Y for natural pan
                _camera.RotateX(pan.Y); // Pitch
                _camera.RotateY(pan.X); // Yaw

                // Zoom with triggers (RT:a5 for in, LT:a4 for out)
                float zoom = Input.GetAxis("ui_right_trigger", "ui_left_trigger") * _zoomSpeed * (float)delta;
                _camera.Translate(new Vector3(0, 0, zoom));
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in _Process: {e.Message}");
            }
        }

        public override void _Input(InputEvent @event)
        {
            try
            {
                if (@event is InputEventJoypadButton || @event is InputEventJoypadMotion)
                {
                    GD.Print($"Controller event: {@event.AsText()}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in _Input: {e.Message}");
            }
        }
    }
}