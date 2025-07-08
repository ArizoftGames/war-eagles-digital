using Godot;
using System;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts //Handles the pause menu
{ 
    public partial class PauseMenu : Node3D
    {
        private const string CreditsMenuScene = "res://Scenes/CreditRoll.tscn";

        public override void _Ready()
        {
            //Connect signal from QuitButton
            var quitButton = GetNode<Button>("Pause_Menu/MainMenu/QuitButton");
            quitButton.Pressed += OnQuitButtonPressed;

            //Connect signal from CreditsButton
            var creditsButton = GetNode<Button>("Pause_Menu/MainMenu/CreditsButton");
            creditsButton.Pressed += OnCreditsButtonPressed;

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
