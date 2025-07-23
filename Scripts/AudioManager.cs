using Godot;
using System;
using System.Collections.Generic;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts
{
    [GlobalClass]
    public partial class AudioManager : Node
    {
        private readonly Dictionary<string, AudioStreamPlayer> _musicPlayers = [];
        private readonly Dictionary<string, AudioStreamPlayer> _effectPlayers = [];
        private string _currentMusicKey;
        private AudioStreamPlayer _currentMusicPlayer;
        private readonly Callable _uiButtonCallable;
        private bool _isConnectingButtons = false;
        private readonly HashSet<string> _connectedButtons = [];

        private int _musicBusIndex = -1;
        private int _planesBusIndex = -1;
        private int _effectsBusIndex = -1;

        public AudioManager()
        {
            _uiButtonCallable = Callable.From(() => PlaySoundEffect("Keystroke"));
        }

        public override void _Ready()
        {
            try
            {
                // Load custom bus layout
                var busLayout = GD.Load<AudioBusLayout>("res://Audio/WE_Bus_layout.tres");
                if (busLayout == null)
                {
                    GD.PrintErr("Failed to load bus layout: res://Audio/WE_Bus_layout.tres");
                    return;
                }
                AudioServer.SetBusLayout(busLayout);

                // Initialize bus indices
                InitializeAudioBuses();
                LoadMusicTracks();
                LoadSoundEffects();
                GD.Print("AudioManager initialized successfully.");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"AudioManager initialization failed: {ex.Message}");
            }
        }

        private void InitializeAudioBuses()
        {
            try
            {
                _musicBusIndex = AudioServer.GetBusIndex("Music");
                _planesBusIndex = AudioServer.GetBusIndex("Planes");
                _effectsBusIndex = AudioServer.GetBusIndex("Effects");

                if (_musicBusIndex == -1)
                    GD.PrintErr("Music bus not found in res://Audio/WE_Bus_layout.tres.");
                if (_planesBusIndex == -1)
                    GD.PrintErr("Planes bus not found in res://Audio/WE_Bus_layout.tres.");
                if (_effectsBusIndex == -1)
                    GD.PrintErr("Effects bus not found in res://Audio/WE_Bus_layout.tres.");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to initialize audio buses: {ex.Message}");
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
                        Stream = GD.Load<AudioStream>($"res://Audio/Music/{filename}"),
                        Bus = "Music"
                    };
                    player.Stream.Set("loop", useCase != "preview");
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
                        Bus = "Effects"
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

        public void PlaySoundEffect(string key)
        {
            try
            {
                // Check global sound toggle
                var config = new ConfigFile();
                var configPath = "user://War Eagles/config.cfg";
                if (FileAccess.FileExists(configPath))
                {
                    var err = config.Load(configPath);
                    if (err == Error.Ok && config.HasSectionKey("Audio", "UIEffectsEnabled"))
                    {
                        if (!config.GetValue("Audio", "UIEffectsEnabled").As<bool>())
                        {
                            GD.Print("AudioManager: UI sound effects disabled, skipping Keystroke");
                            return;
                        }
                    }
                }

                if (_effectPlayers.TryGetValue(key, out var player))
                {
                    player.Play();
                    GD.Print($"Playing effect: {key} on bus {player.Bus}");
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

        public static void SetBusVolume(string busName, float linearVolume)
        {
            try
            {
                int busIndex = AudioServer.GetBusIndex(busName);
                if (busIndex == -1)
                {
                    GD.PrintErr($"Bus {busName} not found.");
                    return;
                }
                float dbVolume = linearVolume == 0 ? -80 : Mathf.LinearToDb(linearVolume);
                AudioServer.SetBusVolumeDb(busIndex, dbVolume);
                GD.Print($"Set {busName} bus volume to {linearVolume} ({dbVolume} dB)");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to set volume for bus {busName}: {ex.Message}");
            }
        }

        public static float GetBusVolume(string busName)
        {
            try
            {
                int busIndex = AudioServer.GetBusIndex(busName);
                if (busIndex == -1)
                {
                    GD.PrintErr($"Bus {busName} not found.");
                    return 0.0f;
                }
                float dbVolume = AudioServer.GetBusVolumeDb(busIndex);
                return Mathf.DbToLinear(dbVolume);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to get volume for bus {busName}: {ex.Message}");
                return 0.0f;
            }
        }

        public static void SetEffectsVolume(float linearVolume)
        {
            try
            {
                SetBusVolume("Planes", linearVolume);
                SetBusVolume("Effects", linearVolume);
                GD.Print($"Set Planes and Effects bus volume to {linearVolume}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to set Planes/Effects volume: {ex.Message}");
            }
        }

        public void ConnectUIButtonAudio()
        {
            if (_isConnectingButtons)
            {
                GD.Print("AudioManager: Skipped ConnectUIButtonAudio due to concurrent call");
                return;
            }
            _isConnectingButtons = true;
            try
            {
                foreach (var node in GetTree().GetNodesInGroup("ui_buttons"))
                {
                    if (node is Button btn && !_connectedButtons.Contains(btn.Name))
                    {
                        btn.Pressed += () => PlaySoundEffect("Keystroke");
                        _connectedButtons.Add(btn.Name);
                        GD.Print($"Connected {btn.Name} to ui_buttons audio");
                    }
                    else if (node is TextureButton texBtn && !_connectedButtons.Contains(texBtn.Name))
                    {
                        texBtn.Pressed += () => PlaySoundEffect("Keystroke");
                        _connectedButtons.Add(texBtn.Name);
                        GD.Print($"Connected {texBtn.Name} to ui_buttons audio");
                    }
                    else
                    {
                        GD.Print($"AudioManager: {node.Name} already connected to ui_buttons audio");
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error connecting ui_buttons audio: {ex.Message}");
            }
            finally
            {
                _isConnectingButtons = false;
                GD.Print("AudioManager: ConnectUIButtonAudio completed");
            }
        }
    }
}