using Godot;
using System;

public partial class IntroAnim : Node3D
{
    public void OnQuitButtonPressed()
    {
        GD.Print("Closing game");
        GetTree().Quit();
    }

}
