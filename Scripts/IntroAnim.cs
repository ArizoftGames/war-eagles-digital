using Godot;
using WarEaglesDigital.Scripts;
using System;

namespace WarEaglesDigital.Scripts //Handles the introductory sequence scene
{
    public partial class IntroAnim : Node3D
    {
        [Export] private Label _versionLabel;
        private AnimationTree animation_Tree;
        private AnimationNodeStateMachinePlayback state_machine;
        private AudioManager MusicBox;
        
        public override void _Ready()
        {
            var startTime = Time.GetTicksMsec();
            GD.Print($"IntroAnim _Ready started at {startTime} ms");

            animation_Tree = GetNode<AnimationTree>("AnimationTree");
            state_machine = (AnimationNodeStateMachinePlayback)animation_Tree.Get("parameters/playback");
            MusicBox = GetNode<AudioManager>("/root/AudioManager");

            ///Set MusicBox volume
            var MusicBus = AudioServer.GetBusIndex("Music");
            AudioServer.SetBusVolumeDb(MusicBus, 8.0f); // Set volume to 8 dB

            // Connect the SkipButton's pressed signal
            var skipButton = GetNode<Button>("SkipButton");
            skipButton.Pressed += OnSkipButtonPressed;

            //Connect Intro_Animation start signal
            animation_Tree.AnimationStarted += OnIntro_AnimationStarted;

            //Connect RESET finished signal
            animation_Tree.AnimationFinished += OnRESETAnimationFinished;

            //Start AnimationTree
            animation_Tree.Active = true;

            //Get _versionLabel settings
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
                    GD.PrintErr("VersionLabel node not found in IntroAnim.tscn.");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to set version label in IntroAnim: {ex.Message}");
            }

            GD.Print($"IntroAnim _Ready completed in {Time.GetTicksMsec() - startTime} ms");
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

        public void ReleaseResources()
        {
            try
            {
                // Free nodes in groups
                foreach (var node in GetTree().GetNodesInGroup("glb_models"))
                    (node as Node)?.QueueFree();

                foreach (var node in GetTree().GetNodesInGroup("audio_players"))
                {
                    node.Call("stop");
                    node.Set("stream", (Godot.Resource)null); // Clear stream in Godot 4.x
                    (node as Node)?.QueueFree();
                }

                foreach (var node in GetTree().GetNodesInGroup("terrains"))
                    (node as Node)?.QueueFree();

                // Free Loading.tscn if present
                var loadingNode = GetTree().Root.GetNodeOrNull<Node>("Loading");
                if (loadingNode != null)
                {
                    loadingNode.QueueFree();
                    GD.Print("Loading.tscn freed during IntroAnim cleanup");
                }

                GD.Print($"Memory after free: {OS.GetStaticMemoryUsage() / 1024 / 1024} MB");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in ReleaseResources: {ex.Message}");
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

        private void OnRESETAnimationFinished(StringName animName)
        {
            if (animName == "RESET")
            {
                //OnAnimationFinished();
                try
                {

                    // Release resources before quitting
                    ReleaseResources();
                    //MusicBox.StopMusic();
                    GetTree().ChangeSceneToFile("res://Scenes/SplashMenu.tscn"); // Change to Splash Menu scene

                    //GD.Print("Closing Game.");
                    //GetTree().Quit();
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Exception in OnAnimationFinished: {ex.Message}");
                }
            }
            else
            {
                // GD.PrintErr($"Unexpected animation finished: {animName}");
            }
        }


        private void SkipIntro()
        {
            try 
            { 
            // Advance AnimationTree to RESET state via AtEnd transition
            state_machine.Start("RESET");
            GD.Print("Skip Triggered");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error skipping intro: {ex.Message}");
            }
        }

        
    }
}