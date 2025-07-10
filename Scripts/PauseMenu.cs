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
            //Connect signal from QuitButton
            var quitButton = GetNode<Button>("Pause_Menu/MainMenu/QuitButton");
            quitButton.Pressed += OnQuitButtonPressed;

            //Connect signal from CreditsButton
            var creditsButton = GetNode<Button>("Pause_Menu/MainMenu/CreditsButton");
            creditsButton.Pressed += OnCreditsButtonPressed;

            //Connect signal from ExtrasButton
            var extrasButton = GetNode<Button>("Pause_Menu/MainMenu/ExtrasButton");
            extrasButton.Pressed += OnExtrasButtonPressed;

            //Apply Versioning info

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
                    GD.PrintErr("VersionLabel node not found in PauseMenu.tscn.");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to set version label in PauseMenu: {ex.Message}");
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

    }
}
