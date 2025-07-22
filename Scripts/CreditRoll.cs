using Godot;
using System;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts
{
    // Handles the scrolling credits display with dynamic backgrounds and audio
    public partial class CreditRoll : Control
    {
        // Adjustable scroll speed (pixels per second)
        [Export]
        public float ScrollSpeed { get; set; } = 38330f / 90f; // Default: full label in 90s

        private RichTextLabel _creditsLabel;
        private AnimationPlayer _animationPlayer;
        private Sprite2D _creditBG0;
        private Sprite2D _creditBG1;
        private Button _backButton;
        private AudioManager _musicManager;

        private const string CreditsTextPath = "res://docs/CreditsText32.txt";
        private const string CreditsThemeUseCase = "Open Credits Animation";
        private const string PauseMenuScene = "res://Scenes/PauseMenu.tscn";

        // Add these fields for programmatically created nodes
        private Theme _creditsTheme;
        private FontFile _buttonFont;
        private static readonly int[] int32Array = [0, 2, 4, 6, 8];
        private static readonly int[] int32Array0 = [1, 3, 5, 7];

        public override void _Ready()
        {
            SetupNodes();
            CreateCreditsAnimation();
            if (_animationPlayer.HasAnimation("Credits_Roll"))
            {
                _animationPlayer.Play("Credits_Roll");
                GD.Print("Credits_Roll animation started.");
            }
            else
            {
                GD.PrintErr("Failed to find Credits_Roll animation.");
            }
            LoadCreditsText();
            _musicManager = GetNodeOrNull<AudioManager>("/root/MusicManager");
            if (_musicManager != null)
            {
                GD.Print("AudioManager found at /root/MusicManager.");
                try
                {
                    _musicManager.PlayMusicByUseCase(CreditsThemeUseCase);
                    GD.Print("Credits music started.");
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error in PlayMusicByUseCase: {e.Message}");
                }
            }
            else
            {
                GD.PrintErr("AudioManager not found at /root/AudioManager. Verify node type and autoload in project.godot.");
            }
            _backButton.Pressed += OnBackButtonPressed;
            _backButton.FocusMode = Control.FocusModeEnum.All;
            _backButton.GrabFocus();
        }

        // Programmatically create and configure all required nodes
        private void SetupNodes()
        {
            _creditsTheme = GD.Load<Theme>("res://Data/Resources/CreditsTheme.tres");
            if (_creditsTheme == null)
                GD.PrintErr("Failed to load CreditsTheme.tres");
            else
                GD.Print("CreditsTheme.tres loaded.");

            _buttonFont = GD.Load<FontFile>("res://Assets/Fonts/L_10646.TTF");
            if (_buttonFont == null)
                GD.PrintErr("Failed to load L_10646.TTF");
            else
                GD.Print("L_10646.TTF font loaded.");

            // Create nodes in order: backgrounds first, then label, then button
            //_creditBG0 = CreateBackgroundSprite("_creditBG0");
            //AddChild(_creditBG0);
            //_creditBG1 = CreateBackgroundSprite("_creditBG1");
            //AddChild(_creditBG1);

            _creditsLabel = new RichTextLabel
            {
                Name = "CreditsLabel",
                CustomMinimumSize = new Vector2(1600, 1600),
                Position = new Vector2(480,-80),
                Theme = _creditsTheme,
                BbcodeEnabled = true,
                FitContent = true,
                ScrollActive = false,
                ScrollFollowing = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                ClipContents = false
            };
           // _creditsLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
            AddChild(_creditsLabel);
            GD.Print("CreditsLabel created and configured.");

            _backButton = new Button
            {
                Name = "Back",
                Text = "Back",
                Size = new Vector2(240, 80),
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            _backButton.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomRight, Control.LayoutPresetMode.KeepSize, (int)20.0f);
            var style = new StyleBoxFlat
            {
                BgColor = new Color("#006060"),
                CornerRadiusTopLeft = 12,
                CornerRadiusTopRight = 12,
                CornerRadiusBottomLeft = 12,
                CornerRadiusBottomRight = 12
            };
            var styleHover = new StyleBoxFlat
            {
                BgColor = new Color("#add8e6"),
                CornerRadiusTopLeft = 12,
                CornerRadiusTopRight = 12,
                CornerRadiusBottomLeft = 12,
                CornerRadiusBottomRight = 12
            };
            var stylePressed = new StyleBoxFlat
            {
                BgColor = new Color("#add8e6"),
                CornerRadiusTopLeft = 12,
                CornerRadiusTopRight = 12,
                CornerRadiusBottomLeft = 12,
                CornerRadiusBottomRight = 12
            };
            _backButton.AddThemeStyleboxOverride("normal", style);
            _backButton.AddThemeStyleboxOverride("hover", styleHover);
            _backButton.AddThemeStyleboxOverride("pressed", stylePressed);
            if (_buttonFont != null)
            {
                _backButton.AddThemeFontOverride("font", _buttonFont);
                _backButton.AddThemeFontSizeOverride("font_size", 60);
                _backButton.AddThemeColorOverride("font_color", new Color("#ffffff"));
            }
            AddChild(_backButton);
            GD.Print("Back button created and styled.");

            _animationPlayer = new AnimationPlayer
            {
                Name = "AnimationPlayer"
            };
            AddChild(_animationPlayer);
            GD.Print("AnimationPlayer created.");
        }

        // Helper to create and configure Sprite2D backgrounds
        //private Sprite2D CreateBackgroundSprite(string name)
        //{
            //var sprite = new Sprite2D
            //{
                //Name = name,
                //Position = Vector2.Zero,
                //Centered = false,
                //ShowBehindParent = true, // Ensure sprite renders behind RichTextLabel
                //Texture = null, // Set by animation
                //Scale = GetViewport().GetVisibleRect().Size / new Vector2(2560, 1440) // Will be scaled by texture size
            //};
           // GD.Print($"{name} created and configured.");
            //return sprite;
        //}

        // Create the Credits_Roll animation with scrolling and crossfades
        private void CreateCreditsAnimation()
        {
            var anim = new Animation
            {
                Length = 90.0f,
                LoopMode = Animation.LoopModeEnum.Linear
            };

            // 1. CreditsLabel vertical scroll (position.y)
            float scrollDistance = 38330f;
            float duration = scrollDistance / ScrollSpeed;
            anim.AddTrack(Animation.TrackType.Value);
            anim.TrackSetPath(0, _creditsLabel.GetPath().ToString() + ":position:y");
            anim.TrackInsertKey(0, 0.0f, 0.0f);
            anim.TrackInsertKey(0, duration, -scrollDistance);


            // Create and add to default library
            var library = new AnimationLibrary();
            library.AddAnimation(new StringName("Credits_Roll"), anim);
            _animationPlayer.AddAnimationLibrary(new StringName(""), library);
            GD.Print("Credits_Roll animation created and added to default library.");
        }

        // Loads the credits text from file and applies theme
        private void LoadCreditsText()
        {
            if (!FileAccess.FileExists(CreditsTextPath))
            {
                GD.PrintErr($"Credits file not found: {CreditsTextPath}");
                _creditsLabel.Text = "[b]Credits file missing![/b]";
                return;
            }

            try
            {
                using var file = FileAccess.Open(CreditsTextPath, FileAccess.ModeFlags.Read);
                var text = file.GetAsText();
                _creditsLabel.BbcodeEnabled = true;
                _creditsLabel.Text = text;
                GD.Print("Credits text loaded successfully.");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading credits text: {e.Message}");
                _creditsLabel.Text = "[b]Error loading credits![/b]";
            }
        }

        // Handles input for Escape/Spacebar to transition to PauseMenu
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.Escape || keyEvent.Keycode == Key.Space)
                {
                    GD.Print("Input received: Transitioning to PauseMenu.");
                    GoToPauseMenu();
                }
            }
        }

        // Handles Back button click
        private void OnBackButtonPressed()
        {
            GD.Print("Back button pressed: Transitioning to PauseMenu.");
            GoToPauseMenu();
        }

        // Transitions to PauseMenu scene
        private void GoToPauseMenu()
        {
            _musicManager.StopMusic();
            GetTree().ChangeSceneToFile(PauseMenuScene);
        }
    }
}