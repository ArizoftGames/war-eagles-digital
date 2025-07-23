using Godot;
using System;
using System.Collections.Generic;
using WarEaglesDigital.Scripts;

namespace WarEaglesDigital.Scripts
{
    /// <summary>
    /// The Extras class manages the Extras menu, including models, soundtrack, articles, and export functionality.
    /// </summary>
    public partial class Extras : Control
    {
        // Node references
        private TabContainer _optionsContainer;
        private VBoxContainer _modelsContainer;
        private GridContainer _modelsGrid;
        private Label _modelsLabel;
        private HBoxContainer _soundtrackContainer;
        private ItemList _soundtrackList;
        private RichTextLabel _historyText;
        private VBoxContainer _furtherReading;
        private RichTextLabel _furtherReadingText;
        private RichTextLabel _designersNotesText;
        private Button _backButton;
        private FileDialog _exportDialog;

        // AudioManager singleton
        private AudioManager _musicManager;

        // CSV data
        private Dictionary<string, Dictionary<string, string>> _modelsData = [];
        private Dictionary<string, Dictionary<string, string>> _musicData = [];
        private string _currentExportPath;

        public override void _Ready()
        {
            try
            {
                // Initialize AudioManager and play Open Extras music
                _musicManager = GetNodeOrNull<AudioManager>("/root/MusicManager");
                _musicManager?.PlayMusicByUseCase("Open Extras");

                // Get node references from scene
                _optionsContainer = GetNode<TabContainer>("OptionsContainer");
                _modelsContainer = GetNode<VBoxContainer>("OptionsContainer/Models");
                _modelsGrid = GetNode<GridContainer>("OptionsContainer/Models/ModelsGrid");
                _modelsLabel = GetNode<Label>("OptionsContainer/Models/ModelsLabel");
                _soundtrackContainer = GetNode<HBoxContainer>("OptionsContainer/Soundtrack");
                _soundtrackList = GetNode<ItemList>("OptionsContainer/Soundtrack/SoundtrackList");
                _historyText = GetNode<RichTextLabel>("OptionsContainer/History of Aerial Strategy");
                _furtherReading = GetNode<VBoxContainer>("OptionsContainer/Further Reading");
                _furtherReadingText = GetNode<RichTextLabel>("OptionsContainer/Further Reading/Further ReadingLabel");
                _designersNotesText = GetNode<RichTextLabel>("OptionsContainer/Designer's Notes");
                _backButton = GetNode<Button>("BackButton");

                // Add back button to ui_buttons group
                DisplayMenuPanel.AddButtonToUIGroup(_backButton, "BackButton");

                // Initialize FileDialog
                _exportDialog = new FileDialog
                {
                    FileMode = FileDialog.FileModeEnum.SaveFile
                };
                _exportDialog.FileSelected += OnExportFileSelected;
                _exportDialog.UseNativeDialog = true;
                _exportDialog.SetTitle("Export to System");
                string exportPath = GetExportPath();
                _exportDialog.SetCurrentDir(exportPath);
                AddChild(_exportDialog);

                // Load CSV data
                if (FileAccess.FileExists("res://Docs/ExtrasModels.csv"))
                    _modelsData = LoadCsv("res://Docs/ExtrasModels.csv");
                else
                {
                    GD.PrintErr("ExtrasModels.csv not found");
                    _modelsLabel.Text = "Error: Models data not available";
                }

                if (FileAccess.FileExists("res://Docs/ExtrasMusic.csv"))
                    _musicData = LoadCsv("res://Docs/ExtrasMusic.csv");
                else
                {
                    GD.PrintErr("ExtrasMusic.csv not found");
                    _soundtrackList.AddItem("Error: Music data not available");
                }

                // Populate tabs
                PopulateModelsTab();
                PopulateSoundtrackTab();
                PopulateArticleTabs();

                // Connect signals
                _backButton.Pressed += OnBackButtonPressed;

                // Notify EffectsManager to connect ui_buttons audio
                GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");
            }
            catch (Exception e)
            {
                GD.PrintErr("Initialization error: " + e.Message);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Escape || keyEvent.Keycode == Key.Space)
                    OnBackButtonPressed();
            }
        }

        // Populate Models tab (GridContainer)
        private void PopulateModelsTab()
        {
            try
            {
                List<Button> buttons = new List<Button>();
                _modelsGrid.Set("theme_override_constants/h_separation", 256);
                _modelsGrid.Set("theme_override_constants/v_separation", 5);
                _modelsGrid.Columns = 5;

                foreach (var modelEntry in _modelsData.Values)
                {
                    var container = new VBoxContainer();
                    var thumbnail = new TextureRect
                    {
                        Texture = LoadTexture("res://Extras/Models/" + modelEntry.GetValueOrDefault("Img")),
                        CustomMinimumSize = new Vector2(256, 256),
                        Size = new Vector2(256, 256),
                    };
                    var exportButton = new Button
                    {
                        AnchorRight = .5f,
                        AnchorLeft = -.5f,
                        OffsetLeft = -128,
                        OffsetRight = 128,
                        OffsetTop = -40,
                        OffsetBottom = 0,
                        Alignment = (HorizontalAlignment)1f,
                        Flat = true,
                        Position = new Vector2(0, 200),
                        Text = modelEntry.GetValueOrDefault("Model"),
                        TooltipText = $"Export {modelEntry.GetValueOrDefault("Model")} (.zip, {modelEntry.GetValueOrDefault("Tooltip")})"
                    };
                    var fontColor = new Color(.7f, 0.9f, 0.8f, 1f);
                    var outlineColor = new Color(.1f, 0.1f, 0.1f, 1f);
                    exportButton.AddThemeColorOverride("font_color", fontColor);
                    exportButton.AddThemeConstantOverride("outline_size", 3);
                    exportButton.Pressed += () => OnExportModelPressed(modelEntry.GetValueOrDefault("Zip"));
                    container.AddChild(thumbnail);
                    thumbnail.AddChild(exportButton);
                    _modelsGrid.AddChild(container);
                    buttons.Add(exportButton);
                }

                // Add buttons to ui_buttons group
                foreach (var button in buttons)
                    DisplayMenuPanel.AddButtonToUIGroup(button, button.Name);

                // Notify EffectsManager to connect ui_buttons audio
                GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");
            }
            catch (Exception e)
            {
                GD.PrintErr("Models population error: " + e.Message);
            }
        }

        // Populate Soundtrack tab (ItemList)
        private void PopulateSoundtrackTab()
        {
            try
            {
                List<TextureButton> buttons = new List<TextureButton>();
                _soundtrackList.Clear();
                foreach (Node child in _soundtrackList.GetChildren())
                {
                    child.QueueFree();
                }

                // Restore correct sizing for _soundtrackList
                _soundtrackList.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                _soundtrackList.SizeFlagsStretchRatio = 3.0f;

                // Remove and recreate exportContainer if it exists
                var existingExportContainer = _soundtrackContainer.GetNodeOrNull<VBoxContainer>("ExportContainer");
                existingExportContainer?.QueueFree();

                var exportContainer = new VBoxContainer
                {
                    Name = "ExportContainer",
                    SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd,
                    SizeFlagsStretchRatio = 1.0f,
                    SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                    CustomMinimumSize = new Vector2(50, 0)
                };
                _soundtrackContainer.AddChild(exportContainer);
                exportContainer.Set("theme_override_constants/separation", 13);

                foreach (var trackEntry in _musicData.Values)
                {
                    int idx = _soundtrackList.AddItem(trackEntry.GetValueOrDefault("Button Text"));
                    _soundtrackList.SetItemTooltip(idx, trackEntry.GetValueOrDefault("Tooltip"));

                    var metadata = new Godot.Collections.Dictionary<string, string>
                    {
                        { "Method", trackEntry.GetValueOrDefault("AudioManager Method") },
                        { "Arguments", trackEntry.GetValueOrDefault("Arguments") },
                        { "ExportFilename", trackEntry.GetValueOrDefault("Export Filename") }
                    };
                    _soundtrackList.SetItemMetadata(idx, metadata);

                    var exportButton = new TextureButton
                    {
                        TextureNormal = LoadTexture("res://Assets/Sprites/MusicExport.png"),
                        TooltipText = $"Export {trackEntry.GetValueOrDefault("Button Text")} (.mp3, {trackEntry.GetValueOrDefault("Tooltip")})",
                        CustomMinimumSize = new Vector2(40, 40),
                        Name = "Export_" + trackEntry.GetValueOrDefault("Button Text").Replace(" ", "_")
                    };
                    exportButton.Pressed += () => OnExportMusicPressed(trackEntry.GetValueOrDefault("Export Filename"));
                    exportContainer.AddChild(exportButton);
                    buttons.Add(exportButton);
                }

                // Add buttons to ui_buttons group
                foreach (var button in buttons)
                    DisplayMenuPanel.AddButtonToUIGroup(button, button.Name);

                // Notify EffectsManager to connect ui_buttons audio
                GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");

                _soundtrackList.ItemSelected += OnSoundtrackItemSelected;
                _soundtrackList.Set("theme_override_constants/separation", 10);
            }
            catch (Exception e)
            {
                GD.PrintErr("Soundtrack population error: " + e.Message);
                _soundtrackList.AddItem("Error: Music data unavailable");
            }
        }

        private void PopulateArticleTabs()
        {
            try
            {
                _historyText.Text = LoadFileText("res://Extras/Articles/History Of Air Strategy.txt");
                _designersNotesText.Text = LoadFileText("res://Extras/Articles/Design Notes.txt");

                // --- Further Reading Tab Layout Fix ---
                var furtherReadingParent = _furtherReadingText.GetParent();
                if (furtherReadingParent is VBoxContainer furtherReadingVBox)
                {
                    foreach (var child in furtherReadingVBox.GetChildren())
                        child.QueueFree();

                    var readingText = new RichTextLabel
                    {
                        Name = "FurtherReadingText",
                        BbcodeEnabled = true,
                        Text = LoadFileText("res://Docs/FurtherReading.txt"),
                        SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                        SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                        SizeFlagsStretchRatio = 9.0f,
                    };
                    furtherReadingVBox.AddChild(readingText);

                    var spacer = new Control
                    {
                        SizeFlagsVertical = Control.SizeFlags.Expand
                    };
                    furtherReadingVBox.AddChild(spacer);

                    var furtherExport = new Button
                    {
                        Text = "Export List",
                        Name = "ExportBibliographyButton",
                        SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
                        SizeFlagsVertical = Control.SizeFlags.ShrinkEnd,
                        SizeFlagsStretchRatio = 1.0f,
                    };
                    furtherExport.Pressed += () => OnExportArticlePressed("res://Docs/FurtherReading.pdf");
                    furtherExport.OffsetBottom = 10;
                    furtherReadingVBox.AddChild(furtherExport);
                    DisplayMenuPanel.AddButtonToUIGroup(furtherExport, "ExportBibliographyButton");

                    // Notify EffectsManager to connect ui_buttons audio
                    GetNode("/root/EffectsManager")?.Call("ConnectUIButtonAudio");
                }
                else
                {
                    GD.PrintErr("Further Reading parent is not a VBoxContainer. Button placement may fail.");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("Article population error: " + e.Message);
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
                    GD.PrintErr($"Failed to create War Eagles directory: {ex.Message}");
                }
            }
            return warEaglesPath;
        }

        private void OnExportModelPressed(string zipFile)
        {
            try
            {
                _exportDialog.SetCurrentFile(zipFile);
                _currentExportPath = "res://Extras/Models/" + zipFile;
                if (FileAccess.FileExists(_currentExportPath))
                    _exportDialog.PopupCentered();
                else
                {
                    GD.PrintErr("Model file not found: " + _currentExportPath);
                    _modelsLabel.Text = $"Error: Export failed for {zipFile}";
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("Model export error: " + e.Message);
            }
        }

        private void OnExportMusicPressed(string filename)
        {
            try
            {
                var toExport = filename;
                toExport = toExport.Replace("res://Extras/Music/", "");
                GD.Print($"Exporting music: {toExport}");
                _exportDialog.SetCurrentFile(toExport);
                _currentExportPath = filename;
                if (FileAccess.FileExists(_currentExportPath))
                {
                    _exportDialog.PopupCentered();
                }
                else
                {
                    GD.PrintErr($"Music file not found: {_currentExportPath}");
                    _soundtrackList.AddChild(new Label { Text = $"Error: Export failed for {_currentExportPath}" });
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("Music export error: " + e.Message);
            }
        }

        private void OnExportArticlePressed(string filePath)
        {
            try
            {
                _exportDialog.SetCurrentFile("res://Docs/FurtherReading.pdf");
                _currentExportPath = filePath;
                if (FileAccess.FileExists(_currentExportPath))
                    _exportDialog.PopupCentered();
                else
                {
                    GD.PrintErr("Article file not found: " + _currentExportPath);
                    _modelsLabel.Text = "Error: Export failed for article";
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("Article export error: " + e.Message);
            }
        }

        private void OnExportFileSelected(string savePath)
        {
            try
            {
                using var src = FileAccess.Open(_currentExportPath, FileAccess.ModeFlags.Read);
                using var dst = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
                if (src == null || dst == null)
                    throw new Exception("File open failed for export.");

                var buffer = src.GetBuffer((long)src.GetLength());
                dst.StoreBuffer(buffer);
                GD.Print("File exported to: " + savePath);
            }
            catch (Exception e)
            {
                GD.PrintErr("File export error: " + e.Message);
                _modelsLabel.Text = "Error: Export failed";
            }
        }

        private void OnBackButtonPressed()
        {
            try
            {
                _musicManager?.StopMusic();
                GetTree().ChangeSceneToFile("res://Scenes/PauseMenu.tscn");
            }
            catch (Exception e)
            {
                GD.PrintErr("Navigation error: " + e.Message);
            }
        }

        private void OnSoundtrackItemSelected(long index)
        {
            try
            {
                var metadataVariant = _soundtrackList.GetItemMetadata((int)index);
                if (metadataVariant.Obj is Godot.Collections.Dictionary metadata)
                {
                    string method = metadata.ContainsKey("Method") ? metadata["Method"].AsString() : null;
                    string arguments = metadata.ContainsKey("Arguments") ? metadata["Arguments"].AsString() : null;

                    if (string.IsNullOrEmpty(method) || string.IsNullOrEmpty(arguments))
                    {
                        GD.PrintErr($"Invalid metadata for track: {_soundtrackList.GetItemText((int)index)}");
                        return;
                    }

                    GD.Print($"Selected track: {_soundtrackList.GetItemText((int)index)}, Method: {method}, Arguments: {arguments}");

                    string[] args = arguments.Split(',');
                    for (int i = 0; i < args.Length; i++)
                        args[i] = args[i].Trim();

                    if (method == "PlayMusicByUseCase")
                    {
                        if (args.Length == 1)
                            _musicManager?.Call("PlayMusicByUseCase", args[0]);
                        else
                            GD.PrintErr($"Invalid argument count for PlayMusicByUseCase: {args.Length}");
                    }
                    else if (method == "PlayMusicByNationMood")
                    {
                        if (args.Length == 2)
                            _musicManager?.Call("PlayMusicByNationMood", args[0], args[1]);
                        else
                            GD.PrintErr($"Invalid argument count for PlayMusicByNationMood: {args.Length}");
                    }
                    else
                    {
                        GD.PrintErr($"Unknown AudioManager method: {method}");
                    }
                }
                else
                {
                    GD.PrintErr($"Invalid metadata type for track: {_soundtrackList.GetItemText((int)index)}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("Item selection error: " + e.Message);
            }
        }

        private static Dictionary<string, Dictionary<string, string>> LoadCsv(string filePath)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                var headers = file.GetCsvLine();
                while (!file.EofReached())
                {
                    var line = file.GetCsvLine();
                    if (line.Length == headers.Length)
                    {
                        var entry = new Dictionary<string, string>();
                        for (int i = 0; i < headers.Length; i++)
                            entry[headers[i]] = line[i];
                        result[line[0]] = entry;
                    }
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("CSV load error: " + e.Message);
            }
            return result;
        }

        private static Texture2D LoadTexture(string path)
        {
            try
            {
                if (ResourceLoader.Exists(path))
                    return ResourceLoader.Load<Texture2D>(path);
                else
                {
                    GD.PrintErr("Texture not found: " + path);
                    return null;
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("Texture load error: " + e.Message);
                return null;
            }
        }

        private static string LoadFileText(string path)
        {
            try
            {
                if (FileAccess.FileExists(path))
                {
                    using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                    return file.GetAsText();
                }
                else
                {
                    GD.PrintErr("Text file not found: " + path);
                    return "Error: Content unavailable";
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("Text file load error: " + e.Message);
                return "Error: Content unavailable";
            }
        }
    }
}