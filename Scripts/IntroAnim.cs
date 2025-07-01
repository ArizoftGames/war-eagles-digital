using Godot;
using WarEaglesDigital.Scripts;
using System;

namespace WarEaglesDigital.Scripts //Handles the introductory sequence scene
{
    public partial class IntroAnim : Node3D
    {
        private AnimationTree animation_Tree;
        private AnimationNodeStateMachinePlayback state_machine;

        public override void _Ready()
        {
            animation_Tree = GetNode<AnimationTree>("AnimationTree");
            state_machine = (AnimationNodeStateMachinePlayback)animation_Tree.Get("parameters/playback");
            
            // Connect the SkipButton's pressed signal
            var skipButton = GetNode<Button>("SkipButton");
            skipButton.Pressed += OnSkipButtonPressed;
            

            GD.Print("IntroAnim initialized successfully");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
            {
                SkipIntro();
            }
        }

        private void OnSkipButtonPressed()
        {
            SkipIntro();
        }

        private void SkipIntro()
        {
            GD.Print("Skip Triggered");
            // Advance AnimationTree to RESET state via AtEnd transition
            state_machine.Start("RESET");
                    }

        public void OnQuitButtonPressed() //Quits game from Main Menu
        {
            GD.Print("Closing game");
            GetTree().Quit();
        }
    }
}