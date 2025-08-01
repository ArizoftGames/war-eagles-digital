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

            // Connect to InputManager.ControllerStateChanged
            /*try
            {
                var inputManager = GetNodeOrNull<Node>("/root/InputManager");
                if (inputManager != null)
                {
                    inputManager.Connect("ControllerStateChanged", new Callable(this, nameof(OnControllerStateChanged)));
                    GD.Print("Connected to InputManager.ControllerStateChanged");
                }
                else
                {
                    GD.PrintErr("InputManager not found at /root/InputManager for signal connection");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception connecting to InputManager.ControllerStateChanged: {ex.Message}");
            }*/

            // Log viewport and panel info for debugging
            var viewportSize = GetViewport().GetVisibleRect().Size;
            /* GD.Print($"Viewport size: {viewportSize}");
             GD.Print($"Notification Panel position: {_notificationPanel.GlobalPosition}");
             GD.Print($"Notification Panel size: {_notificationPanel.Size}");
             GD.Print($"Notification Label size: {_notificationLabel.Size}"); */
        }

        private void OnControllerStateChanged(bool active)
        {
            try
            {
                string message = active ? "Controller enabled" : "Controller disabled";
                ShowNotification(message);
                GD.Print($"Notification: {message}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in OnControllerStateChanged: {ex.Message}");
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                try
                {
                    if (keyEvent.Keycode == Key.Escape || keyEvent.Keycode == Key.Space)
                    {
                        GD.Print("Input: PauseGame triggered");
                        PauseGame();
                    }
                    else if (keyEvent.Keycode == Key.Asciitilde)
                    {
                        GD.Print("Input: AccessConsole triggered");
                        AccessConsole();
                    }
                    else if (keyEvent.Keycode == Key.Delete)
                    {
                        GD.Print("Input: ScreenShot triggered");
                        ScreenShot();
                    }
                    else if (keyEvent.Keycode == Key.X)
                    {
                        GD.Print("Input: Quit game triggered");
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
                            GD.PrintErr($"Exception in ReleaseResources: {ex.Message}");
                        }

                        GetTree().Quit();
                    }
                    else if (keyEvent.Keycode == Key.H)
                    {
                        GD.Print("Input: OpenHelpMenu triggered");
                        OpenHelpMenu();
                    }
                    else if (keyEvent.Keycode == Key.Z)
                    {
                        GD.Print("Input: EndPhase triggered");
                        EndPhase();
                    }
                    else if (keyEvent.Keycode == Key.Enter)
                    {
                        GD.Print("Input: EndTurn triggered");
                        EndTurn();
                    }
                    else if (keyEvent.Keycode == Key.Backspace)
                    {
                        GD.Print("Input: SkipVoiceover triggered");
                        SkipVoiceover();
                    }
                    else if (keyEvent.Keycode == Key.L)
                    {
                        GD.Print("Input: ViewPlayerLossesPool triggered");
                        ViewPlayerLossesPool();
                    }
                    else if (keyEvent.Keycode == Key.K)
                    {
                        GD.Print("Input: ViewEnemyLossesPool triggered");
                        ViewEnemyLossesPool();
                    }
                    else if (keyEvent.Keycode == Key.G)
                    {
                        GD.Print("Input: GameHistory triggered");
                        GameHistory();
                    }
                    else if (keyEvent.Keycode == Key.Q)
                    {
                        GD.Print("Input: SelectUnit triggered");
                        SelectUnit();
                    }
                    else if (keyEvent.Keycode == Key.Tab)
                    {
                        GD.Print("Input: ScrollFocus triggered");
                        ScrollFocus();
                    }
                    else if (keyEvent.Keycode == Key.Backslash)
                    {
                        GD.Print("Input: CameraLevel triggered");
                        CameraLevel();
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Exception in _Input: {ex.Message}");
                }
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
                     GD.Print($"Notification Label size: {_notificationLabel.Size}"); */
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

       /* public void SetGameSettings(Godot.Collections.Dictionary<string, Variant> settings)
        {
            GD.Print("SetGameSettings called.");
            // NYI
        }

        public void GetGameSettings()
        {
            // NYI
        }*/

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
                // transitionScene.QueueFree();
                GD.Print($"Transition to {NextScenePath} completed successfully.");
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in TransitionToIntroAnim: {ex.Message}");
            }
        }

        // NYI method stubs for KeyBindings.csv
        public void OpenHelpMenu()
        {
            GD.Print("OpenHelpMenu NYI");
        }

        public void EndPhase()
        {
            GD.Print("EndPhase NYI");
        }

        public void EndTurn()
        {
            GD.Print("EndTurn NYI");
        }

        public void SkipVoiceover()
        {
            GD.Print("SkipVoiceover NYI");
        }

        public void ViewPlayerLossesPool()
        {
            GD.Print("ViewPlayerLossesPool NYI");
        }

        public void ViewEnemyLossesPool()
        {
            GD.Print("ViewEnemyLossesPool NYI");
        }

        public void GameHistory()
        {
            GD.Print("GameHistory NYI");
        }

        public void SelectUnit()
        {
            GD.Print("SelectUnit NYI");
        }

        public void ScrollFocus()
        {
            GD.Print("ScrollFocus NYI");
        }

        public void CameraLevel()
        {
            GD.Print("CameraLevel NYI");
        }
    }
}