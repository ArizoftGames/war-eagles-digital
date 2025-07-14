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

