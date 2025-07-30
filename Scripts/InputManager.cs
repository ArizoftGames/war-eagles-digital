using Godot;
using System;
using System.Collections.Generic;

namespace WarEaglesDigital.Scripts
{
    [GlobalClass]
    public partial class InputManager : Node
    {
        // --- Fields ---
        private float _moveSpeed = 35f; // 10-60
        private float _panSpeed = 0.5f; // 0.1-1.0
        private float _mousePanSpeed = 0.3f; // 0.1-0.5
        private float _pointerSpeed = 600f; // 300-800
        private float _cursorSpeed = 600f; // 200-1000
        private float _zoomSpeed = 1f;

        private string _currentBindings = "KBM"; // "KBM" or "Controller"

        // KBM state
        private bool _kbmMoveForward, _kbmMoveBack, _kbmStrafeLeft, _kbmStrafeRight;
        private bool _kbmPanLeft, _kbmPanRight, _kbmPanUp, _kbmPanDown;
        private bool _kbmZoomIn, _kbmZoomOut, _kbmLevelCamera;

        // Controller state
        private bool _controllerMoveForward, _controllerMoveBack, _controllerStrafeLeft, _controllerStrafeRight;
        private float _controllerPanX, _controllerPanY;
        private float _controllerZoom;
        private float _controllerPointerX, _controllerPointerY;
        private bool _controllerLevelCamera, _controllerSelectUnit;

        private Vector2 _pointerPosition = Vector2.Zero;

        // --- Signals ---
        [Signal] public delegate void CameraMoveEventHandler(Vector3 movement);
        [Signal] public delegate void CameraPanEventHandler(float yawDelta, float pitchDelta);
        [Signal] public delegate void CameraZoomEventHandler(float zoom);
        [Signal] public delegate void CameraLevelEventHandler();
        [Signal] public delegate void PointerMoveEventHandler(Vector2 position);
        [Signal] public delegate void SelectUnitEventHandler(Vector2 position);

        public override void _Ready()
        {
            GD.Print("InputManager _Ready entry");
            try
            {
                // Detect joypads
                var joypads = Input.GetConnectedJoypads();
                _currentBindings = (joypads.Count > 0) ? "Controller" : "KBM";
                GD.Print($"InputManager initialized, bindings: {_currentBindings}");

                // Connect to Loading.cs
                var loading = GetNodeOrNull<Node>("/root/Loading");
                if (loading != null)
                {
                    loading.Connect("SetBindings", Callable.From<string>(SetBindings));
                    loading.Connect("SetSpeed", Callable.From<string, float>(SetSpeed));
                }

                // Connect to ControlsMenuPanel.cs
                var controlsMenu = GetNodeOrNull<Node>("/root/ControlsMenuPanel");
                if (controlsMenu != null)
                {
                    controlsMenu.Connect("SetBindings", Callable.From<string>(SetBindings));
                }

                // Connect to GameplayMenuPanel.cs (NYI)
                var gameplayMenu = GetNodeOrNull<Node>("/root/GameplayMenuPanel");
                if (gameplayMenu != null)
                {
                    gameplayMenu.Connect("SetSpeed", Callable.From<string, float>(SetSpeed));
                }

                // Initialize pointer position to center of viewport
                var viewport = GetViewport();
                if (viewport != null)
                {
                    _pointerPosition = viewport.GetVisibleRect().Size / 2f;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager _Ready error: {ex.Message}");
            }
            GD.Print("InputManager _Ready exit");
        }

        public override void _Process(double delta)
        {
            try
            {
                if (_currentBindings == "KBM")
                {
                    // Poll KBM actions
                    _kbmMoveForward = Input.IsActionPressed("MoveForward");
                    _kbmMoveBack = Input.IsActionPressed("MoveBack");
                    _kbmStrafeLeft = Input.IsActionPressed("StrafeLeft");
                    _kbmStrafeRight = Input.IsActionPressed("StrafeRight");

                    _kbmPanLeft = Input.IsActionPressed("PanLeft");
                    _kbmPanRight = Input.IsActionPressed("PanRight");
                    _kbmPanUp = Input.IsActionPressed("PanUp");
                    _kbmPanDown = Input.IsActionPressed("PanDown");

                    _kbmZoomIn = Input.IsActionPressed("ZoomIn");
                    _kbmZoomOut = Input.IsActionPressed("ZoomOut");
                    _kbmLevelCamera = Input.IsActionPressed("LevelCamera");

                    // Mouse pan (RMB)
                    if (Input.IsMouseButtonPressed(MouseButton.Right))
                    {
                        var mouseDelta = Input.GetLastMouseVelocity();
                        _controllerPanX = mouseDelta.X * _mousePanSpeed * (float)delta;
                        _controllerPanY = mouseDelta.Y * _mousePanSpeed * (float)delta;
                    }
                    else
                    {
                        _controllerPanX = 0f;
                        _controllerPanY = 0f;
                    }
                }
                else if (_currentBindings == "Controller")
                {
                    // DPad: Buttons 11-14
                    _controllerMoveForward = Input.IsJoyButtonPressed(0, JoyButton.DpadUp); // DPad Up
                    _controllerMoveBack = Input.IsJoyButtonPressed(0, JoyButton.DpadDown); // DPad Down
                    _controllerStrafeLeft = Input.IsJoyButtonPressed(0, JoyButton.DpadLeft); // DPad Left
                    _controllerStrafeRight = Input.IsJoyButtonPressed(0, JoyButton.DpadRight); // DPad Right

                    // LS: Axes 0/1 (Pan)
                    _controllerPanX = GetJoyAxisWithDeadzone(0, JoyAxis.LeftX, 0.2f);
                    _controllerPanY = GetJoyAxisWithDeadzone(0, JoyAxis.LeftY, 0.2f);

                    // RT/LT: Axes 5/4 (Zoom)
                    float rt = GetJoyAxisWithDeadzone(0, JoyAxis.TriggerRight, 0.2f);
                    float lt = GetJoyAxisWithDeadzone(0, JoyAxis.TriggerLeft, 0.2f);
                    _controllerZoom = rt - lt;

                    // RS: Axes 2/3 (Pointer)
                    float rsX = GetJoyAxisWithDeadzone(0, JoyAxis.RightX, 0.2f);
                    float rsY = GetJoyAxisWithDeadzone(0, JoyAxis.RightY, 0.2f);
                    if (Math.Abs(rsX) > 0.01f || Math.Abs(rsY) > 0.01f)
                    {
                        _controllerPointerX = rsX;
                        _controllerPointerY = rsY;
                        _pointerPosition += new Vector2(_controllerPointerX, _controllerPointerY) * _pointerSpeed * (float)delta;
                        ClampPointerToViewport();
                    }

                    // LS press (Button 7): Level Camera
                    _controllerLevelCamera = Input.IsJoyButtonPressed(0, JoyButton.LeftStick);

                    // RS press (Button 8): Select Unit
                    _controllerSelectUnit = Input.IsJoyButtonPressed(0, JoyButton.RightStick);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager _Process error: {ex.Message}");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            try
            {
                if (_currentBindings == "KBM")
                {
                    Vector3 move = Vector3.Zero;
                    if (_kbmMoveForward) move.Z -= 1f;
                    if (_kbmMoveBack) move.Z += 1f;
                    if (_kbmStrafeLeft) move.X -= 1f;
                    if (_kbmStrafeRight) move.X += 1f;
                    if (move != Vector3.Zero)
                    {
                        EmitSignal(SignalName.CameraMove, move * _moveSpeed * (float)delta);
                        GD.Print($"CameraMove: {move * _moveSpeed * (float)delta}");
                    }

                    float yaw = 0f, pitch = 0f;
                    if (_kbmPanLeft) yaw -= 1f;
                    if (_kbmPanRight) yaw += 1f;
                    if (_kbmPanUp) pitch += 1f;
                    if (_kbmPanDown) pitch -= 1f;
                    if (yaw != 0f || pitch != 0f)
                    {
                        EmitSignal(SignalName.CameraPan, yaw * _panSpeed * (float)delta, pitch * _panSpeed * (float)delta);
                        GD.Print($"CameraPan: yaw {yaw * _panSpeed * (float)delta}, pitch {pitch * _panSpeed * (float)delta}");
                    }

                    if (_kbmZoomIn)
                    {
                        EmitSignal(SignalName.CameraZoom, 1f * _zoomSpeed * (float)delta * 20f);
                        GD.Print($"CameraZoom: {1f * _zoomSpeed * (float)delta * 20f}");
                    }
                    if (_kbmZoomOut)
                    {
                        EmitSignal(SignalName.CameraZoom, -1f * _zoomSpeed * (float)delta * 20f);
                        GD.Print($"CameraZoom: {-1f * _zoomSpeed * (float)delta * 20f}");
                    }
                    if (_kbmLevelCamera)
                    {
                        EmitSignal(SignalName.CameraLevel);
                        GD.Print("CameraLevel");
                    }
                    if (Math.Abs(_controllerPanX) > 0.01f || Math.Abs(_controllerPanY) > 0.01f)
                    {
                        EmitSignal(SignalName.CameraPan, _controllerPanX, _controllerPanY);
                        GD.Print($"CameraPan (mouse): {_controllerPanX}, {_controllerPanY}");
                    }
                }
                else if (_currentBindings == "Controller")
                {
                    Vector3 move = Vector3.Zero;
                    if (_controllerMoveForward) move.Z -= 1f;
                    if (_controllerMoveBack) move.Z += 1f;
                    if (_controllerStrafeLeft) move.X -= 1f;
                    if (_controllerStrafeRight) move.X += 1f;
                    if (move != Vector3.Zero)
                    {
                        EmitSignal(SignalName.CameraMove, move * _moveSpeed * (float)delta);
                        GD.Print($"CameraMove: {move * _moveSpeed * (float)delta}");
                    }

                    if (Math.Abs(_controllerPanX) > 0.01f || Math.Abs(_controllerPanY) > 0.01f)
                    {
                        EmitSignal(SignalName.CameraPan, _controllerPanX * _panSpeed * (float)delta, _controllerPanY * _panSpeed * (float)delta);
                        GD.Print($"CameraPan: {_controllerPanX * _panSpeed * (float)delta}, {_controllerPanY * _panSpeed * (float)delta}");
                    }

                    if (Math.Abs(_controllerZoom) > 0.01f)
                    {
                        EmitSignal(SignalName.CameraZoom, _controllerZoom * _zoomSpeed * (float)delta * 20f);
                        GD.Print($"CameraZoom: {_controllerZoom * _zoomSpeed * (float)delta * 20f}");
                    }

                    if (_controllerLevelCamera)
                    {
                        EmitSignal(SignalName.CameraLevel);
                        GD.Print("CameraLevel");
                    }

                    if (Math.Abs(_controllerPointerX) > 0.01f || Math.Abs(_controllerPointerY) > 0.01f)
                    {
                        EmitSignal(SignalName.PointerMove, _pointerPosition);
                        GD.Print($"PointerMove: {_pointerPosition}");
                    }

                    if (_controllerSelectUnit)
                    {
                        EmitSignal(SignalName.SelectUnit, _pointerPosition);
                        GD.Print($"SelectUnit: {_pointerPosition}");
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager _PhysicsProcess error: {ex.Message}");
            }
        }

        // --- Methods ---

        public object GetInputAction(string action)
        {
            try
            {
                switch (action)
                {
                    case "MoveForward":
                        return _currentBindings == "KBM" ? _kbmMoveForward : _controllerMoveForward;
                    case "MoveBack":
                        return _currentBindings == "KBM" ? _kbmMoveBack : _controllerMoveBack;
                    case "StrafeLeft":
                        return _currentBindings == "KBM" ? _kbmStrafeLeft : _controllerStrafeLeft;
                    case "StrafeRight":
                        return _currentBindings == "KBM" ? _kbmStrafeRight : _controllerStrafeRight;
                    case "PanX":
                        return _currentBindings == "KBM" ? 0f : _controllerPanX;
                    case "PanY":
                        return _currentBindings == "KBM" ? 0f : _controllerPanY;
                    case "Zoom":
                        return _currentBindings == "KBM" ? (_kbmZoomIn ? 1f : _kbmZoomOut ? -1f : 0f) : _controllerZoom;
                    case "PointerX":
                        return _controllerPointerX;
                    case "PointerY":
                        return _controllerPointerY;
                    case "LevelCamera":
                        return _currentBindings == "KBM" ? _kbmLevelCamera : _controllerLevelCamera;
                    case "SelectUnit":
                        return _controllerSelectUnit;
                    default:
                        GD.PrintErr($"GetInputAction: Unknown action '{action}'");
                        return null;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"GetInputAction error: {ex.Message}");
                return null;
            }
        }

        public void SetBindings(string mode)
        {
            GD.Print($"SetBindings entry: {mode}");
            try
            {
                if (mode == "KBM" || mode == "Controller")
                {
                    _currentBindings = mode;
                    GD.Print($"Bindings set to {mode}");
                }
                else
                {
                    GD.PrintErr($"SetBindings: Invalid mode '{mode}'");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SetBindings error: {ex.Message}");
            }
            GD.Print("SetBindings exit");
        }

        public void SetSpeed(string type, float value)
        {
            GD.Print($"SetSpeed entry: {type}={value}");
            try
            {
                switch (type)
                {
                    case "MoveSpeed":
                        _moveSpeed = Mathf.Clamp(value, 10f, 60f);
                        break;
                    case "PanSpeed":
                        _panSpeed = Mathf.Clamp(value, 0.1f, 1.0f);
                        break;
                    case "MousePanSpeed":
                        _mousePanSpeed = Mathf.Clamp(value, 0.1f, 0.5f);
                        break;
                    case "PointerSpeed":
                        _pointerSpeed = Mathf.Clamp(value, 300f, 800f);
                        break;
                    case "CursorSpeed":
                        _cursorSpeed = Mathf.Clamp(value, 200f, 1000f);
                        break;
                    default:
                        GD.PrintErr($"SetSpeed: Unknown type '{type}'");
                        return;
                }
                GD.Print($"Set {type} to {value}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SetSpeed error: {ex.Message}");
            }
            GD.Print("SetSpeed exit");
        }

        // --- Helpers ---

        private float GetJoyAxisWithDeadzone(int device, JoyAxis axis, float deadzone)
        {
            try
            {
                GD.Print($"GetJoyAxisWithDeadzone: device={device}, axis={axis}, deadzone={deadzone}");
                float value = Input.GetJoyAxis(device, axis);
                float result = Math.Abs(value) < deadzone ? 0f : value;
                GD.Print($"GetJoyAxisWithDeadzone: value={value}, result={result}");
                return result;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"GetJoyAxisWithDeadzone error: {ex.Message}");
                return 0f;
            }
        }

        private void ClampPointerToViewport()
        {
            var viewport = GetViewport();
            if (viewport != null)
            {
                var size = viewport.GetVisibleRect().Size;
                _pointerPosition.X = Mathf.Clamp(_pointerPosition.X, 0, size.X);
                _pointerPosition.Y = Mathf.Clamp(_pointerPosition.Y, 0, size.Y);
            }
        }

        // Commented-out TerrainTest.cs drag logic (retained for reference)
        // LMB drag: Raycast to AC2, plane intersection at Y=10, moves _ac2Node
    }
}