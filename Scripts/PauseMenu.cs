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

        public void OnQuitButtonPressed()//Quits game from Main Menu
        {
        GD.Print("Closing game");
        GetTree().Quit();
        }

    }
}
