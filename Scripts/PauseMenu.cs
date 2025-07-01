using Godot;
using System;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts //Handles the pause menu
{ 
    public partial class PauseMenu : Node3D
    {

        public override void _Ready()
        {
        //Connect signal from QuitButton
        var quitButton = GetNode<Button>("Pause_Menu/MainMenu/QuitButton");
            quitButton.Pressed += OnQuitButtonPressed;

        }
        public void OnQuitButtonPressed()//Quits game from Main Menu
        {
        GD.Print("Closing game");
        GetTree().Quit();
        }

    }
}
