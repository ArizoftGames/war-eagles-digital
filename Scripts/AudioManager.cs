using Godot;
using System;
using System.Collections.Generic;

namespace WarEaglesDigital.Scripts
{
    public partial class AudioManager : Node
    {
        private readonly Dictionary<string, AudioStreamPlayer> _musicPlayers = [];
        private readonly Dictionary<string, AudioStreamPlayer> _effectPlayers = [];
        private string _currentMusicKey;
        private AudioStreamPlayer _currentMusicPlayer;

        [Export(PropertyHint.None, "Tooltip: Adjust music volume (0.0 to 1.0).")]
        public float MusicVolume { get; set; } = 0.7f;

        [Export(PropertyHint.None, "Tooltip: Adjust sound effect volume (0.0 to 1.0).")]
        public float EffectVolume { get; set; } = 1.0f;

        public override void _Ready()
        {
            try
            {
                LoadMusicTracks();
                LoadSoundEffects();
                UpdateVolumes();
                //PlayMusicByUseCase("Start Game"); // Default
                GD.Print("AudioManager initialized successfully.");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"AudioManager initialization failed: {ex.Message}");
            }
        }

        private void LoadMusicTracks()
        {
            string csvPath = "res://Data/RawData/Music.csv";
            using var file = FileAccess.Open(csvPath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Failed to open {csvPath}");
                return;
            }

            file.GetLine(); // Skip header
            while (!file.EofReached())
            {
                string line = file.GetLine();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    var values = line.Split(",");
                    if (values.Length < 5)
                    {
                        GD.PrintErr($"Invalid music row: {line}");
                        continue;
                    }

                    var dict = new Godot.Collections.Dictionary<string, string>
                    {
                        { "Nationality", values[0].Trim() },
                        { "Mood", values[1].Trim() },
                        { "Use Case", values[2].Trim() },
                        { "Title", values[3].Trim() },
                        { "Filename", values[4].Trim() }
                    };

                    string nationality = dict["Nationality"];
                    string mood = dict["Mood"];
                    string useCase = dict["Use Case"];
                    string filename = dict["Filename"];

                    if (string.IsNullOrEmpty(filename)) continue;

                    string key = string.IsNullOrEmpty(useCase) ? $"{nationality}{mood}" : useCase.Replace(" ", "");
                    var player = new AudioStreamPlayer
                    {
                        Stream = GD.Load<AudioStream>($"res://Audio/Music/{filename}")
                    };
                    player.Stream.Set("loop", true); // Ensure looping for music
                    player.Name = key;
                    AddChild(player);
                    _musicPlayers[key] = player;

                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to load music row: {line}. Error: {ex.Message}");
                }
            }
        }

        private void LoadSoundEffects()
        {
            string csvPath = "res://Data/RawData/SoundEffects.csv";
            using var file = FileAccess.Open(csvPath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Failed to open {csvPath}");
                return;
            }

            file.GetLine(); // Skip header
            while (!file.EofReached())
            {
                string line = file.GetLine();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    var values = line.Split(",");
                    if (values.Length < 6)
                    {
                        GD.PrintErr($"Invalid sound effect row: {line}");
                        continue;
                    }

                    var dict = new Godot.Collections.Dictionary<string, string>
                    {
                        { "Motor", values[0].Trim() },
                        { "Status", values[1].Trim() },
                        { "Gun", values[2].Trim() },
                        { "Sound", values[3].Trim() },
                        { "Bomb", values[4].Trim() },
                        { "Filename", values[5].Trim() }
                    };

                    string motor = dict["Motor"];
                    string status = dict["Status"];
                    string gun = dict["Gun"];
                    string sound = dict["Sound"];
                    string bomb = dict["Bomb"];
                    string filename = dict["Filename"];

                    if (string.IsNullOrEmpty(filename)) continue;

                    string key = string.IsNullOrEmpty(motor) ? (string.IsNullOrEmpty(gun) ? (string.IsNullOrEmpty(sound) ? (string.IsNullOrEmpty(bomb) ? filename : bomb) : sound) : gun) : $"{motor}{status}";
                    var player = new AudioStreamPlayer
                    {
                        Stream = GD.Load<AudioStream>($"res://audio/effects/{filename}"),
                        Name = key,
                        VolumeDb = Mathf.LinearToDb(EffectVolume)
                    };
                    AddChild(player);
                    _effectPlayers[key] = player;

                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to load effect row: {line}. Error: {ex.Message}");
                }
            }
        }

        // Play music for a nation and mood (e.g., called by GameManager on AI mood change, per AIMoodDefinitions.csv)
        public void PlayMusicByNationMood(string nationality, string mood)
        {
            try
            {
                StopMusic();
                string key = $"{nationality}{mood}";
                if (_musicPlayers.TryGetValue(key, out var player))
                {
                    _currentMusicPlayer = player;
                    _currentMusicPlayer.Play();
                    _currentMusicKey = key;
                    GD.Print($"Playing music: {nationality} {mood} ({key})");
                }
                else
                {
                    GD.PrintErr($"Music track not found for {nationality} {mood}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to play music {nationality} {mood}: {ex.Message}");
            }
        }

        // Play music for a use case (e.g., called by GameManager for "Open Credits Animation")
        public void PlayMusicByUseCase(string useCase)
        {
            try
            {
                StopMusic();
                string key = useCase.Replace(" ", "");
                if (_musicPlayers.TryGetValue(key, out var player))
                {
                    _currentMusicPlayer = player;
                    _currentMusicPlayer.Play();
                    _currentMusicKey = key;
                    GD.Print($"Playing music: {useCase} ({key})");
                }
                else
                {
                    GD.PrintErr($"Music track not found for use case {useCase}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to play music for use case {useCase}: {ex.Message}");
            }
        }

        // Play sound effect (e.g., called by GameManager for AirUnit.Motor+Status like "radialDive",
        // AntiAircraftUnit.Gun like "Auto", Event.Sound like "typewriter", or new effects like "flakBurst")
        public void PlaySoundEffect(string key)
        {
            try
            {
                if (_effectPlayers.TryGetValue(key, out var player))
                {
                    player.Play();
                    GD.Print($"Playing effect: {key}");
                }
                else
                {
                    GD.PrintErr($"Sound effect not found: {key}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to play effect {key}: {ex.Message}");
            }
        }

        // Stop current music playback
        public void StopMusic()
        {
            if (_currentMusicPlayer != null)
            {
                _currentMusicPlayer.Stop();
                _currentMusicKey = null;
                _currentMusicPlayer = null;
                GD.Print("Stopped music playback.");
            }
        }

        // Update volumes for all players (called dynamically for UI changes)
        private void UpdateVolumes()
        {
            foreach (var player in _musicPlayers.Values)
            {
                player.VolumeDb = Mathf.LinearToDb(MusicVolume);
            }
            foreach (var player in _effectPlayers.Values)
            {
                player.VolumeDb = Mathf.LinearToDb(EffectVolume);
            }
        }

        public override void _Process(double delta)
        {
            UpdateVolumes();
        }
    }
}