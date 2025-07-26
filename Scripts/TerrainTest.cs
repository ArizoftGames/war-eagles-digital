using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace WarEaglesDigital.Scripts
{
    public partial class TerrainTest : Node3D
    {
        private Camera3D _camera;
        private ConfigFile _config = new();
        private string _configPath = "user://War Eagles/config.cfg";
        private string _controllerBindings = "KBM";
        private int _controllerId = -1;
        private string _controllerName = "";
        private string _controllerGuid = "";
        private Dictionary<string, int> _buttonMap = new();
        private Dictionary<string, int> _axisMap = new();
        private float _moveSpeed = 5.0f;
        private float _panSpeed = 0.5f;
        private float _zoomSpeed = 0.1f;
        private Node3D _ac2Node;
        private CollisionShape3D _ac2Collider;
        private GameManager _gameManager;
        private float _lastLogTime = 0f;
        private const float LogInterval = 1f; // Log every 1 second

        public override void _Ready()
        {
            try
            {
                // Camera
                _camera = GetNodeOrNull<Camera3D>("Camera3D");
                if (_camera == null)
                {
                    GD.PrintErr("Camera3D not found in terrain_test.tscn. Please add one.");
                    return;
                }

                // Config
                if (FileAccess.FileExists(_configPath))
                {
                    var err = _config.Load(_configPath);
                    if (err != Error.Ok)
                        GD.PrintErr($"Failed to load config: {_configPath}");
                }
                else
                {
                    GD.PrintErr($"Config file not found: {_configPath}");
                }
                if (_config.HasSectionKey("Controller", "Bindings"))
                    _controllerBindings = _config.GetValue("Controller", "Bindings", "KBM").AsString();

                // Controller detection
                // In _Ready(), replace controller detection (lines ~65-85)
                var joypads = Input.GetConnectedJoypads();
                GD.Print($"Connected joypads: {string.Join(", ", joypads)}");
                foreach (var id in joypads)
                {
                    var name = Input.GetJoyName(id);
                    var guid = Input.GetJoyGuid(id);
                    GD.Print($"Device {id}: Name={name}, GUID={guid}");
                    if (_controllerBindings == "Controller")
                    {
                        _controllerId = id; // Use Device 0 or first controller
                        _controllerName = name;
                        _controllerGuid = guid;
                        GD.Print($"Controller selected: Device {id}, Name={name}, GUID={guid}");
                        break;
                    }
                }
                if (_controllerId == -1)
                {
                    GD.Print("No controller selected. Fallback to KBM.");
                }
                /*if (_controllerId == -1)
                {
                    GD.Print("No EchtPower EP-01 detected. Fallback to KBM.");
                }*/

                // KeyBindings (hardcoded for now, can be loaded from CSV)
                _buttonMap["MoveForward"] = 11; // DPad Up
                _buttonMap["MoveBack"] = 12;    // DPad Down
                _buttonMap["StrafeLeft"] = 13;  // DPad Left
                _buttonMap["StrafeRight"] = 14; // DPad Right
                _buttonMap["SelectUnit"] = 8;   // RS
                _buttonMap["PauseGame"] = 4;    // Back
                _buttonMap["QuitGame"] = 5;     // Home
                _buttonMap["ScreenShot"] = 6;   // Start

                _axisMap["PanX"] = 0;           // LS X
                _axisMap["PanY"] = 1;           // LS Y
                _axisMap["ZoomIn"] = 5;         // RT
                _axisMap["ZoomOut"] = 4;        // LT

                // GameManager
                _gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
                if (_gameManager == null)
                    GD.PrintErr("GameManager not found at /root/GameManager.");

                // AC2 node and collider
                _ac2Node = GetNodeOrNull<Node3D>("PZone0/AC2");
                if (_ac2Node != null)
                {
                    _ac2Collider = new CollisionShape3D();
                    var boxShape = new BoxShape3D();
                    boxShape.Size = new Vector3(2, 2, 6); // Example size, adjust as needed
                    _ac2Collider.Shape = boxShape;
                    _ac2Node.AddChild(_ac2Collider);
                    GD.Print("CollisionShape3D added to AC2.");
                }
                else
                {
                    GD.PrintErr("AC2 node not found under PZone0.");
                }

                GD.Print("TerrainTest ready. Controller input initialized.");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in _Ready: {e.Message}");
            }
        }

        public override void _Process(double delta)
        {
            try
            {
                var joypads = Input.GetConnectedJoypads();
                bool useController = _controllerId != -1 && joypads.Contains(_controllerId);
                if (!useController)
                {
                    if (Time.GetTicksMsec() / 1000f - _lastLogTime >= LogInterval)
                    {
                        GD.Print("No EchtPower EP-01 detected. Using KBM fallback.");
                        _lastLogTime = Time.GetTicksMsec() / 1000f;
                    }
                    return;
                }

                // Camera movement (DPad)
                float moveForward = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["MoveForward"]) ? 1f : 0f;
                float moveBack = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["MoveBack"]) ? 1f : 0f;
                float strafeLeft = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["StrafeLeft"]) ? 1f : 0f;
                float strafeRight = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["StrafeRight"]) ? 1f : 0f;

                float forward = moveForward - moveBack;
                float strafe = strafeRight - strafeLeft;

                Vector3 movement = new Vector3(strafe, 0, forward) * _moveSpeed * (float)delta;
                _camera.Translate(movement);

                // Camera pan (LS)
                float panX = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PanX"]);
                float panY = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PanY"]);

                if (Math.Abs(panX) > 0.2f || Math.Abs(panY) > 0.2f)
                {
                    Vector3 pan = new Vector3(panX, -panY, 0) * _panSpeed * (float)delta;
                    _camera.RotateX(pan.Y);
                    _camera.RotateY(pan.X);
                }

                // Camera zoom (RT/LT)
                float zoomIn = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["ZoomIn"]);
                float zoomOut = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["ZoomOut"]);

                float zoom = 0f;
                if (zoomIn > 0.2f) zoom += zoomIn;
                if (zoomOut > 0.2f) zoom -= zoomOut;

                _camera.Translate(new Vector3(0, 0, zoom * _zoomSpeed * (float)delta));
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
                if (@event is InputEventJoypadButton btnEvent)
                {
                    GD.Print($"Controller event: Button {btnEvent.ButtonIndex} Pressed={btnEvent.Pressed}");

                    if (btnEvent.Device == _controllerId && btnEvent.Pressed)
                    {
                        switch (btnEvent.ButtonIndex)
                        {
                            case JoyButton.Back: // Button 4
                                _gameManager?.PauseGame();
                                GD.Print("PauseGame triggered.");
                                break;
                            case JoyButton.Guide: // Button 5
                                GetTree().Quit();
                                GD.Print("QuitGame triggered.");
                                break;
                            case JoyButton.Start: // Button 6
                                _gameManager?.ScreenShot();
                                GD.Print("ScreenShot triggered.");
                                break;
                            case JoyButton.RightStick: // Button 8
                                RaycastToAC2();
                                break;
                        }
                    }
                }
                else if (@event is InputEventJoypadMotion motionEvent)
                {
                    GD.Print($"Controller axis: Axis {motionEvent.Axis} Value={motionEvent.AxisValue}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in _Input: {e.Message}");
            }
        }

        private void RaycastToAC2()
        {
            try
            {
                var spaceState = GetWorld3D().DirectSpaceState;
                var from = _camera.GlobalTransform.Origin;
                var to = from + _camera.GlobalTransform.Basis.Z * -100f; // Forward direction

                // Create query parameters
                var query = PhysicsRayQueryParameters3D.Create(from, to);

                // Perform raycast
                var result = spaceState.IntersectRay(query);

                if (result.Count > 0 && result.ContainsKey("collider"))
                {
                    var collider = result["collider"];
                    GD.Print($"Raycast hit: {collider}");
                }
                else
                {
                    GD.Print("Raycast did not hit AC2.");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in RaycastToAC2: {e.Message}");
            }
        }
    }
}