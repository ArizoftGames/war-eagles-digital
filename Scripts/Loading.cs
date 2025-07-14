using Godot;
using System;
using System.Threading.Tasks;
using WarEaglesDigital.Scripts; // Importing the namespace for consistency

namespace WarEaglesDigital.Scripts // Handles the loading scene
{
    /// <summary>
    /// The Loading class represents a loading screen in the game.
    /// It inherits from Control, allowing it to be used as a UI element.
    /// </summary>
    /// <remarks>
    /// This class is part of the WarEaglesDigital project and is used to display a loading screen during game transitions.
    /// </remarks>

public partial class Loading : Control        
{

        public override void _Ready()
        {
            // Ready method is called when the node is added to the scene.

            WaitTimer(3); // Start the timer with a delay of 1 second
            

            GD.Print("Loading scene is ready.");
            
        }

        private async void WaitTimer(int delta)
        {
            //Simulate loading
            //var delayTime = delta * 1000;
            await ToSignal(GetTree().CreateTimer(delta), "timeout");
            // Simulate loading time
            // After loading is done, we can call a method to handle the next steps.
            OnLoadingFinished();
        }

        private void OnLoadingFinished()
        {
            // This method can be used to handle actions after loading is complete.
            GD.Print("Loading finished.");
            var introAnim = "res://Scenes/IntroAnim.tscn"; // Path to the IntroAnim scene
            GetTree().ChangeSceneToFile(introAnim);
        }
    }
}