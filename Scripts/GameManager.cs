using Godot;
using System;

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

            // Log viewport and panel info for debugging
            var viewportSize = GetViewport().GetVisibleRect().Size;
            GD.Print($"Viewport size: {viewportSize}");
            GD.Print($"Notification Panel position: {_notificationPanel.GlobalPosition}");
            GD.Print($"Notification Panel size: {_notificationPanel.Size}");
            GD.Print($"Notification Label size: {_notificationLabel.Size}");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Escape || keyEvent.Keycode == Key.Space)
                {
                    PauseGame();
                }
                else if (keyEvent.Keycode == Key.Asciitilde)
                {
                    AccessConsole();
                }
                else if (keyEvent.Keycode == Key.Delete)
                {
                    ScreenShot();
                }
                else if (keyEvent.Keycode == Key.X)
                {
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

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string path = $"res://Screenshots/screenshot_{timestamp}.png";
                Error result = screenshot.SavePng(path);
                if (result != Error.Ok)
                {
                    GD.PrintErr($"Failed to save screenshot to {path}: Error code {result}");
                    ShowNotification($"Failed to save screenshot: Error {result}");
                }
                else
                {
                    GD.Print($"Screenshot saved to {path}");
                    ShowNotification($"Screenshot saved: {path}");
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
                    GD.Print($"Notification Label position: {_notificationLabel.GlobalPosition}");
                    GD.Print($"Notification Panel position: {_notificationPanel.GlobalPosition}");
                    GD.Print($"Notification Label size: {_notificationLabel.Size}");
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
    }
}