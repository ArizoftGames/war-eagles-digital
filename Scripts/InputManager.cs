using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts
{
    public partial class InputManager : Node
    {
        private int _controllerId = -1; // -1 for no controller, set to device ID
        private string _controllerName = "";
        private string _controllerGuid = "";
        private bool _controllerActive = false; // Toggled by ControlsMenuPanel.cs
        private Godot.Collections.Dictionary<string, int> _buttonMap = [];
        private Godot.Collections.Dictionary<string, int> _axisMap = [];
        private Sprite2D _pointer;
        private float _panSpeed = 0.5f; // Default from TerrainTest.cs
        private float _moveSpeed = 35.0f;
        private float _pointerSpeed = 600.0f;
        private float _mousePanSpeed = 0.3f;
        private float _cursorSpeed = 600.0f;
        private float _pointerX;
        private float _pointerY;
        private ConfirmationDialog _acceptDialog;
        private int device;
        private bool connected;
        private CanvasLayer _canvasLayer;

        public override void _Ready()
        {
            ProcessMode = Node.ProcessModeEnum.Always;
            try
            {
                // Set custom mouse cursor for controller
                var cursorTexture = GD.Load<Texture2D>("res://Assets/Sprites/UI_Elements/crosshair1_64.png");
                Input.SetCustomMouseCursor(cursorTexture, Input.CursorShape.Arrow, new Vector2(32, 32)); // Center hotspot at 32x32 for 64x64 crosshair
                GD.Print("InputManager: Set custom mouse cursor to crosshair1_64.png");

                // Detect controllers
                var joypads = Input.GetConnectedJoypads();
                GD.Print($"InputManager: Connected joypads: {string.Join(", ", joypads)}");
                foreach (var id in joypads)
                {
                    var name = Input.GetJoyName(id);
                    var guid = Input.GetJoyGuid(id);
                    GD.Print($"InputManager: Device {id}: Name={name}, GUID={guid}");
                    if (name != null && (name.Contains("Xbox", StringComparison.OrdinalIgnoreCase) || name.Contains("XInput", StringComparison.OrdinalIgnoreCase)))
                    {
                        _controllerId = id;
                        _controllerName = name;
                        _controllerGuid = guid;
                        _controllerActive = true;
                        GD.Print($"InputManager: Controller selected: Device {id}, Name={name}, GUID={guid}");
                        break;
                    }
                }
                if (_controllerId == -1)
                {
                    GD.Print("InputManager: No controller selected. Using KBM");
                }

                // Initialize controller mappings (from KeyBindings.csv)
                _buttonMap["MoveForward"] = 11; // DPad Up
                _buttonMap["MoveBack"] = 12; // DPad Down
                _buttonMap["StrafeLeft"] = 13; // DPad Left
                _buttonMap["StrafeRight"] = 14; // DPad Right
                _buttonMap["SelectUnit"] = 8; // RS
                _buttonMap["PauseGame"] = 4; // Back
                _buttonMap["QuitGame"] = 5; // Home
                _buttonMap["ScreenShot"] = 6; // Start
                _buttonMap["ResetCamera"] = 7; // LS
                _buttonMap["EndPhase"] = 9; // LB
                _buttonMap["EndTurn"] = 10; // RB
                _buttonMap["SkipVoiceover"] = 3; // Y
                _buttonMap["ViewPlayerLosses"] = 0; // A
                _buttonMap["ViewEnemyLosses"] = 1; // B
                _buttonMap["OpenHelpMenu"] = 2; // X

                _axisMap["PanX"] = 0; // LS X
                _axisMap["PanY"] = 1; // LS Y
                _axisMap["ZoomIn"] = 5; // RT
                _axisMap["ZoomOut"] = 4; // LT
                _axisMap["PointerX"] = 2; // RS X
                _axisMap["PointerY"] = 3; // RS Y

                // Stub controller connection handling
                Input.JoyConnectionChanged += (device, connected) =>
                {
                    this.device = (int)device;
                    this.connected = connected;
                    HandleJoyConnectionChanged((int)device, connected);
                };

                // Load settings from config (set by Loading.cs)
                var config = new ConfigFile();
                var configPath = "user://War Eagles/config.cfg";
                if (FileAccess.FileExists(configPath))
                {
                    Error err = config.Load(configPath);
                    if (err == Error.Ok)
                    {
                        LoadControllerSettings(config.GetValue("Controller", "Bindings", "KBM"));
                        LoadGameplaySettings(new Dictionary
                        {
                            ["panspeed"] = config.GetValue("Gameplay", "panspeed", 0.5f),
                            ["movespeed"] = config.GetValue("Gameplay", "movespeed", 35.0f),
                            ["pointerspeed"] = config.GetValue("Gameplay", "pointerspeed", 800.0f), // Median speed for WarpMouse, adjustable
                            ["mousepanspeed"] = config.GetValue("Gameplay", "mousepanspeed", 0.3f),
                            ["cursorspeed"] = config.GetValue("Gameplay", "cursorspeed", 600.0f)
                        });
                        GD.Print("InputManager: Loaded settings from config");
                    }
                    else
                    {
                        GD.PrintErr($"InputManager: Failed to load config: {err}");
                    }
                }
                else
                {
                    GD.Print("InputManager: Config file not found, using defaults");
                }

                // Connect to ControlsMenuPanel.Accept (NYI)
                // TODO: Connect to ControlsMenuPanel.Accept signal when implemented
                GD.Print("InputManager: Ready, awaiting ControlsMenuPanel signal connection");

                // Archived CanvasLayer/Sprite2D logic
                /*
                // Initialize controller pointer on CanvasLayer
                _canvasLayer = new CanvasLayer
                {
                    Layer = 0, // Layer 0 per Guidelines.md
                    ProcessMode = CanvasLayer.ProcessModeEnum.Always,
                };
                AddChild(_canvasLayer);
                _pointer = new Sprite2D
                {
                    Name = "Pointer",
                    Texture = GD.Load<Texture2D>("res://Assets/Sprites/UI_Elements/crosshair1_64.png"),
                    Position = GetViewport().GetVisibleRect().Size / 2, // Center initially
                    ZIndex = 0, // Layer 0 for pointer
                    Visible = true
                };
                _canvasLayer.AddChild(_pointer);
                GD.Print("InputManager: Pointer Sprite2D added to CanvasLayer at Layer 0");
                */
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager: Error in _Ready: {ex.Message}");
            }
        }

        public override void _Process(double delta)
        {
            try
            {
                if (!_controllerActive || _controllerId == -1)
                    return;

                // Update cursor position using RS inputs (Axis 2/3)
                float pointerX = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PointerX"]);
                float pointerY = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PointerY"]);

                // Apply deadzone to prevent drift
                if (Math.Abs(pointerX) < 0.2f) pointerX = 0f;
                if (Math.Abs(pointerY) < 0.2f) pointerY = 0f;

                if (Math.Abs(pointerX) > 0f || Math.Abs(pointerY) > 0f)
                {
                    // Median pointer speed for WarpMouse, adjustable for OS mouse sensitivity
                    const float pointerSpeed = 800.0f; // Tuned for WarpMouse, tweak as needed
                    const float smoothingFactor = 0.1f; // Smoothing to reduce jitter, tweak as needed
                    Vector2 currentPos = GetViewport().GetMousePosition();
                    Vector2 deltaPos = new Vector2(pointerX, pointerY) * pointerSpeed * (float)delta;

                    // Apply smoothing
                    Vector2 smoothedDelta = deltaPos.Lerp(Vector2.Zero, smoothingFactor);
                    Vector2 newPos = currentPos + smoothedDelta;

                    // Clamp to viewport bounds
                    var viewportSize = GetViewport().GetVisibleRect().Size;
                    newPos = newPos.Clamp(Vector2.Zero, viewportSize);

                    // Move cursor
                    Input.WarpMouse(newPos);
                    GD.Print($"InputManager: Warped mouse to {newPos}, Delta: {smoothedDelta}, Raw Input: ({pointerX}, {pointerY})");
                }

                // Archived Sprite2D/CanvasLayer logic
                /*
                // Update controller pointer position (RS Axis 2/3)
                _pointerX = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PointerX"]);
                _pointerY = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PointerY"]);
                if (Math.Abs(_pointerX) > 0.2f || Math.Abs(_pointerY) > 0.2f)
                {
                    Vector2 pointerDelta = new Vector2(_pointerX, _pointerY) * _pointerSpeed * (float)delta;
                    _pointer.Position += pointerDelta;
                    // Clamp to viewport bounds
                    var viewportSize = GetViewport().GetVisibleRect().Size;
                    _pointer.Position = _pointer.Position.Clamp(Vector2.Zero, viewportSize);
                    GD.Print($"InputManager: Pointer moved to {_pointer.Position}");
                }
                */
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager: Error in _Process: {ex.Message}");
            }
        }

        public override void _Input(InputEvent @event)
        {
            try
            {
                if (!_controllerActive || _controllerId == -1)
                    return;

                if (@event is InputEventJoypadButton btnEvent && btnEvent.Device == _controllerId)
                {
                    GD.Print($"InputManager: Controller button {btnEvent.ButtonIndex} Pressed={btnEvent.Pressed}");
                    var gameManager = GetNodeOrNull<Node>("/root/GameManager");
                    if (gameManager == null)
                    {
                        GD.PrintErr("InputManager: GameManager not found at /root/GameManager");
                        return;
                    }

                    if (btnEvent.Pressed)
                    {
                        switch (btnEvent.ButtonIndex)
                        {
                            case JoyButton.Back when (int)btnEvent.ButtonIndex == _buttonMap["PauseGame"]: // Back: 4
                                gameManager.Call("PauseGame");
                                GD.Print("InputManager: Called PauseGame");
                                break;
                            case JoyButton.Guide when (int)btnEvent.ButtonIndex == _buttonMap["QuitGame"]: // Guide: 5
                                gameManager.Call("QuitGame");
                                GD.Print("InputManager: Called QuitGame");
                                break;
                            case JoyButton.Start when (int)btnEvent.ButtonIndex == _buttonMap["ScreenShot"]: // Start: 6
                                gameManager.Call("ScreenShot");
                                GD.Print("InputManager: Called ScreenShot");
                                break;
                            case JoyButton.LeftStick when (int)btnEvent.ButtonIndex == _buttonMap["ResetCamera"]: // LS: 7
                                gameManager.Call("CameraLevel");
                                GD.Print("InputManager: Called CameraLevel");
                                break;
                            case JoyButton.LeftShoulder when (int)btnEvent.ButtonIndex == _buttonMap["EndPhase"]: // LB: 9
                                gameManager.Call("EndPhase");
                                GD.Print("InputManager: Called EndPhase");
                                break;
                            case JoyButton.RightShoulder when (int)btnEvent.ButtonIndex == _buttonMap["EndTurn"]: // RB: 10
                                gameManager.Call("EndTurn");
                                GD.Print("InputManager: Called EndTurn");
                                break;
                            case JoyButton.Y when (int)btnEvent.ButtonIndex == _buttonMap["SkipVoiceover"]: // Y: 3
                                gameManager.Call("SkipVoiceover");
                                GD.Print("InputManager: Called SkipVoiceover");
                                break;
                            case JoyButton.A when (int)btnEvent.ButtonIndex == _buttonMap["ViewPlayerLosses"]: // A: 0
                                gameManager.Call("ViewPlayerLossesPool");
                                GD.Print("InputManager: Called ViewPlayerLossesPool");
                                break;
                            case JoyButton.B when (int)btnEvent.ButtonIndex == _buttonMap["ViewEnemyLosses"]: // B: 1
                                gameManager.Call("ViewEnemyLossesPool");
                                GD.Print("InputManager: Called ViewEnemyLossesPool");
                                break;
                            case JoyButton.X when (int)btnEvent.ButtonIndex == _buttonMap["OpenHelpMenu"]: // X: 2
                                gameManager.Call("OpenHelpMenu");
                                GD.Print("InputManager: Called OpenHelpMenu");
                                break;
                            case JoyButton.RightStick when (int)btnEvent.ButtonIndex == _buttonMap["SelectUnit"]: // RS: 8
                                                                                                                  // Emulate LMB click at current mouse position
                                var mousePos = GetViewport().GetMousePosition();
                                var mouseEvent = new InputEventMouseButton
                                {
                                    ButtonIndex = MouseButton.Left,
                                    Pressed = true,
                                    Position = mousePos,
                                    GlobalPosition = GetViewport().GetWindow().GetPosition() + mousePos
                                };
                                Input.ParseInputEvent(mouseEvent);
                                GD.Print($"InputManager: Emulated LMB click at Position={mousePos}, GlobalPosition={mouseEvent.GlobalPosition}");

                                // Release event to complete click
                                mouseEvent = new InputEventMouseButton
                                {
                                    ButtonIndex = MouseButton.Left,
                                    Pressed = false,
                                    Position = mousePos,
                                    GlobalPosition = GetViewport().GetWindow().GetPosition() + mousePos
                                };
                                Input.ParseInputEvent(mouseEvent);
                                GD.Print($"InputManager: Emulated LMB release at Position={mousePos}, GlobalPosition={mouseEvent.GlobalPosition}");
                                break;
                        }
                    }
                }

                // Archived Sprite2D/CanvasLayer logic
                /*
                if (@event is InputEventJoypadButton btnEvent && btnEvent.Device == _controllerId)
                {
                    GD.Print($"InputManager: Controller button {btnEvent.ButtonIndex} Pressed={btnEvent.Pressed}");
                    var gameManager = GetNodeOrNull<Node>("/root/GameManager");
                    if (gameManager == null)
                    {
                        GD.PrintErr("InputManager: GameManager not found at /root/GameManager");
                        return;
                    }

                    if (btnEvent.Pressed)
                    {
                        switch (btnEvent.ButtonIndex)
                        {
                            case JoyButton.RightStick when (int)btnEvent.ButtonIndex == _buttonMap["SelectUnit"]: // RS: 8
                                // Emulate mouse motion for hover
                                var motionEvent = new InputEventMouseMotion
                                {
                                    Position = _pointer.Position,
                                    GlobalPosition = _pointer.GlobalPosition
                                };
                                Input.ParseInputEvent(motionEvent);
                                GD.Print($"InputManager: Emulated mouse motion at Position={_pointer.Position}, GlobalPosition={_pointer.GlobalPosition}");

                                // Emulate mouse click
                                var mouseEvent = new InputEventMouseButton
                                {
                                    ButtonIndex = MouseButton.Left,
                                    Pressed = true,
                                    Position = _pointer.Position,
                                    GlobalPosition = _pointer.GlobalPosition
                                };
                                Input.ParseInputEvent(mouseEvent);
                                GD.Print($"InputManager: Emulated mouse click at Position={_pointer.Position}, GlobalPosition={_pointer.GlobalPosition}");

                                // Debug: Check if button under pointer is hit
                                // var control = GetViewport().GuiGetFocusOwner() ?? GetViewport().GetNodeOrNull<Control>("PauseMenu/Pause_Menu");
                                // if (control != null)
                                // {
                                //     GD.Print($"InputManager: Focus owner or PauseMenu found: {control.Name}");
                                //     var localPos = control.GetGlobalRect().HasPoint(_pointer.GlobalPosition);
                                //     if (localPos)
                                //     {
                                //         GD.Print($"InputManager: Pointer at {_pointer.GlobalPosition} is over {control.Name}");
                                //     }
                                //     else
                                //     {
                                //         GD.Print($"InputManager: Pointer at {_pointer.GlobalPosition} is NOT over {control.Name}");
                                //     }
                                // }
                                // else
                                // {
                                //     GD.PrintErr("InputManager: No focus owner or PauseMenu found for emulated click");
                                // }
                                break;
                        }
                    }
                }
                */
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager: Error in _Input: {ex.Message}");
            }
        }

        public void GetCameraInputs(out Vector3 movement, out float yaw, out float pitch, out float zoom)
        {
            try
            {
                movement = Vector3.Zero;
                yaw = 0f;
                pitch = 0f;
                zoom = 0f;

                if (!_controllerActive || _controllerId == -1)
                    return;

                // Movement (DPad)
                float moveForward = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["MoveForward"]) ? 1f : 0f;
                float moveBack = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["MoveBack"]) ? 1f : 0f;
                float strafeLeft = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["StrafeLeft"]) ? 1f : 0f;
                float strafeRight = Input.IsJoyButtonPressed(_controllerId, (JoyButton)_buttonMap["StrafeRight"]) ? 1f : 0f;
                movement = new Vector3(strafeRight - strafeLeft, 0, moveBack - moveForward); // Inverted for intuitive Up=forward

                // Pan (LS)
                yaw = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PanX"]);
                pitch = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["PanY"]);

                // Zoom (LT/RT)
                float zoomIn = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["ZoomIn"]);
                float zoomOut = Input.GetJoyAxis(_controllerId, (JoyAxis)_axisMap["ZoomOut"]);
                zoom = zoomIn - zoomOut;

                GD.Print($"InputManager: Camera inputs - Movement: {movement}, Yaw: {yaw}, Pitch: {pitch}, Zoom: {zoom}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager: Error in GetCameraInputs: {ex.Message}");
                movement = Vector3.Zero;
                yaw = 0f;
                pitch = 0f;
                zoom = 0f;
            }
        }

        public void SetControllerActive(bool active)
        {
            try
            {
                _controllerActive = active;
                var config = new ConfigFile();
                var configPath = "user://War Eagles/config.cfg";
                string bindings = active ? "Default" : "KBM";
                config.SetValue("Controller", "Bindings", bindings);
                Error err = config.Save(configPath);
                if (err != Error.Ok)
                {
                    GD.PrintErr($"InputManager: Failed to save config: {err}");
                }
                else
                {
                    GD.Print($"InputManager: Updated config Bindings to {bindings}");
                }
                // Emit ControllerStateChanged for ControlsMenuPanel (NYI)
                // TODO: Emit ControllerStateChanged when ControlsMenuPanel is implemented
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager: Error in SetControllerActive: {ex.Message}");
            }
        }

        private void HandleJoyConnectionChanged(int device, bool connected)
        {
            try
            {
                if (connected)
                {
                    string joyName = Input.GetJoyName(device) ?? "Unknown Controller";
                    _acceptDialog = new ConfirmationDialog
                    {
                        Title = "New Controller Detected",
                        DialogText = $"Use {joyName}?",
                        //Theme = GD.Load<Theme>("res://Data/Resources/UI_Theme.tres"),
                        InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen
                    };
                    _acceptDialog.SetTheme(GD.Load<Theme>("res://Data/Resources/UI_Theme .tres"));
                    AddChild(_acceptDialog);
                    _acceptDialog.Confirmed += AcceptConfirmed;
                    _acceptDialog.Canceled += AcceptCanceled;
                    _acceptDialog.PopupCentered();
                    GD.Print($"InputManager: Showing dialog for controller {joyName}");
                }
                else
                {
                    if (_controllerId == device)
                    {
                        _controllerActive = false;
                        _controllerId = -1;
                        _controllerName = "";
                        _controllerGuid = "";
                        GD.Print("InputManager: Controller connection lost");
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager: Error in HandleJoyConnectionChanged: {ex.Message}");
            }
        }

        private void AcceptConfirmed()
        {
            try
            {
                _controllerId = device;
                _controllerName = Input.GetJoyName(device) ?? "Unknown Controller";
                _controllerGuid = Input.GetJoyGuid(device) ?? "";
                _controllerActive = true;

                // Update mappings (same as _Ready, from KeyBindings.csv)
                _buttonMap.Clear();
                _buttonMap["MoveForward"] = 11; // DPad Up
                _buttonMap["MoveBack"] = 12; // DPad Down
                _buttonMap["StrafeLeft"] = 13; // DPad Left
                _buttonMap["StrafeRight"] = 14; // DPad Right
                _buttonMap["SelectUnit"] = 8; // RS
                _buttonMap["PauseGame"] = 4; // Back
                _buttonMap["QuitGame"] = 5; // Home
                _buttonMap["ScreenShot"] = 6; // Start
                _buttonMap["ResetCamera"] = 7; // LS
                _buttonMap["EndPhase"] = 9; // LB
                _buttonMap["EndTurn"] = 10; // RB
                _buttonMap["SkipVoiceover"] = 3; // Y
                _buttonMap["ViewPlayerLosses"] = 0; // A
                _buttonMap["ViewEnemyLosses"] = 1; // B
                _buttonMap["OpenHelpMenu"] = 2; // X

                _axisMap.Clear();
                _axisMap["PanX"] = 0; // LS X
                _axisMap["PanY"] = 1; // LS Y
                _axisMap["ZoomIn"] = 5; // RT
                _axisMap["ZoomOut"] = 4; // LT
                _axisMap["PointerX"] = 2; // RS X
                _axisMap["PointerY"] = 3; // RS Y

                //AddChild(_canvasLayer);
                _pointer = new Sprite2D
                {
                    Name = "Pointer",
                    Texture = GD.Load<Texture2D>("res://Assets/Sprites/UI_Elements/crosshair1_64.png"),
                    Position = GetViewport().GetVisibleRect().Size / 2, // Center initially
                    ZIndex = 0, // Layer 0 for pointer
                    Visible = true
                };
                _canvasLayer.AddChild(_pointer);
                GD.Print("InputManager: Pointer Sprite2D added to CanvasLayer at Layer 0");

                GD.Print($"InputManager: Enabled controller: Device {device}, Name={_controllerName}, GUID={_controllerGuid}");
                _acceptDialog.QueueFree();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager: Error in AcceptConfirmed: {ex.Message}");
                _acceptDialog.QueueFree();
            }
        }

        private void AcceptCanceled()
        {
            try
            {
                GD.Print("InputManager: Controller dialog canceled, no changes made");
                _acceptDialog.QueueFree();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager: Error in AcceptCanceled: {ex.Message}");
                _acceptDialog.QueueFree();
            }
        }

        public void LoadControllerSettings(Variant bindings)
        {
            try
            {
                _controllerActive = bindings.As<string>() == "Default";
                GD.Print($"InputManager: Loaded controller settings, active: {_controllerActive}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager: Error in LoadControllerSettings: {ex.Message}");
            }
        }

        public void LoadGameplaySettings(Dictionary settings)
        {
            try
            {
                if (settings.ContainsKey("panspeed"))
                    _panSpeed = settings["panspeed"].As<float>();
                if (settings.ContainsKey("movespeed"))
                    _moveSpeed = settings["movespeed"].As<float>();
                if (settings.ContainsKey("pointerspeed"))
                    _pointerSpeed = settings["pointerspeed"].As<float>();
                if (settings.ContainsKey("mousepanspeed"))
                    _mousePanSpeed = settings["mousepanspeed"].As<float>();
                if (settings.ContainsKey("cursorspeed"))
                    _cursorSpeed = settings["cursorspeed"].As<float>();
                GD.Print($"InputManager: Loaded gameplay settings - panSpeed: {_panSpeed}, moveSpeed: {_moveSpeed}, pointerSpeed: {_pointerSpeed}, mousePanSpeed: {_mousePanSpeed}, cursorSpeed: {_cursorSpeed}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"InputManager: Error in LoadGameplaySettings: {ex.Message}");
            }
        }
    }
}