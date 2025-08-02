using Godot;
using System;
using System.Collections.Generic;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts
{
    /// <summary>
    /// DisplayMenuPanel: Populates the display/video/HUD options menu programmatically.
    /// </summary>
    public partial class DisplayMenuPanel : Panel
    {
        // --- Fields ---
        private Godot.Collections.Dictionary<string, Variant> _settings = [];
        private ConfigFile _config;
        private readonly Vector2I[] _supported_resolutions =
        [
            new Vector2I(1280, 720),
            new Vector2I(1920, 1080),
            new Vector2I(2560, 1440),
            new Vector2I(3840, 2160),
            new Vector2I(7680, 4320)
        ];

        // Node references
        private VBoxContainer _display_menu;
        private Label _display_menu_label;
        private VBoxContainer _video_menu;
        private Label _video_menu_label;
        private OptionButton _video_button;
        private Button _auto_detect_button;
        private VBoxContainer _hud_menu;
        private Label _hud_menu_label;
        private GridContainer _hud_themes;
        private CheckButton _national_theme_button;
        private CheckButton _we_theme_button;
        private CheckButton _hc_theme_button;
        private CheckButton _sy_theme_button;
        private Button _font_nat_button;
        private Button _font_sys_button;
        private Label _format_label;
        private OptionButton _format_button;
        private HBoxContainer _close_container;
        private Button _accept_button;
        private Button _cancel_button;
        private ConfirmationDialog _confirmation_dialog;

        // Caches
        private Godot.Collections.Dictionary<string, Variant> _initial_settings = [];

        // Resource cache
        private Theme _theme_su, _theme_we, _theme_hc, _theme_sy;
        private FontFile _font_sys;

        public override void _Ready()
        {
            try
            {
                // Load config
                _config = new ConfigFile();
                var config_path = "user://War Eagles/config.cfg";
                if (FileAccess.FileExists(config_path))
                {
                    var err = _config.Load(config_path);
                    if (err != Error.Ok)
                        GD.Print($"Config load error: {err}");
                }

                // Load settings or set defaults
                _settings["Resolution"] = _config.HasSectionKey("Display", "Resolution")
                    ? _config.GetValue("Display", "Resolution")
                    : new Vector2I(1920, 1080);
                _settings["Scale"] = _config.HasSectionKey("Display", "Scale")
                    ? _config.GetValue("Display", "Scale")
                    : 1.0f;
                _settings["HUDTheme"] = _config.HasSectionKey("Display", "HUDTheme")
                    ? _config.GetValue("Display", "HUDTheme")
                    : "National";
                _settings["HUDFont"] = _config.HasSectionKey("Display", "HUDFont")
                    ? _config.GetValue("Display", "HUDFont")
                    : "National";
                _settings["HUDFormat"] = _config.HasSectionKey("Display", "HUDFormat")
                    ? _config.GetValue("Display", "HUDFormat")
                    : "Time, Date";

                // Cache initial settings
                foreach (var key in _settings.Keys)
                    _initial_settings[key] = _settings[key];

                // Preload themes and font
                try
                {
                    _theme_su = GD.Load<Theme>("res://Data/Resources/HUD_SU_Theme.tres");
                    _theme_we = GD.Load<Theme>("res://Data/Resources/HUD_WE_Theme.tres");
                    _theme_hc = GD.Load<Theme>("res://Data/Resources/HUD_HC_Theme.tres");
                    _theme_sy = GD.Load<Theme>("res://Data/Resources/HUD_SY_Theme.tres");
                    _font_sys = GD.Load<FontFile>("res://Assets/Fonts/BAHNSCHRIFT.TTF");
                }
                catch (Exception e)
                {
                    GD.Print($"Theme/font load error: {e.Message}");
                }

                PopulateDisplayMenu();
            }
            catch (Exception ex)
            {
                GD.Print($"Exception in DisplayMenuPanel._Ready: {ex.Message}");
            }
        }

        private void PopulateDisplayMenu()
        {
            try
            {
                // --- Display Menu Container ---
                _display_menu = new VBoxContainer { CustomMinimumSize = new Vector2(1024, 0) };
                AddChild(_display_menu);

                // --- Display Menu Label ---
                _display_menu_label = new Label
                {
                    Text = "  Display Options",
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                _display_menu_label.AddThemeFontSizeOverride("font_size", 56);
                _display_menu.AddChild(_display_menu_label);

                // --- Video Menu ---
                _video_menu = new VBoxContainer();
                _video_menu.AddThemeConstantOverride("separation", 12);
                _video_menu.Alignment = BoxContainer.AlignmentMode.Center;
                _display_menu.AddChild(_video_menu);

                // Video Menu Label
                _video_menu_label = new Label
                {
                    Text = "Video Resolution",
                    CustomMinimumSize = new Vector2(0, 120),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                _video_menu_label.AddThemeFontSizeOverride("font_size", 48);
                _video_menu.AddChild(_video_menu_label);

                // Video OptionButton
                _video_button = new OptionButton();
                _video_button.AddThemeFontSizeOverride("font_size", 48);
                _video_button.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
                string[] res_labels = ["1280x720 - SD", "1920x1080 - HD", "2560x1440 - 2K", "3840x2160 - 4K", "7680x4320 - 8K"];
                foreach (var label in res_labels)
                    _video_button.AddItem(label);
                int selected = 1; // Default to 1920x1080
                for (int i = 0; i < _supported_resolutions.Length; i++)
                {
                    if (_settings["Resolution"].As<Vector2I>() == _supported_resolutions[i])
                        selected = i;
                }
                _video_button.Selected = selected;
                _video_button.ItemSelected += OnVideoButtonItemSelected;
                _video_menu.AddChild(_video_button);
                _video_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_video_button.Name} to ui_buttons group");

                // AutoDetect Button
                _auto_detect_button = new Button
                {
                    Text = "Autodetect",
                    Flat = true,
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter
                };
                _auto_detect_button.Pressed += OnAutoDetectButtonPressed;
                _video_menu.AddChild(_auto_detect_button);
                _auto_detect_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_auto_detect_button.Name} to ui_buttons group");

                // --- HUD Menu ---
                _hud_menu = new VBoxContainer();
                _hud_menu.AddThemeConstantOverride("separation", 25);
                _display_menu.AddChild(_hud_menu);

                // HUD Menu Label
                _hud_menu_label = new Label
                {
                    Text = "Interface Settings",
                    CustomMinimumSize = new Vector2(0, 120),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                _hud_menu_label.AddThemeFontSizeOverride("font_size", 48);
                _hud_menu.AddChild(_hud_menu_label);

                // HUD Themes Grid
                _hud_themes = new GridContainer
                {
                    Columns = 2,
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter
                };
                _hud_themes.AddThemeConstantOverride("h_separation", 12);
                _hud_themes.AddThemeConstantOverride("v_separation", 12);
                _hud_menu.AddChild(_hud_themes);

                // National Theme Button
                _national_theme_button = new CheckButton
                {
                    Text = "National",
                    Theme = _theme_su,
                    CustomMinimumSize = new Vector2(360, 80),
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter
                };
                _national_theme_button.Toggled += OnNationalThemeButtonToggled;
                _hud_themes.AddChild(_national_theme_button);
                _national_theme_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_national_theme_button.Name} to ui_buttons group");

                // War Eagles Theme Button
                _we_theme_button = new CheckButton
                {
                    Text = "War Eagles",
                    Theme = _theme_we,
                    CustomMinimumSize = new Vector2(360, 80),
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter
                };
                _we_theme_button.Toggled += OnWEThemeButtonToggled;
                _hud_themes.AddChild(_we_theme_button);
                _we_theme_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_we_theme_button.Name} to ui_buttons group");

                // Contrast Theme Button
                _hc_theme_button = new CheckButton
                {
                    Text = "Contrast",
                    Theme = _theme_hc,
                    CustomMinimumSize = new Vector2(360, 80),
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter
                };
                _hc_theme_button.Toggled += OnHCThemeButtonToggled;
                _hud_themes.AddChild(_hc_theme_button);
                _hc_theme_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_hc_theme_button.Name} to ui_buttons group");

                // System Theme Button
                _sy_theme_button = new CheckButton
                {
                    Text = "System",
                    Theme = _theme_sy,
                    CustomMinimumSize = new Vector2(360, 80),
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter
                };
                _sy_theme_button.Toggled += OnSYThemeButtonToggled;
                _hud_themes.AddChild(_sy_theme_button);
                _sy_theme_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_sy_theme_button.Name} to ui_buttons group");

                // Font National Button
                _font_nat_button = new Button
                {
                    Text = "National Font",
                    Flat = true,
                    CustomMinimumSize = new Vector2(360, 80),
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter
                };
                _font_nat_button.Pressed += OnFontNatButtonPressed;
                _hud_themes.AddChild(_font_nat_button);
                _font_nat_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_font_nat_button.Name} to ui_buttons group");

                // Font System Button
                _font_sys_button = new Button
                {
                    Text = "System Font",
                    Flat = true,
                    CustomMinimumSize = new Vector2(360, 80),
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter
                };
                try
                {
                    if (_font_sys != null)
                        _font_sys_button.AddThemeFontOverride("font", _font_sys);
                }
                catch (Exception e)
                {
                    GD.Print($"Failed to apply system font: {e.Message}");
                }
                _font_sys_button.Pressed += OnFontSysButtonPressed;
                _hud_themes.AddChild(_font_sys_button);
                _font_sys_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_font_sys_button.Name} to ui_buttons group");

                // Format Label
                _format_label = new Label
                {
                    Text = "Turn and Phase Format",
                    CustomMinimumSize = new Vector2(0, 80),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                _display_menu.AddChild(_format_label);

                // Format OptionButton
                _format_button = new OptionButton
                {
                    Flat = true,
                    SizeFlagsHorizontal = SizeFlags.ShrinkCenter
                };
                _format_button.AddItem("Time, Date", 1);
                _format_button.AddItem("Phase, Turn", 2);
                _format_button.Selected = _settings["HUDFormat"].As<string>() == "Phase, Turn" ? 1 : 0;
                _format_button.ItemSelected += OnFormatButtonItemSelected;
                _display_menu.AddChild(_format_button);
                _format_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_format_button.Name} to ui_buttons group");

                // Close Container
                _close_container = new HBoxContainer
                {
                    CustomMinimumSize = new Vector2(0, 256),
                    Alignment = BoxContainer.AlignmentMode.Center
                };
                _close_container.AddThemeConstantOverride("separation", 128);
                _display_menu.AddChild(_close_container);

                // Accept Button
                _accept_button = new Button
                {
                    Text = "Accept",
                    CustomMinimumSize = new Vector2(256, 128),
                    Flat = false,
                    SizeFlagsVertical = SizeFlags.ShrinkCenter
                };
                _accept_button.Pressed += OnAcceptButtonPressed;
                _close_container.AddChild(_accept_button);
                _accept_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_accept_button.Name} to ui_buttons group");

                // Cancel Button
                _cancel_button = new Button
                {
                    Text = "Cancel",
                    CustomMinimumSize = new Vector2(256, 128),
                    Flat = false,
                    SizeFlagsVertical = SizeFlags.ShrinkCenter
                };
                _cancel_button.Pressed += OnCancelButtonPressed;
                _close_container.AddChild(_cancel_button);
                _cancel_button.AddToGroup("ui_buttons");
                //GD.Print($"Added {_cancel_button.Name} to ui_buttons group");
            }
            catch (Exception ex)
            {
                GD.Print($"Exception in PopulateDisplayMenu: {ex.Message}");
            }
        }

        // --- Event Handlers ---

        private void OnVideoButtonItemSelected(long index)
        {
            try
            {
                if (index < 0 || index >= _supported_resolutions.Length)
                {
                    GD.Print("Unsupported resolution index selected.");
                    return;
                }
                _settings["Resolution"] = _supported_resolutions[(int)index];
                float scale = (float)Math.Round((_supported_resolutions[(int)index].Y / 1440.0) * 4) / 4;
                _settings["Scale"] = scale;
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnVideoButtonItemSelected: {ex.Message}");
            }
        }

        private void OnAutoDetectButtonPressed()
        {
            try
            {
                Vector2I screenSize = new(1920, 1080); // Mock for testing
                int closest = 1; // Default to 1920x1080
                double minDist = double.MaxValue;
                for (int i = 0; i < _supported_resolutions.Length; i++)
                {
                    double dist = (screenSize - _supported_resolutions[i]).LengthSquared();
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = i;
                    }
                }
                if (minDist > 0)
                    GD.Print($"Autodetect: Closest supported resolution is {_supported_resolutions[closest]}");
                _settings["Resolution"] = _supported_resolutions[closest];
                float scale = (float)Math.Round((_supported_resolutions[closest].Y / 1440.0) * 4) / 4;
                _settings["Scale"] = scale;
                _video_button.Selected = closest;
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnAutoDetectButtonPressed: {ex.Message}");
            }
        }

        private void OnNationalThemeButtonToggled(bool pressed)
        {
            if (pressed)
            {
                _settings["HUDTheme"] = "National";
                SetThemeButtonStates("National");
            }
        }

        private void OnWEThemeButtonToggled(bool pressed)
        {
            if (pressed)
            {
                _settings["HUDTheme"] = "War Eagles";
                SetThemeButtonStates("War Eagles");
            }
        }

        private void OnHCThemeButtonToggled(bool pressed)
        {
            if (pressed)
            {
                _settings["HUDTheme"] = "Contrast";
                SetThemeButtonStates("Contrast");
            }
        }

        private void OnSYThemeButtonToggled(bool pressed)
        {
            if (pressed)
            {
                _settings["HUDTheme"] = "System";
                SetThemeButtonStates("System");
            }
        }

        private void OnFontNatButtonPressed()
        {
            _settings["HUDFont"] = "National";
        }

        private void OnFontSysButtonPressed()
        {
            try
            {
                if (_font_sys == null)
                    GD.Print("System font (BAHNSCHRIFT.TTF) not loaded.");
                _settings["HUDFont"] = "System";
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnFontSysButtonPressed: {ex.Message}");
            }
        }

        private void OnFormatButtonItemSelected(long index)
        {
            _settings["HUDFormat"] = index == 1 ? "Phase, Turn" : "Time, Date";
        }

        private void OnAcceptButtonPressed()
        {
            try
            {
                _confirmation_dialog = new ConfirmationDialog
                {
                    DialogText = "Accept these settings?",
                    Borderless = true,
                    Unresizable = false,
                    Size = (Vector2I)new Vector2(400, 200),
                    Position = (Vector2I)new Vector2(0, 0)
                };
                _confirmation_dialog.SetInitialPosition(Window.WindowInitialPosition.CenterPrimaryScreen);
                _confirmation_dialog.AddThemeColorOverride("background_color", new Color(0, 0, 0, 0));
                _confirmation_dialog.Confirmed += OnConfirmationDialogConfirmed;
                _confirmation_dialog.Canceled += OnConfirmationDialogCanceled;
                AddChild(_confirmation_dialog);
                _confirmation_dialog.Popup();
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnAcceptButtonPressed: {ex.Message}");
            }
        }

        private void OnCancelButtonPressed()
        {
            try
            {
                RestoreInitialSettings();
                GetTree().ChangeSceneToFile("res://Scenes/PauseMenu.tscn");
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnCancelButtonPressed: {ex.Message}");
            }
        }

        private void OnConfirmationDialogConfirmed()
        {
            try
            {
                // Apply resolution and scale
                if (_settings.ContainsKey("Resolution"))
                    DisplayServer.WindowSetSize(_settings["Resolution"].As<Vector2I>());
                if (_settings.ContainsKey("Scale"))
                    GetWindow().SetContentScaleFactor(_settings["Scale"].As<float>());

                // Save to config
                _config.SetValue("Display", "Resolution", _settings["Resolution"]);
                _config.SetValue("Display", "Scale", _settings["Scale"]);
                _config.SetValue("Display", "HUDTheme", _settings["HUDTheme"]);
                _config.SetValue("Display", "HUDFont", _settings["HUDFont"]);
                _config.SetValue("Display", "HUDFormat", _settings["HUDFormat"]);
                _config.Save("user://War Eagles/config.cfg");

                // Notify GameManager
               /* var gameManager = GetNodeOrNull<Node>("/root/GameManager");
                if (gameManager != null)
                    gameManager.Call("SetGameSettings", _settings);
                else
                    GD.Print("GameManager not found for SetGameSettings.");*/

                _confirmation_dialog.QueueFree();
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
                RestoreInitialSettings();
                _confirmation_dialog.QueueFree();
                GetTree().ChangeSceneToFile("res://Scenes/PauseMenu.tscn");
            }
            catch (Exception ex)
            {
                GD.Print($"Error in OnConfirmationDialogCanceled: {ex.Message}");
            }
        }

        // --- Helpers ---

        private void SetThemeButtonStates(string selected)
        {
            _national_theme_button.ButtonPressed = selected == "National";
            _we_theme_button.ButtonPressed = selected == "War Eagles";
            _hc_theme_button.ButtonPressed = selected == "Contrast";
            _sy_theme_button.ButtonPressed = selected == "System";
        }

        private void RestoreInitialSettings()
        {
            foreach (var key in _initial_settings.Keys)
                _settings[key] = _initial_settings[key];
        }

        // NYI: For future features
        private void ApplyHUDTheme(string theme)
        {
            // NYI: Apply HUD theme to UI
        }

        private void ApplyHUDFont(string font)
        {
            // NYI: Apply HUD font to UI
        }

        private void ApplyHUDFormat(string format)
        {
            // NYI: Apply HUD format to UI
        }

        public void InitializeDisplayMenu(string branch)
        {
            try
            {
                if (branch == "Video and Display")
                {
                    PopulateDisplayMenu();
                    GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");
                }
                else
                {
                    GD.Print($"InitializeDisplayMenu: Unknown branch '{branch}'");
                }
            }
            catch (Exception ex)
            {
                GD.Print($"Exception in InitializeDisplayMenu: {ex.Message}");
            }
        }
    }
}