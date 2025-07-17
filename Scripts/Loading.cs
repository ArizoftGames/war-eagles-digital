using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WarEaglesDigital.Scripts;
using Godot.Collections;

namespace WarEaglesDigital.Scripts
{
    public partial class Loading : Control
    {
        //private AnimationPlayer Fadeout_Player;
        private readonly string _userConfigPath = "user://War Eagles/config.cfg";
        private readonly string _resConfigPath = "res://Data/War Eagles/config.cfg";
        private ConfigFile _config = new ConfigFile();
        private Dictionary _settings = new Dictionary();
        private string _activeConfigPath;
        private readonly Vector2I[] _supportedResolutions = new[]


       {
            new Vector2I(1280, 720),
            new Vector2I(1920, 1080),
            new Vector2I(2560, 1440),
            new Vector2I(3840, 2160),
            new Vector2I(7680, 4320)
        };
        private Resource _introAnimScene; // Preloaded IntroAnim.tscn
        private const float _minimumLoadingTime = 3.0f; // Covers loading and grey screen

        public override void _Ready()
        {
            //GD.Print("Loading scene initialized");
            LoadGameSettings();
            WaitTimer(_minimumLoadingTime);
        }

        private void LoadGameSettings()
        {
            try
            {
                // Migration from old user:// to current user://
                string oldConfigPath = "user://War Eagles/config.cfg";
                string newConfigPath = _userConfigPath; // Already absolute: "user://War Eagles/config.cfg"
                if (FileAccess.FileExists(oldConfigPath) && !FileAccess.FileExists(newConfigPath))
                {
                    using var oldDir = DirAccess.Open("user://");
                    using var newDir = DirAccess.Open("user://");
                    if (oldDir != null && newDir != null && !newDir.DirExists("War Eagles"))
                        newDir.MakeDir("War Eagles");
                    if (oldDir.FileExists("War Eagles/config.cfg"))
                    {
                        oldDir.Copy("War Eagles/config.cfg", newConfigPath);
                        oldDir.Remove("War Eagles/config.cfg");
                    }
                }

                // Perform loading tasks
                _introAnimScene = ResourceLoader.Load("res://Scenes/IntroAnim.tscn");
                if (_introAnimScene == null)
                {
                    GD.PushError("Failed to preload IntroAnim.tscn");
                    return;
                }

                bool configOk = CheckConfigFile();
                if (!configOk)
                {
                    GD.PushError("Config file check failed. Creating default settings.");
                    WriteDefaultConfig();
                }
                LoadDisplaySettings();
                LoadControllerSettings();
                LoadGameplaySettings();
                ApplySettings();
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in LoadGameSettings: {ex.Message}");
            }
        }

        private async void WaitTimer(float delay)
        {
            try
            {
                
                //Fadeout_Player = GetNode<AnimationPlayer>("FadeoutPlayer");
                
                await ToSignal(GetTree().CreateTimer(delay), "timeout");
                GD.Print("WaitTimer completed, calling TransitionTo()");
                var gameManager = GetNodeOrNull<Node>("/root/GameManager");
                gameManager.Call("TransitionTo", "res://Scenes/IntroAnim.tscn"); 
                // TransitionToIntroAnim();
                //Fadeout_Player.Play("Fadeout");
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in WaitTimer: {ex.Message}");
                //TransitionToIntroAnim(); // Proceed to transition on error
            }
        }

        private bool CheckConfigFile()
        {
            try
            {
                var userDir = DirAccess.Open("user://");
                if (userDir != null && !userDir.DirExists("War Eagles"))
                {
                    Error mkErr = userDir.MakeDir("War Eagles");
                    if (mkErr != Error.Ok)
                    {
                        GD.PushError($"Failed to create user://War Eagles/: {mkErr}");
                        var resDir = DirAccess.Open("res://Data");
                        if (resDir != null && !resDir.DirExists("War Eagles"))
                            resDir.MakeDir("War Eagles");
                        _activeConfigPath = "res://Data/War Eagles/config.cfg";
                        return false;
                    }
                }
                _activeConfigPath = "user://War Eagles/config.cfg";
                if (FileAccess.FileExists(_activeConfigPath))
                {
                    Error err = _config.Load(_activeConfigPath);
                    return err == Error.Ok;
                }
                return false; // Triggers config write if absent
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in CheckConfigFile: {ex.Message}");
                return false;
            }
        }

        private void LoadDisplaySettings()
        {
            try
            {
                if (_config.HasSectionKey("Display", "Resolution"))
                {
                    _settings["Resolution"] = _config.GetValue("Display", "Resolution");
                    _settings["Scale"] = _config.GetValue("Display", "Scale");
                    _settings["HUDTheme"] = _config.GetValue("Display", "HUDTheme");
                    _settings["HUDFont"] = _config.GetValue("Display", "HUDFont");
                    _settings["HUDFormat"] = _config.GetValue("Display", "HUDFormat");
                    return;
                }
                Vector2I screenSize = DisplayServer.ScreenGetSize();
                bool supported = false;
                foreach (var resolution in _supportedResolutions)
                {
                    if (screenSize == resolution)
                    {
                        supported = true;
                        break;
                    }
                }
                if (!supported)
                {
                    GD.PushError($"Unsupported resolution {screenSize}. Defaulting to 1920x1080");
                    screenSize = new Vector2I(1920, 1080);
                }
                float scale = (float)Math.Round((screenSize.Y / 1440.0) * 4) / 4;
                if (!supported) scale = 1f;
                _settings["Resolution"] = screenSize;
                _settings["Scale"] = scale;
                _settings["HUDTheme"] = "National";
                _settings["HUDFont"] = "National";
                _settings["HUDFormat"] = "Time/Date";
                _config.SetValue("Display", "Resolution", screenSize);
                _config.SetValue("Display", "Scale", scale);
                _config.SetValue("Display", "HUDTheme", "National");
                _config.SetValue("Display", "HUDFont", "National");
                _config.SetValue("Display", "HUDFormat", "Time/Date");
                _config.Save(_activeConfigPath);
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in LoadDisplaySettings: {ex.Message}");
            }
        }

        private void LoadControllerSettings()
        {
            try
            {
                if (_config.HasSectionKey("Controller", "Bindings"))
                {
                    _settings["Bindings"] = _config.GetValue("Controller", "Bindings");
                    return;
                }
                var joypads = Input.GetConnectedJoypads();
                string bindings = "Keyboard";
                foreach (int device in joypads)
                {
                    string joyName = Input.GetJoyName(device);
                    string joyGUID = Input.GetJoyGuid(device);
                    if (joyName != null && (joyName.Contains("Xbox", StringComparison.OrdinalIgnoreCase) || joyName.Contains("XInput", StringComparison.OrdinalIgnoreCase)))
                    {
                        GD.Print($"Detected Xbox controller: Name={joyName}");
                        GD.Print($"GUID: {joyGUID}");
                        bindings = "Default";
                        break;
                    }
                }
                _settings["Bindings"] = bindings;
                _config.SetValue("Controller", "Bindings", bindings);
                _config.Save(_activeConfigPath);
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in LoadControllerSettings: {ex.Message}");
            }
        }

        private void LoadGameplaySettings()
        {
            try
            {
                if (_config.HasSection("Gameplay"))
                {
                    _settings["Mode"] = _config.GetValue("Gameplay", "Mode", "Quality");
                    _settings["Year"] = _config.GetValue("Gameplay", "Year", "Historical");
                    _settings["Opponent"] = _config.GetValue("Gameplay", "Opponent", "Historical");
                    _settings["Zone"] = _config.GetValue("Gameplay", "Zone", "Nation");
                    _settings["Purchase"] = _config.GetValue("Gameplay", "Purchase", "Historical");
                    _settings["MoveSpeed"] = _config.GetValue("Gameplay", "MoveSpeed", "Normal");
                    _settings["CombatSpeed"] = _config.GetValue("Gameplay", "CombatSpeed", "Normal");
                    _settings["TransitionSpeed"] = _config.GetValue("Gameplay", "TransitionSpeed", "Normal");
                    _settings["VoiceOver"] = _config.GetValue("Gameplay", "VoiceOver", "Normal");
                    return;
                }
                _settings["Mode"] = "Quality";
                _settings["Year"] = "Historical";
                _settings["Opponent"] = "Historical";
                _settings["Zone"] = "Nation";
                _settings["Purchase"] = "Historical";
                _settings["MoveSpeed"] = "Normal";
                _settings["CombatSpeed"] = "Normal";
                _settings["TransitionSpeed"] = "Normal";
                _settings["VoiceOver"] = "Normal";
                _config.SetValue("Gameplay", "Mode", "Quality");
                _config.SetValue("Gameplay", "Year", "Historical");
                _config.SetValue("Gameplay", "Opponent", "Historical");
                _config.SetValue("Gameplay", "Zone", "Nation");
                _config.SetValue("Gameplay", "Purchase", "Historical");
                _config.SetValue("Gameplay", "MoveSpeed", "Normal");
                _config.SetValue("Gameplay", "CombatSpeed", "Normal");
                _config.SetValue("Gameplay", "TransitionSpeed", "Normal");
                _config.SetValue("Gameplay", "VoiceOver", "Normal");
                _config.Save(_activeConfigPath);
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in LoadGameplaySettings: {ex.Message}");
            }
        }

        private void ApplySettings()
        {
            try
            {
                if (_settings.ContainsKey("Resolution"))
                {
                    Vector2I resolution = _settings["Resolution"].As<Vector2I>();
                    DisplayServer.WindowSetSize(resolution);
                }
                if (_settings.ContainsKey("Scale"))
                {
                    float scale = _settings["Scale"].As<float>();
                    GetWindow().SetContentScaleFactor(scale);
                }
                if (_settings.ContainsKey("HUDTheme"))
                {
                    string theme = _settings["HUDTheme"].As<string>();
                    ApplyHUDTheme(theme);
                }
                var gameManager = GetNodeOrNull<Node>("/root/GameManager");
                if (gameManager != null)
                {
                    gameManager.Call("SetGameSettings", _settings);
                }
                else
                {
                    GD.PushError("GameManager not found");
                }
                /* try
                {
                    UnitDatabase.GetControllerBindings(_settings["Bindings"].ToString());
                }
                catch (Exception ex)
                {
                    GD.PushError($"Error applying controller bindings: {ex.Message}");
                } */
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in ApplySettings: {ex.Message}");
            }
        }

        private void ApplyHUDTheme(string theme)
        {
            GD.Print($"HUD theme application NYI for theme: {theme}");
        }

      /*  private async void TransitionToIntroAnim()
        {
            try
            {
                var transitionScene = ResourceLoader.Load<PackedScene>("res://Scenes/Transition.tscn").Instantiate();
                AddChild(transitionScene);
                var animPlayer = transitionScene.GetNode<AnimationPlayer>("AnimationPlayer");
                animPlayer.Play("FadeIn");
                await ToSignal(animPlayer, "animation_finished");
                GetTree().ChangeSceneToFile("res://Scenes/IntroAnim.tscn");
                animPlayer.Play("FadeOut");
                await ToSignal(animPlayer, "animation_finished");
                transitionScene.QueueFree();
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in TransitionToIntroAnim: {ex.Message}");
            }
        }*/

        private void WriteDefaultConfig()
        {
            try
            {
                var userDir = DirAccess.Open("user://");
                if (userDir != null && !userDir.DirExists("War Eagles"))
                    userDir.MakeDir("War Eagles");
                _config.SetValue("Display", "Resolution", new Vector2I(1920, 1080));
                _config.SetValue("Display", "Scale", 1.0f);
                _config.SetValue("Display", "HUDTheme", "National");
                _config.SetValue("Display", "HUDFont", "National");
                _config.SetValue("Display", "HUDFormat", "Time/Date");
                _config.SetValue("Controller", "Bindings", "Keyboard");
                _config.SetValue("Gameplay", "Mode", "Quality");
                _config.SetValue("Gameplay", "Year", "Historical");
                _config.SetValue("Gameplay", "Opponent", "Historical");
                _config.SetValue("Gameplay", "Zone", "Nation");
                _config.SetValue("Gameplay", "Purchase", "Historical");
                _config.SetValue("Gameplay", "MoveSpeed", "Normal");
                _config.SetValue("Gameplay", "CombatSpeed", "Normal");
                _config.SetValue("Gameplay", "TransitionSpeed", "Normal");
                _config.SetValue("Gameplay", "VoiceOver", "Normal");
                _config.Save(_activeConfigPath);
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in WriteDefaultConfig: {ex.Message}");
            }
        }

        /*        private void TransitionToIntroAnim()
                {
                    try
                    {
                       // if (_introAnimScene is PackedScene packedScene && packedScene.CanInstantiate())
                        //{
                            //Fadeout_Player = GetNode<AnimationPlayer>("FadeoutPlayer");
                            var startTime = Time.GetTicksMsec();
                            GD.Print("Attempting scene transition to IntroAnim.tscn");
                            //Fadeout_Player.Play("Fadeout");
                            //GetTree().ChangeSceneToPacked(packedScene);
                            GetTree().ChangeSceneToFile("res://Scenes/IntroAnim.tscn");

                            GD.Print($"Scene transition took {Time.GetTicksMsec() - startTime} ms");
                            // No QueueFree; Loading.tscn persists until IntroAnim cleanup
                        //}
                        //else
                        //{
                        //    GD.PushError("Preloaded IntroAnim.tscn is not valid or cannot be instantiated");
                        //    GetTree().ChangeSceneToFile("res://Scenes/IntroAnim.tscn"); // Fallback
                        //}
                    }
                    catch (Exception ex)
                    {
                        GD.PushError($"Error in TransitionToIntroAnim: {ex.Message}");
                    }
                }*/
    }
}