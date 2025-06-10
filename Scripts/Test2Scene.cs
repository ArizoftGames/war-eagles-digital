using Godot;
using System;
using WarEaglesDigital.Scripts;

public partial class Test2Scene : Node3D
{
    private AudioManager _audioManager;

    public override void _Ready()
    {
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        // Connect button signals
        GetNode<Button>("UI/PlayUSConfident").Pressed += OnPlayUSConfidentPressed;
        GetNode<Button>("UI/PlayOpenBattleBoard").Pressed += OnPlayOpenBattleBoardPressed;
        GetNode<Button>("UI/PlayRadialDive").Pressed += OnPlayRadialDivePressed;
        GetNode<Button>("UI/PlayAuto").Pressed += OnPlayAutoPressed;
        GetNode<Button>("UI/PlayTypewriter").Pressed += OnPlayTypewriterPressed;
    }

    private void OnPlayUSConfidentPressed()
    {
        try
        {
            _audioManager.PlayMusicByNationMood("USA", "Confident");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to play US Confident music: {ex.Message}");
        }
    }

    private void OnPlayOpenBattleBoardPressed()
    {
        try
        {
            _audioManager.PlayMusicByUseCase("Open Battle Board");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to play Open Battle Board music: {ex.Message}");
        }
    }

    private void OnPlayRadialDivePressed()
    {
        try
        {
            _audioManager.PlaySoundEffect("radialDive");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to play radialDive effect: {ex.Message}");
        }
    }

    private void OnPlayAutoPressed()
    {
        try
        {
            _audioManager.PlaySoundEffect("Auto");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to play Auto effect: {ex.Message}");
        }
    }

    private void OnPlayTypewriterPressed()
    {
        try
        {
            _audioManager.PlaySoundEffect("typewriter");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to play typewriter effect: {ex.Message}");
        }
    }
}