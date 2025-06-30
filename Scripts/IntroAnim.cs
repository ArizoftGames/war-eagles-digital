using Godot;
using System;

namespace WarEaglesDigital.Scripts //Handles the introductory sequence scene
{ 
public partial class IntroAnim : Node3D
{
    public void OnQuitButtonPressed()//Quits game from Main Menu
    {
        GD.Print("Closing game");
        GetTree().Quit();
    }

}
}
