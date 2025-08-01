using Godot;
using System;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts
{
    public partial class SplashMenu : Control
    {
        [Export] private Label _versionLabel;
        private AudioManager _musicManager;
        private const string CreditsMenuScene = "res://Scenes/CreditRoll.tscn";
        private GameManager _gameManager;
        

        public override void _Ready()
        {
            _gameManager = (GameManager)GetNodeOrNull<Node>("/root/GameManager");
            try
            {
                _musicManager = GetNode<AudioManager>("/root/MusicManager");

                // Set Music volume
                var musicBus = AudioServer.GetBusIndex("Music");
                AudioServer.SetBusVolumeDb(musicBus, 8.0f); // Set volume to 8 dB

                // Connect button signals
                var creditsButton = GetNode<Button>("Splashscreen/MainMenu/CreditsButton");
                creditsButton.Pressed += OnCreditsButtonPressed;

                var extrasButton = GetNode<Button>("Splashscreen/MainMenu/ExtrasButton");
                extrasButton.Pressed += OnExtrasButtonPressed;

                var quitButton = GetNode<Button>("Splashscreen/MainMenu/QuitButton");
                quitButton.Pressed += OnQuitButtonPressed;

                var optionsButton = GetNode<MenuButton>("Splashscreen/MainMenu/OptionsButton");
                optionsButton.GetPopup().IndexPressed += OnOptionsButtonItemSelected;

                // Preload Options.tscn
                var optionsScene = GD.Load<PackedScene>("res://Scenes/Options.tscn");
                if (optionsScene == null)
                {
                    GD.PrintErr("Failed to preload Options.tscn.");
                    return;
                }
                var optionsInstance = (Control)optionsScene.Instantiate();
                AddChild(optionsInstance);
                if (optionsInstance is Control optionsControl)
                    optionsControl.Visible = false; // Hide by default
                else
                {
                    GD.PrintErr("Failed to instantiate Options.tscn as Control.");
                    return;
                }

                // Apply Versioning info
                if (_versionLabel != null)
                {
                    string version = ProjectSettings.GetSetting("application/config/version").AsString();
                    int buildNumber = ProjectSettings.GetSetting("application/config/build_number").AsInt32();
                    _versionLabel.Text = $"Version: {version} Build {buildNumber}";
                }
                else
                {
                    GD.PrintErr("VersionLabel node not found in SplashMenu.tscn.");
                }

                // Connect static buttons' audio
                GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");

                GD.Print("SplashMenu is ready.");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in SplashMenu._Ready: {ex.Message}");
            }
        }

        /*public void ReleaseResources()
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
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in SplashMenu.ReleaseResources: {ex.Message}");
            }
        }*/

        private void OnQuitButtonPressed()
        {
            try
            {
                _gameManager.Call("ReleaseResources");
                _gameManager.Call("QuitGame");
                /*ReleaseResources();
                _musicManager?.StopMusic();
                GD.Print("Closing Game.");
                GetTree().Quit();*/
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in OnQuitButtonPressed: {ex.Message}");
            }
        }

        private void OnCreditsButtonPressed()
        {
            try
            {
                _gameManager.ReleaseResources();
                //ReleaseResources();
                GD.Print("Opening Credits Menu");
                GetTree().ChangeSceneToFile(CreditsMenuScene);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to open Credits Menu: {ex.Message}");
            }
        }

        private void OnOptionsButtonItemSelected(long index)
        {
            _gameManager?.ReleaseResources();
            try
            {
                var optionsNode = GetNode<Control>("Control");
                if (optionsNode == null)
                {
                    GD.PrintErr("Control node not found.");
                    return;
                }
                GD.Print("Options node found: ", optionsNode);
                optionsNode.Visible = true;

                foreach (Node child in optionsNode.GetChildren())
                {
                    if (child is Panel panel)
                        panel.Visible = false;
                }

                switch (index)
                {
                    case 0: // Video and Display
                        var displayPanel = optionsNode.GetNode<Panel>("DisplayMenuPanel");
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
                        var audioPanel = optionsNode.GetNode<Panel>("AudioMenuPanel");
                        GD.Print("audioPanel: ", optionsNode.GetNodeOrNull<Panel>("AudioMenuPanel"));
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

                        var controlsPanel = optionsNode.GetNode<Panel>("ControlsMenuPanel");
                        GD.Print("controlsPanel: ", optionsNode.GetNodeOrNull<Panel>("ControlsMenuPanel"));
                        if (controlsPanel != null)
                        {
                            GD.Print("controlsPanel found: ", controlsPanel);
                            controlsPanel.Visible = true;
                            GD.Print("controlsPanel visible: ", controlsPanel.Visible);
                            if (controlsPanel is ControlsMenuPanel controlMenu)
                                controlMenu.InitializeControlsMenu("Controls");
                            else
                                GD.PrintErr("ControlsMenuPanel script not attached to ControlsMenuPanel node.");
                            GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");
                        }
                        //GD.Print("Controls branch NYI: ControlsMenuPanel.cs not implemented.");
                        break;
                    case 3: // Game Settings
                        GD.Print("Gameplay branch NYI: GameplayMenuPanel.cs not implemented.");
                        break;
                }
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
                _gameManager.ReleaseResources();
                GD.Print("Opening Extras Menu");
                GetTree().ChangeSceneToFile("res://Scenes/Extras.tscn");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to open Extras Menu: {ex.Message}");
            }
        }
    }
}