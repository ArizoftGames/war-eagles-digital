using Godot;
using WarEaglesDigital.Scripts;
using System;

namespace WarEaglesDigital.Scripts //Handles the introductory sequence scene
{
    public partial class IntroAnim : Node3D
    {
        private AnimationTree animation_Tree;
        private AnimationNodeStateMachinePlayback state_machine;
        private AudioManager MusicBox;

        public override void _Ready()
        {
            animation_Tree = GetNode<AnimationTree>("AnimationTree");
            state_machine = (AnimationNodeStateMachinePlayback)animation_Tree.Get("parameters/playback");
            MusicBox = GetNode<AudioManager>("/root/AudioManager");

            // Connect the SkipButton's pressed signal
            var skipButton = GetNode<Button>("SkipButton");
            skipButton.Pressed += OnSkipButtonPressed;

            //Connect Intro_Animation start signal
            animation_Tree.AnimationStarted += OnIntro_AnimationStarted;

            //Set MusicBox Volume
            MusicBox.MusicVolume = 1;
            //var vol = MusicBox.MusicVolume.ToString();
            //GD.Print($"MusicBox Volume set to {vol}");

            //Start AnimationTree
            animation_Tree.Active = true;


            GD.Print("IntroAnim initialized successfully.");
        }

        private void OnIntro_AnimationStarted(StringName animName)
        {
            if (animName == "Intro_Animation")
            {
                try
                { 
                //GD.Print("MusicBox playing IntroTheme.");
                MusicBox.PlayMusicByUseCase("Start Game"); // Play intro music
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error playing intro music: {e.Message}");
                }
            }
            else
                {
                
                }
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
            //GD.Print("Skip Triggered");
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