using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WarEaglesDigital.Scripts;
using Godot.Collections;

namespace WarEaglesDigital.Scripts
{
    public partial class GameManager : Node
    {
        private Label _notificationLabel;
        private Timer _notificationTimer;
        private Panel _notificationPanel;
        private System.Collections.Generic.Dictionary<Key, string> _keyBindings = [];

        public override void _Ready()
        {
            SetProcessInput(true);

            // Initialize notification UI
            var canvasLayer = new CanvasLayer();
            AddChild(canvasLayer);

            // Create a Panel for background
            _notificationPanel = new Panel
            {
                Visible = false,
                // Set custom anchors and offsets for bottom-right with 20px margin (for 2560x1440 viewport)
                AnchorLeft = 1.0f,
                AnchorTop = 1.0f,
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                OffsetRight = -20, // 20px from right edge
                OffsetBottom = -20, // 20px from bottom edge
                OffsetLeft = -620, // Panel width (600) + 20px margin
                OffsetTop = -220, // Panel height (200) + 20px margin
                CustomMinimumSize = new Vector2(600, 200)
            };
            canvasLayer.AddChild(_notificationPanel);

            _notificationLabel = new Label
            {
                Text = "",
                Visible = false,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                ClipText = false,
                HorizontalAlignment = HorizontalAlignment.Left, // Align text left
                                                                // Set Label to fill panel with padding
                AnchorLeft = 0.0f,
                AnchorTop = 0.0f,
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                OffsetLeft = 10, // 10px padding
                OffsetRight = -10,
                OffsetTop = 10,
                OffsetBottom = -10
            };
            _notificationLabel.AddThemeFontSizeOverride("font_size", 36);
            try
            {
                var font = ResourceLoader.Load<FontFile>("res://Assets/Fonts/BAHNSCHRIFT.TTF");
                if (font != null)
                {
                    _notificationLabel.AddThemeFontOverride("font", font);
                }
                else
                {
                    GD.PrintErr("Failed to load font at res://Assets/Fonts/BAHNSCHRIFT.TTF");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in loading font: {ex.Message}");
            }
            _notificationLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
            _notificationPanel.AddChild(_notificationLabel);

            // Set Panel background
            var styleBox = new StyleBoxFlat
            {
                BgColor = new Color(0, 0, 0, 0.7f)
            };
            styleBox.SetCornerRadiusAll(10);
            styleBox.BorderColor = new Color(1, 1, 1, 1f);
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthRight = 2;
            _notificationPanel.AddThemeStyleboxOverride("panel", styleBox);

            _notificationTimer = new Timer
            {
                OneShot = true,
                WaitTime = 3.0f
            };
            _notificationTimer.Timeout += () =>
            {
                _notificationLabel.Visible = false;
                _notificationPanel.Visible = false;
            };
            canvasLayer.AddChild(_notificationTimer);

            // Log viewport and panel info for debugging
            var viewportSize = GetViewport().GetVisibleRect().Size;
            /* GD.Print($"Viewport size: {viewportSize}");
             GD.Print($"Notification Panel position: {_notificationPanel.GlobalPosition}");
             GD.Print($"Notification Panel size: {_notificationPanel.Size}");
             GD.Print($"Notification Label size: {_notificationLabel.Size}");*/

            try
            {
                // Load KeyBindings.csv
                if (!FileAccess.FileExists("res://Docs/KeyBindings.csv")) { GD.PrintErr("CSV missing"); return; }
                using var file = FileAccess.Open("res://Docs/KeyBindings.csv", FileAccess.ModeFlags.Read);
                file.GetCsvLine(); // Skip header
                while (!file.EofReached())
                {
                    var line = file.GetCsvLine();
                    if (line.Length < 2) continue;
                    string command = line[0]; // e.g., "Pause Game and Open Menu"
                    string kb = line[1]; // e.g., "Esc or Spacebar"
                                         // Parse kb to Key enums (split 'or', map "Esc" to Key.Escape, etc.)
                    foreach (string keyStr in kb.Split(" or "))
                    {
                        Key keyEnum = keyStr.Trim() switch
                        {
                            "Esc" => Key.Escape,
                            "Spacebar" => Key.Space,
                            "X" => Key.X,
                            "~" => Key.Asciitilde,
                            "H" => Key.H,
                            "Delete" => Key.Delete,
                            "Z" => Key.Z,
                            "Enter" => Key.Enter,
                            "Backspace" => Key.Backspace,
                            "L" => Key.L,
                            "K" => Key.K,
                            "G" => Key.G,
                            "Q" => Key.Q,
                            "E" => Key.E,
                            "Tab" => Key.Tab,
                            "+" => Key.Plus,
                            "-" => Key.Minus,
                            "Up Arrow" => Key.Up,
                            "Down Arrow" => Key.Down,
                            "Left Arrow" => Key.Left,
                            "Rt Arrow" => Key.Right,
                            "W" => Key.W,
                            "S" => Key.S,
                            "A" => Key.A,
                            "D" => Key.D,
                            "\\" => Key.Backslash,
                            _ => Key.None
                        };
                        if (keyEnum != Key.None) _keyBindings[keyEnum] = command;
                    }
                }
            }
            catch (Exception ex) { GD.PrintErr($"CSV load error: {ex}"); }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                try
                {
                    if (_keyBindings.TryGetValue(keyEvent.Keycode, out string command))
                    {
                        switch (command)
                        {
                            case "Pause Game and Open Menu": PauseGame(); break;
                            case "Quit Game no save [NYI]":
                                try
                                {
                                    // Godot's Array does not have ForEach, use a regular foreach loop
                                    foreach (var node in GetTree().GetNodesInGroup("glb_models"))
                                        (node as Node)?.QueueFree();
                                    foreach (var node in GetTree().GetNodesInGroup("audio_players"))
                                    {
                                        node.Call("stop");
                                        node.Set("stream", (Godot.Resource)null); // Correct way to clear the stream in Godot 4.x C#
                                        (node as Node)?.QueueFree();
                                    }
                                    foreach (var node in GetTree().GetNodesInGroup("terrains"))
                                        (node as Node)?.QueueFree();
                                    GD.Print($"Memory after free: {OS.GetStaticMemoryUsage() / 1024 / 1024} MB");
                                }
                                catch (Exception ex)
                                {
                                    GD.PrintErr($"Exception in ReleaseResources(): {ex.Message}");
                                }
                                GetTree().Quit();
                                break;
                            case "Open Console [NYI]": AccessConsole(); break;
                            case "Open Help Menu [NYI]": GD.Print("NYI: Help Menu"); break;
                            case "Screenshot": ScreenShot(); break;
                            case "End Phase [NYI]": GD.Print("NYI: End Phase"); break;
                            case "End Turn [NYI]": GD.Print("NYI: End Turn"); break;
                            case "Skip Voiceover": GD.Print("NYI: Skip Voiceover"); break;
                            case "View Player Losses Pool [NYI]": GD.Print("NYI: Player Losses"); break;
                            case "View Enemy Losses Pool [NYI]": GD.Print("NYI: Enemy Losses"); break;
                            case "Game History [NYI]": GD.Print("NYI: Game History"); break;
                            // Ignore mouse/controller-specific (handled in InputManager)
                            default: break;
                        }
                    }
                }
                catch (Exception ex) { GD.PrintErr($"Input error: {ex}"); }
            }
        }

        public void PauseGame()
        {
            try
            {
                GetTree().Paused = true;
                Error result = GetTree().ChangeSceneToFile("res://Scenes/PauseMenu.tscn");
                if (result != Error.Ok)
                {
                    GD.PrintErr($"Failed to load PauseMenu.tscn: Error code {result}");
                    ShowNotification($"Failed to load Pause Menu: Error {result}");
                }
                else
                {
                    GD.Print("Successfully loaded PauseMenu.tscn");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in PauseGame: {ex.Message}");
                ShowNotification($"Pause Error: {ex.Message}");
            }
        }

        private string GetExportPath()
        {
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string warEaglesPath = System.IO.Path.Combine(documentsPath, "War Eagles");
            if (!System.IO.Directory.Exists(warEaglesPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(warEaglesPath);
                    GD.Print($"Created War Eagles directory at: {warEaglesPath}");
                }
                catch (Exception ex)
                {
                    GD.PushError($"Failed to create War Eagles directory: {ex.Message}");
                }
            }
            return warEaglesPath;
        }

        public void ScreenShot()
        {
            try
            {
                Image screenshot = GetViewport().GetTexture().GetImage();
                if (screenshot == null)
                {
                    GD.PrintErr("Failed to capture screenshot: Viewport texture is null");
                    ShowNotification("Failed to capture screenshot");
                    return;
                }

                //string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string exportPath = GetExportPath();
                string fileName = System.IO.Path.Combine(exportPath, $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                Error result = screenshot.SavePng(fileName);
                if (result != Error.Ok)
                {
                    GD.PrintErr($"Failed to save screenshot to {fileName}: Error code {result}");
                    ShowNotification($"Failed to save screenshot: Error {result}");
                }
                else
                {
                    GD.Print($"Screenshot saved to {fileName}");
                    ShowNotification($"Screenshot saved: {fileName}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in ScreenShot: {ex.Message}");
                ShowNotification($"Screenshot Error: {ex.Message}");
            }
        }

        public void AccessConsole()
        {
            try
            {
                GD.Print("AccessConsole called: Runtime console not yet implemented");
                ShowNotification("Console not yet implemented");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in AccessConsole: {ex.Message}");
                ShowNotification($"Console Error: {ex.Message}");
            }
        }

        private void ShowNotification(string message)
        {
            try
            {
                if (_notificationLabel != null && _notificationTimer != null && _notificationPanel != null)
                {
                    _notificationLabel.Text = message;
                    _notificationLabel.Visible = true;
                    _notificationPanel.Visible = true;
                    _notificationTimer.Start();
                   /* GD.Print($"Notification Label position: {_notificationLabel.GlobalPosition}");
                    GD.Print($"Notification Panel position: {_notificationPanel.GlobalPosition}");
                    GD.Print($"Notification Label size: {_notificationLabel.Size}");*/
                }
                else
                {
                    GD.PrintErr("Notification UI not initialized");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in ShowNotification: {ex.Message}");
            }
        }

        public void SetGameSettings(Godot.Collections.Dictionary<string, Variant> settings)
        {
            GD.Print("SetGameSettings called.");
            //NYI
            
           
        }

        public void GetGameSettings()
        {
            //NYI

        }

        public async void TransitionTo(string NextScenePath)
        {
            try
            {
                GD.Print($"Transitioning to scene: {NextScenePath}");
                          
                var transitionScene = ResourceLoader.Load<PackedScene>("res://Scenes/Transition.tscn").Instantiate();
                AddChild(transitionScene);
                GD.Print("Transition scene instantiated successfully.");
                var animPlayer = transitionScene.GetNode<AnimationPlayer>("AnimationPlayer");
                animPlayer.Play("FadeIn");
                await ToSignal(animPlayer, "animation_finished");
                GD.Print("FadeIn animation finished, changing scene...");
                GetTree().ChangeSceneToFile(NextScenePath);
                GD.Print($"Scene changed to {NextScenePath}, playing FadeOut animation.");
                animPlayer.Play("FadeOut");
                await ToSignal(animPlayer, "animation_finished");
                GD.Print("FadeOut animation finished, removing transition scene.");
                //transitionScene.QueueFree();
                GD.Print($"Transition to {NextScenePath} completed successfully.");
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in TransitionToIntroAnim: {ex.Message}");
            }
        }
    }
}