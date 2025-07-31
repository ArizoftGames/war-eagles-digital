using Godot;
using System;
//.using System.IO;
using System.Linq;
using System.Collections.Generic;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts
{
    /// <summary>
    /// ControlsMenuPanel: Populates the controls menu programmatically, manages controller selection, displays key bindings, and saves settings.
    /// </summary>
    public partial class ControlsMenuPanel : Panel
    {
        // --- Fields ---
        private VBoxContainer _controlsMenu;
        private Label _controlsMenuLabel;
        private HBoxContainer _controlSelectors;
        private Label _controllersLabel;
        private CheckButton _kbmButton;
        private CheckButton _controllerButton;
        private GridContainer _keyBindingList;
        private HBoxContainer _closeContainer;
        private Button _acceptButton;
        private Button _cancelButton;
        private ConfirmationDialog _confirmationDialog;

        private ConfigFile _config;
        private Godot.Collections.Dictionary<string, Variant> _settings = [];
        private Godot.Collections.Dictionary<string, Variant> _initialSettings = [];

        private const string ConfigPath = "user://War Eagles/config.cfg";
        private const string KeyBindingsPath = "res://Docs/KeyBindings.csv";

        public override void _Ready()
        {
            try
            {
                // Initialize config
                _config = new ConfigFile();
                if (FileAccess.FileExists(ConfigPath))
                {
                    var err = _config.Load(ConfigPath);
                    if (err != Error.Ok)
                        GD.Print($"ControlsMenuPanel: Config load error: {err}");
                }

                // Load settings or set defaults
                _settings["Bindings"] = _config.HasSectionKey("Controller", "Bindings")
                    ? _config.GetValue("Controller", "Bindings")
                    : "KBM";
                _initialSettings["Bindings"] = _settings["Bindings"];

                //PopulateControlsMenu();

                // ConfirmationDialog setup
                _confirmationDialog = new ConfirmationDialog
                {
                    DialogText = "Accept these settings?",
                    Borderless = true,
                    Unresizable = false,
                    Size = (Vector2I)new Vector2(400, 200),
                    Position = (Vector2I)new Vector2(0, 0)
                };
                _confirmationDialog.SetInitialPosition(Window.WindowInitialPosition.CenterPrimaryScreen);
                _confirmationDialog.Confirmed += OnConfirmationDialogConfirmed;
                _confirmationDialog.Canceled += OnConfirmationDialogCanceled;
                AddChild(_confirmationDialog);
            }
            catch (Exception ex)
            {
                GD.Print($"Exception in ControlsMenuPanel._Ready: {ex.Message}");
            }
        }

        private void PopulateControlsMenu()
        {
            try
            {
                // Get ControlsMenu VBoxContainer
                _controlsMenu = GetNodeOrNull<VBoxContainer>("ControlsMenu");
                if (_controlsMenu == null)
                {
                    GD.PrintErr("ControlsMenuPanel: ControlsMenu node missing");
                    return;
                }

                // Clear existing children
                foreach (Node child in _controlsMenu.GetChildren())
                    child.QueueFree();

                // --- ControlsMenuLabel ---
                _controlsMenuLabel = new Label
                {
                    Text = "Game Controls",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _controlsMenuLabel.AddThemeFontSizeOverride("font_size", 48);
                _controlsMenu.AddChild(_controlsMenuLabel);

                // --- ControlSelectors ---
                _controlSelectors = new HBoxContainer
                {
                    CustomMinimumSize = new Vector2(1280, 0)
                };
                _controlsMenu.AddChild(_controlSelectors);

                // ControllersLabel
                _controllersLabel = new Label
                {
                    Text = "Select Game Controller:",
                    CustomMinimumSize = new Vector2(400, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _controllersLabel.AddThemeFontSizeOverride("font_size", 32);
                _controlSelectors.AddChild(_controllersLabel);

                // KBMButton
                _kbmButton = new CheckButton
                {
                    Text = "Keyboard and Mouse (KBM)",
                    CustomMinimumSize = new Vector2(420, 0),
                    //Button.HorizontalAlignment = HorizontalAlignment.Left
                };
                _kbmButton.AddThemeFontSizeOverride("font_size", 32);
                _kbmButton.Toggled += OnKBMButtonToggled;
                _kbmButton.AddToGroup("ui_buttons");
                _controlSelectors.AddChild(_kbmButton);
                _kbmButton.Alignment = HorizontalAlignment.Left;

                // ControllerButton
                _controllerButton = new CheckButton
                {
                    Text = "X-Box Controller + KBM",
                    CustomMinimumSize = new Vector2(420, 0),
                    //HorizontalAlignment = HorizontalAlignment.Left
                };
                _controllerButton.AddThemeFontSizeOverride("font_size", 32);
                _controllerButton.Toggled += OnControllerButtonToggled;
                _controllerButton.AddToGroup("ui_buttons");
                _controllerButton.Alignment = HorizontalAlignment.Left;
                _controlSelectors.AddChild(_controllerButton);

                // --- KeyBindingList ---
                _keyBindingList = new GridContainer
                {
                    Columns = 3,
                    CustomMinimumSize = new Vector2(1280, 0),
                    SizeFlagsHorizontal = (Control.SizeFlags)Control.SizeFlags.ShrinkCenter,
                    SizeFlagsVertical = (Control.SizeFlags)Control.SizeFlags.Fill
                };
                _keyBindingList.Set("theme_override_constants/h_separation", 4);
                _controlsMenu.AddChild(_keyBindingList);

                // --- CloseContainer ---
                _closeContainer = new HBoxContainer
                {
                    CustomMinimumSize = new Vector2(1280, 240),
                    Alignment = BoxContainer.AlignmentMode.Center
                };
                _closeContainer.Set("theme_override_constants/separation", 100);
                _controlsMenu.AddChild(_closeContainer);

                // AcceptButton
                _acceptButton = new Button
                {
                    Text = "Accept",
                    CustomMinimumSize = new Vector2(160, 120),
                    SizeFlagsVertical = (Control.SizeFlags)Control.SizeFlags.ShrinkCenter
                };
                _acceptButton.Pressed += OnAcceptButtonPressed;
                _acceptButton.AddToGroup("ui_buttons");
                _closeContainer.AddChild(_acceptButton);

                // CancelButton
                _cancelButton = new Button
                {
                    Text = "Cancel",
                    CustomMinimumSize = new Vector2(160, 120),
                    SizeFlagsVertical = (Control.SizeFlags)Control.SizeFlags.ShrinkCenter
                };
                _cancelButton.Pressed += OnCancelButtonPressed;
                _cancelButton.AddToGroup("ui_buttons");
                _closeContainer.AddChild(_cancelButton);

                LoadControlSettings();
                PopulateKeyBindings();
            }
            catch (Exception ex)
            {
                GD.Print($"Exception in PopulateControlsMenu: {ex.Message}");
            }
        }

        private void LoadControlSettings()
        {
            try
            {
                string mode = _settings.ContainsKey("Bindings") ? _settings["Bindings"].AsString() : "KBM";
                if (mode == "Controller")
                {
                    _controllerButton.ButtonPressed = true;
                    _kbmButton.ButtonPressed = false;
                }
                else
                {
                    _kbmButton.ButtonPressed = true;
                    _controllerButton.ButtonPressed = false;
                }
                _initialSettings["Bindings"] = mode;
            }
            catch (Exception ex)
            {
                GD.Print($"Error loading control settings: {ex.Message}");
                _kbmButton.ButtonPressed = true;
                _controllerButton.ButtonPressed = false;
                _settings["Bindings"] = "KBM";
            }
        }

        private void PopulateKeyBindings()
        {
            try
            {
                foreach (Node child in _keyBindingList.GetChildren())
                    child.QueueFree();

                if (!FileAccess.FileExists(KeyBindingsPath))
                {
                    var errorLabel = new Label
                    {
                        Text = "Error loading key bindings",
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    _keyBindingList.AddChild(errorLabel);
                    return;
                }

                using var file = FileAccess.Open(KeyBindingsPath, FileAccess.ModeFlags.Read);
                var header = file.GetCsvLine();
                int rowCount = 0;
                while (!file.EofReached())
                {
                    var line = file.GetCsvLine();
                    if (line.Length != 3)
                        continue;
                    for (int i = 0; i < 3; i++)
                    {
                        var label = new Label
                        {
                            Text = line[i],
                            CustomMinimumSize = new Vector2(412, 0),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            SizeFlagsHorizontal = (Control.SizeFlags)Control.SizeFlags.Fill,
                            SizeFlagsVertical = (Control.SizeFlags)Control.SizeFlags.ShrinkCenter,
                            ThemeTypeVariation = "GridLabel"
                        };
                        _keyBindingList.AddChild(label);
                    }
                    rowCount++;
                }
                if (rowCount == 0)
                {
                    var errorLabel = new Label
                    {
                        Text = "No key bindings found",
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    _keyBindingList.AddChild(errorLabel);
                }
            }
            catch (Exception ex)
            {
                GD.Print($"Error populating key bindings: {ex.Message}");
                var errorLabel = new Label
                {
                    Text = "Error loading key bindings",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _keyBindingList.AddChild(errorLabel);
            }
        }

        private void OnKBMButtonToggled(bool toggled)
        {
            try
            {
                if (toggled)
                {
                    // Enable KBM input, disable controller input
                    _controllerButton.ButtonPressed = false;
                    _settings["Bindings"] = "KBM";
                    GD.Print("KBM selected. Controller input disabled.");
                    // InputMap update NYI (handled elsewhere)
                }
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnKBMButtonToggled: {ex.Message}");
            }
        }

        private void OnControllerButtonToggled(bool toggled)
        {
            try
            {
                if (toggled)
                {
                    // Detect controller
                    var joypads = Input.GetConnectedJoypads();
                    bool found = false;
                    foreach (int id in joypads)
                    {
                        string joyName = Input.GetJoyName(id);
                        string joyGuid = Input.GetJoyGuid(id);
                        GD.Print($"Controller detected: {joyName}, GUID: {joyGuid}");
                        if (joyName.Contains("XBOX FOR WINDOWS", StringComparison.OrdinalIgnoreCase) || joyGuid.Contains("XINPUT", StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        GD.Print("No supported controller detected. Fallback to KBM.");
                        _controllerButton.ButtonPressed = false;
                        _kbmButton.ButtonPressed = true;
                        _settings["Bindings"] = "KBM";
                        return;
                    }
                    _kbmButton.ButtonPressed = false;
                    _settings["Bindings"] = "Controller";
                    GD.Print("Controller selected. KBM input disabled.");
                    // InputMap update NYI (handled elsewhere)
                }
                else
                {
                    // Disable controller input
                    _settings["Bindings"] = "KBM";
                    _kbmButton.ButtonPressed = true;
                    GD.Print("Controller deselected. KBM enabled.");
                }
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnControllerButtonToggled: {ex.Message}");
                _settings["Bindings"] = "KBM";
                _kbmButton.ButtonPressed = true;
                _controllerButton.ButtonPressed = false;
            }
        }

        private void OnAcceptButtonPressed()
        {
            try
            {
                _confirmationDialog.Popup();
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnAcceptButtonPressed: {ex.Message}");
            }
        }

        private void OnConfirmationDialogConfirmed()
        {
            try
            {
                // Save settings to config
                _config.SetValue("Controller", "Bindings", _settings["Bindings"]);
                var err = _config.Save(ConfigPath);
                if (err != Error.Ok)
                    GD.Print($"ControlsMenuPanel: Config save error: {err}");

                // Notify GameManager
               /* var gameManager = GetNodeOrNull<Node>("/root/GameManager");
                if (gameManager != null)
                    gameManager.Call("SetGameSettings", _settings);
                else
                    GD.Print("GameManager not found for SetGameSettings.");*/

                _confirmationDialog.QueueFree();
                GetTree().ChangeSceneToFile("res://Scenes/PauseMenu.tscn");
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnConfirmationDialogConfirmed: {ex.Message}");
            }
        }

        private void OnConfirmationDialogCanceled()
        {
            try
            {
                _confirmationDialog.QueueFree();
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnConfirmationDialogCanceled: {ex.Message}");
            }
        }

        private void OnCancelButtonPressed()
        {
            try
            {
                // Restore initial settings
                if (_initialSettings.ContainsKey("Bindings"))
                    _settings["Bindings"] = _initialSettings["Bindings"];
                GetTree().ChangeSceneToFile("res://Scenes/PauseMenu.tscn");
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnCancelButtonPressed: {ex.Message}");
            }
        }

        public void InitializeControlsMenu(string branch)
        {
            try
            {
                if (branch == "Controls")
                {
                    PopulateControlsMenu();
                    GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");
                }
                else
                {
                    GD.Print($"InitializeControlsMenu: Unknown branch '{branch}'");
                }
            }
            catch (Exception ex)
            {
                GD.Print($"Exception in InitializeDisplayMenu: {ex.Message}");
            }
        }
    }
}
