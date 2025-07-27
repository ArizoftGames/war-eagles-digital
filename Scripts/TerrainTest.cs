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
        private string _controllerBindings = "Controller"; // Hardcoded for testbed
        private int _controllerId = -1;
        private string _controllerName = "";
        private string _controllerGuid = "";
        private readonly Dictionary<string, int> _buttonMap = [];
        private readonly Dictionary<string, int> _axisMap = [];
        private float _moveSpeed = 35.0f; // Set for responsive movement
        private float _panSpeed = 0.5f;
        private float _zoomSpeed = 1.0f; // Multiplier for FOV changes
        private float _pointerSpeed = 400.0f; // Pixels per second for RS pointer movement
        private Node3D _ac2Node;
        private StaticBody3D _ac2Static;
        private GameManager _gameManager;
        private Sprite2D _pointer;
        private float _lastLogTime = 0f;
        private const float LogInterval = 1f; // Log every 1 second
        private float _defaultFov = 70.0f; // Default camera FOV
        // Input state for physics process
        private float _moveForward;
        private float _moveBack;
        private float _strafeLeft;
        private float _strafeRight;
        private float _panX;
        private float _panY;
        private float _zoomIn;
        private float _zoomOut;
        private float _pointerX;
        private float _pointerY;
        private bool _raycastRequested;
        private Vector2 _raycastPointerPosition;
        private bool _isDragging;
        private Node3D _draggedNode;
        private bool _dragInitiated;

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
                _defaultFov = _camera.Fov; // Store default FOV

                // Pointer
                _pointer = new Sprite2D
                {
                    Name = "Pointer",
                    Texture = GD.Load<Texture2D>("res://Assets/Sprites/UI_Elements/crosshair1_64.png"),
                    Position = GetViewport().GetVisibleRect().Size / 2, // Center initially
                    ZIndex = 0 // Layer 0
                };
                AddChild(_pointer);
                GD.Print("Pointer Sprite2D added to root.");

                // Controller detection
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
                _buttonMap["ResetCamera"] = 7;  // LS

                _axisMap["PanX"] = 0;           // LS X
                _axisMap["PanY"] = 1;           // LS Y
                _axisMap["ZoomIn"] = 5;         // RT
                _axisMap["ZoomOut"] = 4;        // LT
                _axisMap["PointerX"] = 2;       // RS X
                _axisMap["PointerY"] = 3;       // RS Y

                // GameManager
                _gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
                if (_gameManager == null)
                    GD.PrintErr("GameManager not found at /root/GameManager.");

                // AC2 node and collider
                _ac2Node = GetNodeOrNull<Node3D>("PZone0/AC2");
                if (_ac2Node != null)
                {
                    _ac2Static = new StaticBody3D
                    {
                        Name = "AC2Static",
                        Transform = new Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 7, 0)
                    };
                    var ac2Collision = new CollisionShape3D
                    {
                        Name = "AC2Collision",
                        Shape = new BoxShape3D { Size = new Vector3(30, 10, 30) }
                    };
                    _ac2Static.AddChild(ac2Collision);
                    _ac2Node.AddChild(_ac2Static);
                    _ac2Static.CollisionLayer = 1; // Ensure on layer 1
                    _ac2Static.CollisionMask = 1;
                    GD.Print("AC2Static and AC2Collision added to PZone0/AC2.");
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
                    return; // Fallback to KBM silently
                }

                // Poll inputs for physics process
                _moveForward = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["MoveForward"]) ? 1f : 0f;
                _moveBack = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["MoveBack"]) ? 1f : 0f;
                _strafeLeft = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["StrafeLeft"]) ? 1f : 0f;
                _strafeRight = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["StrafeRight"]) ? 1f : 0f;

                _panX = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PanX"]);
                _panY = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PanY"]);

                _zoomIn = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["ZoomIn"]);
                _zoomOut = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["ZoomOut"]);

                // Pointer movement (RS)
                _pointerX = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PointerX"]);
                _pointerY = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PointerY"]);
                if (Math.Abs(_pointerX) > 0.2f || Math.Abs(_pointerY) > 0.2f)
                {
                    Vector2 pointerDelta = new Vector2(_pointerX, _pointerY) * _pointerSpeed * (float)delta;
                    _pointer.Position += pointerDelta;
                    // Clamp to viewport bounds
                    var viewportSize = GetViewport().GetVisibleRect().Size;
                    _pointer.Position = _pointer.Position.Clamp(Vector2.Zero, viewportSize);
                    //GD.Print($"Pointer: Position={_pointer.Position}, DeltaX={_pointerX}, DeltaY={_pointerY}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in _Process: {e.Message}");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            try
            {
                if (_camera == null) return;

                // Camera movement (DPad)
                float forward = _moveBack - _moveForward; // Inverted for intuitive Up=forward, Down=back
                float strafe = _strafeRight - _strafeLeft;
                Vector3 movement = new Vector3(strafe, 0, forward) * _moveSpeed * (float)delta;
                _camera.Translate(movement);

                // Camera pan (LS)
                if (Math.Abs(_panX) > 0.2f || Math.Abs(_panY) > 0.2f)
                {
                    _camera.RotateY(-_panX * _panSpeed * (float)delta); // Inverted for LS left=yaw left, right=yaw right
                    _camera.RotateX(_panY * _panSpeed * (float)delta);  // Inverted for LS up=pitch down, down=pitch up
                    //GD.Print($"Pan: panX={_panX}, panY={_panY}");
                }

                // Camera zoom (LT/RT)
                if (_zoomIn > 0.2f || _zoomOut > 0.2f)
                {
                    float zoom = _zoomIn - _zoomOut;
                    float newFov = _camera.Fov - zoom * _zoomSpeed * (float)delta * 20f; // Tuned for ~10-15 deg/sec
                    newFov = Mathf.Clamp(newFov, 30f, 100f); // Limit FOV range
                    _camera.Fov = newFov;
                    GD.Print($"Zoom: zoomIn={_zoomIn}, zoomOut={_zoomOut}, NewFov={newFov}");
                }

                // Dragging
                if (_isDragging && _draggedNode != null)
                {
                    var plane = new Plane(Vector3.Up, 7.158859f); // Plane at Y=7.158859
                    var from = _camera.ProjectRayOrigin(_pointer.Position);
                    var to = from + _camera.ProjectRayNormal(_pointer.Position) * 500f;
                    var intersection = plane.IntersectsRay(from, to);
                    if (intersection.HasValue)
                    {
                        // Smooth movement with Lerp
                        var targetPos = intersection.Value;
                        _draggedNode.GlobalPosition = _draggedNode.GlobalPosition.Lerp(targetPos, 10f * (float)delta);
                        GD.Print($"Dragging: {_draggedNode.Name} to {_draggedNode.GlobalPosition}");
                    }
                    else
                    {
                        GD.Print("Dragging: No plane intersection.");
                    }
                }

                // Raycast if requested
                if (_raycastRequested)
                {
                    _raycastRequested = false;
                    var hitNode = RaycastToAC2(_raycastPointerPosition);
                    if (_dragInitiated && hitNode != null && hitNode == _ac2Static)
                    {
                        _isDragging = true;
                        _draggedNode = _ac2Node;
                        GD.Print($"Dragging started: {_draggedNode.Name}");
                    }
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in _PhysicsProcess: {e.Message}");
            }
        }

        public override void _Input(InputEvent @event)
        {
            try
            {
                if (@event is InputEventJoypadButton btnEvent)
                {
                    GD.Print($"Controller event: Button {btnEvent.ButtonIndex} Pressed={btnEvent.Pressed}");

                    if (btnEvent.Device == _controllerId)
                    {
                        if (btnEvent.ButtonIndex == (JoyButton)_buttonMap["SelectUnit"])
                        {
                            if (btnEvent.Pressed)
                            {
                                _raycastRequested = true;
                                _raycastPointerPosition = _pointer.Position;
                                _dragInitiated = true;
                            }
                            else
                            {
                                _isDragging = false;
                                _dragInitiated = false;
                                if (_draggedNode != null)
                                {
                                    GD.Print($"Dragging stopped: {_draggedNode.Name}");
                                    _draggedNode = null;
                                }
                            }
                        }
                        else if (btnEvent.Pressed)
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
                                case JoyButton.LeftStick: // Button 7
                                    _camera.Rotation = new Vector3(_camera.Rotation.X, _camera.Rotation.Y, 0); // Reset Z-rotation
                                    GD.Print($"Camera Z-rotation reset to 0. Current rotation: {_camera.Rotation}");
                                    break;
                            }
                        }
                    }
                }
                else if (@event is InputEventJoypadMotion motionEvent)
                {
                    //GD.Print($"Controller axis: Axis {motionEvent.Axis} Value={motionEvent.AxisValue}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in _Input: {e.Message}");
            }
        }

        private Node RaycastToAC2(Vector2 pointerPosition)
        {
            try
            {
                var spaceState = GetWorld3D().DirectSpaceState;
                if (spaceState == null)
                {
                    GD.PrintErr("DirectSpaceState is null in RaycastToAC2!");
                    return null;
                }
                // Project pointer position to 3D ray
                var from = _camera.ProjectRayOrigin(pointerPosition);
                var to = from + _camera.ProjectRayNormal(pointerPosition) * 500f; // Standardized ray length
                // Log ray and AC2 position for debugging
                GD.Print($"Raycast: From={from}, To={to}, AC2 Position={_ac2Static?.GlobalPosition ?? Vector3.Zero}");

                // Create query parameters with default collision mask
                var query = PhysicsRayQueryParameters3D.Create(from, to);
                query.CollisionMask = 1; // Layer 1

                // Perform raycast
                var result = spaceState.IntersectRay(query);

                if (result.Count > 0 && result.ContainsKey("collider"))
                {
                    var collider = result["collider"].As<GodotObject>();
                    GD.Print($"Raycast hit: {collider} at pointer position {pointerPosition}");
                    return collider as Node;
                }
                else
                {
                    GD.Print($"Raycast did not hit anything at pointer position {pointerPosition}");
                    return null;
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in RaycastToAC2: {e.Message}");
                return null;
            }
        }
    }
}