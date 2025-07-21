using Godot;
using System;
using WarEaglesDigital.Scripts; // Importing the namespace for consistency


namespace WarEaglesDigital.Scripts // Handles the splash menu scene
{
    public partial class SplashMenu : Control
    {
    [Export] private Label _versionLabel;
    private AudioManager MusicBox;
    private const string CreditsMenuScene = "res://Scenes/CreditRoll.tscn";

    public override void _Ready()
    {

            try
            {
                MusicBox = GetNode<AudioManager>("/root/AudioManager");

                ///Set MusicBox volume
                var MusicBus = AudioServer.GetBusIndex("Music");
                AudioServer.SetBusVolumeDb(MusicBus, 8.0f); // Set volume to 8 dB


                // Connect the CreditsButton's pressed signal
                var creditsButton = GetNode<Button>("Splashscreen/MainMenu/CreditsButton");
                creditsButton.Pressed += OnCreditsButtonPressed;

                var optionsScene = GD.Load<PackedScene>("res://Scenes/Options.tscn");
                if (optionsScene == null)
                {
                    GD.PrintErr("Failed to preload Options.tscn.");
                    return;
                }
                var optionsInstance = optionsScene.Instantiate();
                AddChild(optionsInstance);
                if (optionsInstance is Control optionsControl)
                    optionsControl.Visible = false; // Hide by default

                // Connect OptionsButton signal
                var optionsButton = GetNode<MenuButton>("Splashscreen/MainMenu/OptionsButton");
                optionsButton.GetPopup().IndexPressed += OnOptionsButtonItemSelected;

                // Connect the ExtrasButton's pressed signal
                var extrasButton = GetNode<Button>("Splashscreen/MainMenu/ExtrasButton");
                extrasButton.Pressed += OnExtrasButtonPressed;

                // Connect the QuitButton's pressed signal
                var quitButton = GetNode<Button>("Splashscreen/MainMenu/QuitButton");
                quitButton.Pressed += OnQuitButtonPressed;

                try
                {
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
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to set version label in IntroAnim: {ex.Message}");
                }

                GD.Print("SplashMenu is ready.");

            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in SplashMenu _Ready: {ex.Message}");
            }
            }

    public void ReleaseResources()
    {
        try
        {
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

            //GD.Print($"Memory after free: {OS.GetStaticMemoryUsage() / 1024 / 1024} MB");

        }
        catch (Exception ex)
        {
            GD.PrintErr($"Exception in SplashMenu.ReleaseResources(): {ex.Message}");
        }
    }

    private void OnQuitButtonPressed()
    {
        try
        {

            // Release resources before quitting
            ReleaseResources();
            MusicBox.StopMusic();

            GD.Print("Closing Game.");
            GetTree().Quit();
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
            //Release resources before opening Credits Menu
            ReleaseResources();
            //GD.Print("Releasing resources before opening Credits Menu.");

            //GD.Print("Opening Credits Menu");
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

                // Show the selected panel based on index
                switch (index)
                {
                    case 1: // Video and Display
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
                    case 0: // Audio
                        var audioPanel = optionsNode.GetNode<Panel>("AudioMenuPanel");
                        if (audioPanel != null) audioPanel.Visible = true;
                        break;
                    case 2: // Controls
                        var controlsPanel = optionsNode.GetNode<Panel>("ControlsPanel");
                        if (controlsPanel != null) controlsPanel.Visible = true;
                        break;
                    case 3: // Game Settings
                        var gameplayPanel = optionsNode.GetNode<Panel>("GameplayPanel");
                        if (gameplayPanel != null) gameplayPanel.Visible = true;
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
            ReleaseResources();

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

