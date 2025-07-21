using Godot;
using System;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts //Handles the pause menu
{ 
    public partial class PauseMenu : Node3D
    {
        private const string CreditsMenuScene = "res://Scenes/CreditRoll.tscn";
        [Export] private Label _versionLabel;

        public override void _Ready()
        {
            try
            {
                // Connect existing signals
                var quitButton = GetNode<Button>("Pause_Menu/MainMenu/QuitButton");
                quitButton.Pressed += OnQuitButtonPressed;

                var creditsButton = GetNode<Button>("Pause_Menu/MainMenu/CreditsButton");
                creditsButton.Pressed += OnCreditsButtonPressed;

                var extrasButton = GetNode<Button>("Pause_Menu/MainMenu/ExtrasButton");
                extrasButton.Pressed += OnExtrasButtonPressed;

                // Preload Options.tscn
                var optionsScene = GD.Load<PackedScene>("res://Scenes/Options.tscn");
                if (optionsScene == null)
                {
                    GD.PrintErr("Failed to preload Options.tscn.");
                    return;
                }
                var optionsInstance = optionsScene.Instantiate();
                AddChild(optionsInstance);
                GD.Print("Options children: ", optionsInstance.GetChildren());
                if (optionsInstance is Control optionsControl)
                    optionsControl.Visible = false; // Hide by default

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
                var optionsNode = GetNode<Control>("Control");
                if (optionsNode == null)
                {
                    GD.PrintErr("Control node not found.");
                    return;
                }
                GD.Print("Options node found: ", optionsNode);
                optionsNode.Show(); // Ensure root is visible

                // Hide all panels initially
                foreach (Node child in optionsNode.GetChildren())
                {
                    if (child is Panel panel)
                        panel.Visible = false;
                }
                GD.Print("Selected index: ", index);
                // Show the selected panel based on index
                switch (index)
                {
                    
                    case 0: // Video and Display
                        var displayPanel = optionsNode.GetNode<Panel>("DisplayMenuPanel");
                        if (displayPanel != null)
                        {
                            GD.Print("DisplayPanel found: ", displayPanel);
                            displayPanel.Show();
                            GD.Print("DisplayPanel visible: ", displayPanel.Visible);
                            if (displayPanel is DisplayMenuPanel displayMenu)
                                displayMenu.InitializeDisplayMenu("Video and Display");
                            else
                                GD.PrintErr("DisplayMenuPanel script not attached to DisplayMenuPanel node.");
                        }
                        break;
                    case 1: // Audio
                        var audioPanel = optionsNode.GetNode<Panel>("AudioMenuPanel");
                        GD.Print("audioPanel: ", optionsNode.GetNodeOrNull<Panel>("AudioMenuPanel"));
                        if (audioPanel != null)
                        {
                            GD.Print("AudioPanel found: ", audioPanel);
                            audioPanel.Show();
                            GD.Print("AudioPanel visible: ", audioPanel.Visible);
                            if (audioPanel is AudioMenuPanel audioMenu)
                                audioMenu.InitializeAudioMenu("Audio");
                            else
                                GD.PrintErr("AudioMenuPanel script not attached to AudioMenuPanel node.");
                        }
                        break;
                    case 2: // Controls
                        GD.Print("Controls branch NYI: ControlsMenuPanel.cs not implemented.");
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
                //Loads Extras.tscn
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

                //GD.Print($"Memory before free: {OS.GetStaticMemoryUsage() / 1024 / 1024} MB");
                // Godot's Array does not have ForEach, use a regular foreach loop
                foreach (var node in GetTree().GetNodesInGroup("glb_models"))
                    (node as Node)?.QueueFree();

                foreach (var node in GetTree().GetNodesInGroup("audio_players"))
                {
                    node.Call("stop");
                    node.Set("stream", (Godot.Resource)null); // Correct way to clear the stream in Godot 4.x C#
                    (node as Node)?.QueueFree();
                }

                foreach (var node in GetTree().GetNodesInGroup("terrains"))
                    (node as Node)?.QueueFree();

                //GD.Print($"Memory before quit: {OS.GetStaticMemoryUsage() / 1024 / 1024} MB");

                GD.Print("Closing Game.");
                GetTree().Quit();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in OnQuitButtonPressed: {ex.Message}");
            }
        }

       /* private void InitializeOptionsPopup()
        {
            try
            {
                var optionsButton = GetNode<MenuButton>("Pause_Menu/MainMenu/OptionsButton");
                var popup = optionsButton.GetPopup();
                popup.Clear();
                popup.AddItem("Options", 4, true); // Separator label
                popup.AddItem("Video and Display", 1);
                popup.AddItem("Audio", 0);
                popup.AddItem("Controls", 2);
                popup.AddItem("Game Settings", 3);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in InitializeOptionsPopup: {ex.Message}");
            }
        }*/

    }
}
