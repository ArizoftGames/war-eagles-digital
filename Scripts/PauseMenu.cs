using Godot;
using System;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts //Handles the pause menu
{
    public partial class PauseMenu : Node3D
    {
        private const string CreditsMenuScene = "res://Scenes/CreditRoll.tscn";
        [Export] private Label _versionLabel;
        private Control _optionsInstance;

        public override void _Ready()
        {
            try
            {
                GD.Print("PauseMenu: _Ready called");

                // Connect existing signals
                var creditsButton = GetNode<Button>("Pause_Menu/MainMenu/CreditsButton");
                creditsButton.Pressed += OnCreditsButtonPressed;

                var extrasButton = GetNode<Button>("Pause_Menu/MainMenu/ExtrasButton");
                extrasButton.Pressed += OnExtrasButtonPressed;

                // Preload and cache Options.tscn
                if (_optionsInstance == null)
                {
                    var optionsScene = GD.Load<PackedScene>("res://Scenes/Options.tscn");
                    if (optionsScene == null)
                    {
                        GD.PrintErr("Failed to preload Options.tscn.");
                        return;
                    }
                    _optionsInstance = optionsScene.Instantiate() as Control;
                    if (_optionsInstance == null)
                    {
                        GD.PrintErr("Failed to instantiate Options.tscn as Control.");
                        return;
                    }
                    AddChild(_optionsInstance);
                    _optionsInstance.Owner = this;
                    _optionsInstance.Visible = false;
                    GD.Print("Options instance cached: ", _optionsInstance);
                }
                else
                {
                    GD.Print("Options instance already cached: ", _optionsInstance);
                }

                // Connect OptionsButton signal
                var optionsButton = GetNode<MenuButton>("Pause_Menu/MainMenu/OptionsButton");
                var popup = optionsButton.GetPopup();
                optionsButton.GetPopup().IndexPressed += OnOptionsButtonItemSelected;

                // Apply Versioning info
                if (_versionLabel != null)
                {
                    string version = ProjectSettings.GetSetting("application/config/version").AsString();
                    int buildNumber = ProjectSettings.GetSetting("application/config/build_number").AsInt32();
                    _versionLabel.Text = $"Version: {version} Build {buildNumber}";
                }
                else
                {
                    GD.PrintErr("VersionLabel node not found in PauseMenu.tscn.");
                }

                // Connect static buttons' audio
                GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in PauseMenu._Ready: {ex.Message}");
            }
        }

        private void OnCreditsButtonPressed()
        {
            try
            {
                GD.Print("Opening Credits Menu");
                GetTree().ChangeSceneToFile(CreditsMenuScene);
            }
            catch (Exception)
            {
                GD.PrintErr("Failed to open Credits Menu.");
            }
        }

        private void OnOptionsButtonItemSelected(long index)
        {
            try
            {
                if (_optionsInstance == null)
                {
                    GD.PrintErr("Options instance not found.");
                    return;
                }
                GD.Print("Options node found: ", _optionsInstance);
                _optionsInstance.Visible = true; // Show Options root

                // Hide all panels initially
                foreach (Node child in _optionsInstance.GetChildren())
                {
                    if (child is Panel panel)
                        panel.Visible = false;
                }
                GD.Print("Selected index: ", index);

                // Show the selected panel based on index
                switch (index)
                {
                    case 0: // Video and Display
                        var displayPanel = _optionsInstance.GetNode<Panel>("DisplayMenuPanel");
                        if (displayPanel != null)
                        {
                            GD.Print("DisplayPanel found: ", displayPanel);
                            displayPanel.Visible = true;
                            GD.Print("DisplayPanel visible: ", displayPanel.Visible);
                            if (displayPanel is DisplayMenuPanel displayMenu)
                                displayMenu.InitializeDisplayMenu("Video and Display");
                            else
                                GD.PrintErr("DisplayMenuPanel script not attached to DisplayMenuPanel node.");
                            GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");
                        }
                        break;
                    case 1: // Audio
                        var audioPanel = _optionsInstance.GetNode<Panel>("AudioMenuPanel");
                        GD.Print("audioPanel: ", _optionsInstance.GetNodeOrNull<Panel>("AudioMenuPanel"));
                        if (audioPanel != null)
                        {
                            GD.Print("AudioPanel found: ", audioPanel);
                            audioPanel.Visible = true;
                            GD.Print("AudioPanel visible: ", audioPanel.Visible);
                            if (audioPanel is AudioMenuPanel audioMenu)
                                audioMenu.InitializeAudioMenu("Audio");
                            else
                                GD.PrintErr("AudioMenuPanel script not attached to AudioMenuPanel node.");
                            GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");
                        }
                        break;
                    case 2: // Controls
                        GD.Print("Controls branch NYI: ControlsMenuPanel.cs not implemented.");
                        break;
                    case 3: // Game Settings
                        GD.Print("Gameplay branch NYI: GameplayMenuPanel.cs not implemented.");
                        break;
                }

                // Restore focus to ContinueButton
                var continueButton = GetNode<Button>("Pause_Menu/MainMenu/ContinueButton");
                if (continueButton != null)
                    continueButton.GrabFocus();
                else
                    GD.PrintErr("ContinueButton not found for focus restoration.");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in OnOptionsButtonItemSelected: {ex.Message}");
            }
        }

        public void OnExtrasButtonPressed()
        {
            try
            {
                GD.Print("Opening Extras Menu");
                GetTree().ChangeSceneToFile("res://Scenes/Extras.tscn");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to open Extras Menu: {ex.Message}");
            }
        }

        private void OnQuitButtonPressed()
        {
            try
            {
                foreach (var node in GetTree().GetNodesInGroup("glb_models"))
                    (node as Node)?.QueueFree();

                foreach (var node in GetTree().GetNodesInGroup("audio_players"))
                {
                    node.Call("stop");
                    node.Set("stream", (Godot.Resource)null);
                    (node as Node)?.QueueFree();
                }

                foreach (var node in GetTree().GetNodesInGroup("terrains"))
                    (node as Node)?.QueueFree();

                GD.Print("Closing Game.");
                GetTree().Quit();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in OnQuitButtonPressed: {ex.Message}");
            }
        }
    }
}