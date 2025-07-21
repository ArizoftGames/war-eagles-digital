using Godot;
using System;
using System.Collections.Generic;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts
{
    /// <summary>
    /// AudioMenuPanel: Populates the audio options menu programmatically, manages bus volumes, previews, and config saving.
    /// </summary>
    public partial class AudioMenuPanel : Panel
    {
        // Node references
        private VSlider _masterSlider;
        private VSlider _musicSlider;
        private VSlider _effectsSlider;
        private VSlider _voiceSlider;
        private Button _masterPreviewButton;
        private Button _musicPreviewButton;
        private Button _effectsPreviewButton;
        private Button _voicePreviewButton;
        private Button _acceptButton;
        private Button _cancelButton;
        // Update field at line 17 (after private Button _cancelButton;)
        private ConfirmationDialog _acceptDialog;

        // Managers
        private AudioManager _audioManager;
        private ConfigFile _config;

        // Cache initial bus volumes
        private Dictionary<string, float> _initialVolumes = new();

        public override void _Ready()
        {
            try
            {
                // Get AudioManager singleton
                _audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
                if (_audioManager == null)
                {
                    GD.PrintErr("AudioMenuPanel: AudioManager node missing");
                    return;
                }

                // Load config
                _config = new ConfigFile();
                var config_path = "user://War Eagles/config.cfg";
                if (FileAccess.FileExists(config_path))
                {
                    var err = _config.Load(config_path);
                    if (err != Error.Ok)
                        GD.PrintErr($"AudioMenuPanel: Config load error: {err}");
                }

                // Cache initial bus volumes (linear 0-1)
                _initialVolumes["Master"] = _config.HasSectionKey("audio", "master_volume")
                    ? _config.GetValue("audio", "master_volume").As<float>()
                    : AudioManager.GetBusVolume("Master");
                _initialVolumes["Music"] = _config.HasSectionKey("audio", "music_volume")
                    ? _config.GetValue("audio", "music_volume").As<float>()
                    : AudioManager.GetBusVolume("Music");
                _initialVolumes["Effects"] = _config.HasSectionKey("audio", "effects_volume")
                    ? _config.GetValue("audio", "effects_volume").As<float>()
                    : AudioManager.GetBusVolume("Effects");

                GD.Print("AudioMenuPanel: Initialized successfully in _Ready");
            }
            catch (Exception e)
            {
                GD.PrintErr("AudioMenuPanel._Ready: " + e.Message);
            }
        }

        private void PopulateAudioMenu()
        {
            try
            {
                // Get static AudioMenu container
                var audioMenu = GetNodeOrNull<VBoxContainer>("AudioMenu");
                if (audioMenu == null)
                {
                    GD.PrintErr("AudioMenuPanel: AudioMenu node missing");
                    return;
                }

                // Clear existing children to prevent duplicates
                foreach (Node child in audioMenu.GetChildren())
                    child.QueueFree();

                // --- Label ---
                var label = new Label
                {
                    Text = "Volume Levels",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                label.AddThemeFontSizeOverride("font_size", 56);
                audioMenu.AddChild(label);

                // --- Slider Labels ---
                var sliderLabels = new HBoxContainer
                {
                    CustomMinimumSize = new Vector2(780, 55),
                    SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ExpandFill
                };
                audioMenu.AddChild(sliderLabels);

                foreach (var name in new[] { "Master", "Music", "Effects", "Voice" })
                {
                    var lbl = new Label
                    {
                        Text = name.ToUpper(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        CustomMinimumSize = new Vector2(192, 55),
                        SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ExpandFill
                    };
                    sliderLabels.AddChild(lbl);
                }

                // --- Sliders ---
                var sliders = new HBoxContainer
                {
                    CustomMinimumSize = new Vector2(780, 256),
                    SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ExpandFill
                };
                audioMenu.AddChild(sliders);

                // Create sliders
                _masterSlider = new VSlider
                {
                    Name = "MasterSlider",
                    TickCount = 10,
                    TicksOnBorders = true,
                    MaxValue = 10,
                    CustomMinimumSize = new Vector2(192, 192),
                    SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ExpandFill
                };
                sliders.AddChild(_masterSlider);

                _musicSlider = new VSlider
                {
                    Name = "MusicSlider",
                    TickCount = 10,
                    TicksOnBorders = true,
                    MaxValue = 10,
                    CustomMinimumSize = new Vector2(192, 256),
                    SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ExpandFill
                };
                sliders.AddChild(_musicSlider);

                _effectsSlider = new VSlider
                {
                    Name = "EffectsSlider",
                    TickCount = 10,
                    TicksOnBorders = true,
                    MaxValue = 10,
                    CustomMinimumSize = new Vector2(192, 256),
                    SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ExpandFill
                };
                sliders.AddChild(_effectsSlider);

                _voiceSlider = new VSlider
                {
                    Name = "VoiceSlider",
                    TickCount = 10,
                    TicksOnBorders = true,
                    MaxValue = 10,
                    CustomMinimumSize = new Vector2(192, 156),
                    SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ExpandFill
                };
                sliders.AddChild(_voiceSlider);

                // Set initial slider values (linear 0-1 mapped to 0-10)
                _masterSlider.Value = LinearToSlider(_initialVolumes["Master"]);
                _musicSlider.Value = LinearToSlider(_initialVolumes["Music"]);
                _effectsSlider.Value = LinearToSlider(_initialVolumes["Effects"]);
                _voiceSlider.Value = 0; // NYI

                // Connect slider signals
                _masterSlider.ValueChanged += MasterVolumeChanged;
                _musicSlider.ValueChanged += MusicVolumeChanged;
                _effectsSlider.ValueChanged += EffectsVolumeChanged;
                _voiceSlider.ValueChanged += VoiceVolumeChanged;

                // --- Preview Buttons ---
                var previewButtons = new HBoxContainer
                {
                    CustomMinimumSize = new Vector2(780, 48),
                    SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ExpandFill
                };
                audioMenu.AddChild(previewButtons);

                _masterPreviewButton = CreatePreviewButton("MasterPreviewButton");
                _musicPreviewButton = CreatePreviewButton("MusicPreviewButton");
                _effectsPreviewButton = CreatePreviewButton("EffectsPreviewButton");
                _voicePreviewButton = CreatePreviewButton("VoicePreviewButton");
                previewButtons.AddChild(_masterPreviewButton);
                previewButtons.AddChild(_musicPreviewButton);
                previewButtons.AddChild(_effectsPreviewButton);
                previewButtons.AddChild(_voicePreviewButton);

                _masterPreviewButton.Pressed += MasterPreviewPressed;
                _musicPreviewButton.Pressed += MusicPreviewPressed;
                _effectsPreviewButton.Pressed += EffectsPreviewPressed;
                _voicePreviewButton.Pressed += VoicePreviewPressed;

                // --- Accept/Cancel Buttons ---
                var closeContainer = new HBoxContainer
                {
                    Alignment = (BoxContainer.AlignmentMode)HorizontalAlignment.Center,
                    CustomMinimumSize = new Vector2(780, 80),
                    SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ExpandFill,
                    SizeFlagsVertical = (SizeFlags)(int)Control.SizeFlags.ExpandFill
                };
                closeContainer.AddThemeConstantOverride("separation", 200);
                audioMenu.AddChild(closeContainer);

                _acceptButton = new Button
                {
                    Name = "AcceptButton",
                    Text = "Accept",
                    CustomMinimumSize = new Vector2(160, 60),
                    SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ShrinkCenter
                };
                closeContainer.AddChild(_acceptButton);

                _cancelButton = new Button
                {
                    Name = "CancelButton",
                    Text = "Cancel",
                    CustomMinimumSize = new Vector2(160, 60),
                    SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ShrinkCenter
                };
                closeContainer.AddChild(_cancelButton);

                _acceptButton.Pressed += AcceptButtonPressed;
                _cancelButton.Pressed += CancelPressed;

                GD.Print("AudioMenuPanel: Populated successfully in PopulateAudioMenu");
            }
            catch (Exception e)
            {
                GD.PrintErr("AudioMenuPanel.PopulateAudioMenu: " + e.Message);
            }
        }

        private Button CreatePreviewButton(string name)
        {
            var btn = new Button
            {
                Name = name,
                Text = "Preview",
                Flat = true,
                CustomMinimumSize = new Vector2(192, 48),
                SizeFlagsHorizontal = (SizeFlags)(int)Control.SizeFlags.ExpandFill,
                Alignment = HorizontalAlignment.Center
            };
            btn.AddThemeFontSizeOverride("font_size", 36);
            return btn;
        }

        private float LinearToSlider(float linearVolume)
        {
            // Convert AudioManager linear volume (0-1) to slider scale (0-10)
            return Mathf.Round(linearVolume * 10f);
        }

        private float SliderToLinear(double sliderValue)
        {
            // Convert slider value (0-10) to AudioManager linear volume (0-1)
            return (float)(sliderValue / 10.0);
        }

        private void MasterVolumeChanged(double value)
        {
            try
            {
                AudioManager.SetBusVolume("Master", SliderToLinear(value));
                GD.Print($"Master volume set to: {value}");
            }
            catch (Exception e)
            {
                GD.PrintErr("MasterVolumeChanged: " + e.Message);
            }
        }

        private void MusicVolumeChanged(double value)
        {
            try
            {
                AudioManager.SetBusVolume("Music", SliderToLinear(value));
                GD.Print($"Music volume set to: {value}");
            }
            catch (Exception e)
            {
                GD.PrintErr("MusicVolumeChanged: " + e.Message);
            }
        }

        private void EffectsVolumeChanged(double value)
        {
            try
            {
                AudioManager.SetBusVolume("Effects", SliderToLinear(value));
                GD.Print($"Effects volume set to: {value}");
            }
            catch (Exception e)
            {
                GD.PrintErr("EffectsVolumeChanged: " + e.Message);
            }
        }

        private void VoiceVolumeChanged(double value)
        {
            // NYI placeholder
            GD.Print("Voice NYI");
        }

        private async void MasterPreviewPressed()
        {
            try
            {
                _audioManager.PlayMusicByUseCase("preview");
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                _audioManager.PlaySoundEffect("thirty");
                GD.Print("Master preview played");
            }
            catch (Exception e)
            {
                GD.PrintErr("MasterPreviewPressed: " + e.Message);
            }
        }

        private void MusicPreviewPressed()
        {
            try
            {
                _audioManager.PlayMusicByUseCase("preview");
                GD.Print("Music preview played");
            }
            catch (Exception e)
            {
                GD.PrintErr("MusicPreviewPressed: " + e.Message);
            }
        }

        private void EffectsPreviewPressed()
        {
            try
            {
                _audioManager.PlaySoundEffect("thirty");
                GD.Print("Effects preview played");
            }
            catch (Exception e)
            {
                GD.PrintErr("EffectsPreviewPressed: " + e.Message);
            }
        }

        private void VoicePreviewPressed()
        {
            // NYI placeholder
            GD.Print("Voice NYI");
        }

        private void AcceptButtonPressed()
        {
            try
            {
                _acceptDialog = new ConfirmationDialog
                {
                    Title = "Confirm Audio Settings",
                    DialogText = "Apply these audio settings?"
                };
                
                _acceptDialog.Confirmed += AcceptConfirmed;
                AddChild(_acceptDialog);
                _acceptDialog.PopupCentered();
                GD.Print("Accept dialog shown");
            }
            catch (Exception e)
            {
                GD.PrintErr("AcceptButtonPressed: " + e.Message);
            }
        }

        private void AcceptConfirmed()
        {
            try
            {
                // Save settings to config.cfg
                _config.SetValue("audio", "master_volume", SliderToLinear(_masterSlider.Value));
                _config.SetValue("audio", "music_volume", SliderToLinear(_musicSlider.Value));
                _config.SetValue("audio", "effects_volume", SliderToLinear(_effectsSlider.Value));
                var err = _config.Save("user://War Eagles/config.cfg");
                if (err != Error.Ok)
                    GD.PrintErr($"AudioMenuPanel: Config save error: {err}");

                // Apply bus volumes (redundant but ensures sync)
                AudioManager.SetBusVolume("Master", SliderToLinear(_masterSlider.Value));
                AudioManager.SetBusVolume("Music", SliderToLinear(_musicSlider.Value));
                AudioManager.SetBusVolume("Effects", SliderToLinear(_effectsSlider.Value));

                // Stop any playing previews
                _audioManager.StopMusic();

                // Free ConfirmationDialog
                if (_acceptDialog != null)
                {
                    _acceptDialog.QueueFree();
                    _acceptDialog = null;
                    GD.Print("AudioMenuPanel: ConfirmationDialog freed");
                }

                // Reload PauseMenu
                GetTree().ChangeSceneToFile("res://Scenes/PauseMenu.tscn");
                GD.Print("Audio settings saved and PauseMenu reloaded");
            }
            catch (Exception e)
            {
                GD.PrintErr("AcceptConfirmed: " + e.Message);
            }
        }

        private void CancelPressed()
        {
            try
            {
                // Reset sliders to initial values
                _masterSlider.Value = LinearToSlider(_initialVolumes["Master"]);
                _musicSlider.Value = LinearToSlider(_initialVolumes["Music"]);
                _effectsSlider.Value = LinearToSlider(_initialVolumes["Effects"]);
                _voiceSlider.Value = 0; // NYI

                // Stop any playing previews
                _audioManager.StopMusic();

                // Reload PauseMenu
                GetTree().ChangeSceneToFile("res://Scenes/PauseMenu.tscn");
                GD.Print("Audio settings canceled and PauseMenu reloaded");
            }
            catch (Exception e)
            {
                GD.PrintErr("CancelPressed: " + e.Message);
            }
        }

        public void InitializeAudioMenu(string branch)
        {
            if (branch == "Audio")
            {
                PopulateAudioMenu();
                GD.Print("AudioMenuPanel: InitializeAudioMenu called with branch 'Audio'");
            }
            else
            {
                GD.Print($"AudioMenuPanel: Unknown branch '{branch}'");
            }
        }
    }
}