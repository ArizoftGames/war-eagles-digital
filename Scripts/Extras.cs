using Godot;
using System;
using System.Collections.Generic;
using WarEaglesDigital.Scripts;

public partial class Extras : Control
{
    // Node references
    private TabContainer _optionsContainer;
    private GridContainer _modelsGrid;
    private Label _modelsLabel;
    private ItemList _soundtrackList;
    private RichTextLabel _historyText;
    private RichTextLabel _furtherReadingText;
    private RichTextLabel _designersNotesText;
    private Button _backButton;
    private FileDialog _exportDialog;

    // AudioManager singleton
    private AudioManager _audioManager;

    // CSV data
    private Dictionary<string, Dictionary<string, string>> _modelsData = new();
    private Dictionary<string, Dictionary<string, string>> _musicData = new();
    private string _currentExportPath;

    public override void _Ready()
    {
        try
        {
            // Initialize AudioManager and play Open Extras music
            _audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
            _audioManager?.PlayMusicByUseCase("Open Extras");

            // Get node references from scene
            _optionsContainer = GetNode<TabContainer>("OptionsContainer");
            _modelsGrid = GetNode<GridContainer>("OptionsContainer/Models");
            _modelsLabel = GetNode<Label>("OptionsContainer/Models/ModelsLabel");
            _soundtrackList = GetNode<ItemList>("OptionsContainer/Soundtrack");
            _historyText = GetNode<RichTextLabel>("OptionsContainer/History of Aerial Strategy");
            _furtherReadingText = GetNode<RichTextLabel>("OptionsContainer/Further Reading");
            _designersNotesText = GetNode<RichTextLabel>("OptionsContainer/Designer's Notes");
            _backButton = GetNode<Button>("BackButton");

            // Initialize FileDialog
            _exportDialog = new FileDialog
            {
                FileMode = FileDialog.FileModeEnum.SaveFile
            };
            _exportDialog.AddFilter("*.zip;*.mp3;*.txt", "Supported Files");
            _exportDialog.FileSelected += OnExportFileSelected;
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
            _modelsGrid.Set("theme_override_constants/h_separation", 20);
            _modelsGrid.Set("theme_override_constants/v_separation", 20);
            _modelsGrid.Columns = 4;

            foreach (var modelEntry in _modelsData.Values)
            {
                var container = new CenterContainer();
                var thumbnail = new TextureRect
                {
                    Texture = LoadTexture("res://Extras/Models/" + modelEntry.GetValueOrDefault("Img")),
                    CustomMinimumSize = new Vector2(256, 256)
                };
                var exportButton = new Button
                {
                    Text = modelEntry.GetValueOrDefault("Model"),
                    TooltipText = $"Export {modelEntry.GetValueOrDefault("Model")} (.zip, {modelEntry.GetValueOrDefault("Tooltip")})"
                };
                exportButton.Pressed += () => OnExportModelPressed(modelEntry.GetValueOrDefault("Zip"));
                container.AddChild(thumbnail);
                container.AddChild(exportButton);
                _modelsGrid.AddChild(container);
            }
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
            // Clear existing items and children in SoundtrackList
            _soundtrackList.Clear();
            foreach (Node child in _soundtrackList.GetChildren())
            {
                child.QueueFree();
            }

            // Create HBoxContainer to layout ItemList and Export buttons
            var layoutContainer = new HBoxContainer
            {
                Name = "LayoutContainer",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            _soundtrackList.AddChild(layoutContainer);

            // Re-parent ItemList to layoutContainer and adjust size
            _soundtrackList.Reparent(layoutContainer);
            _soundtrackList.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _soundtrackList.SizeFlagsStretchRatio = 3.0f; // ItemList takes 75% width

            // Create VBoxContainer for Export buttons
            var exportContainer = new VBoxContainer
            {
                Name = "ExportContainer",
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(50, 0)
            };
            layoutContainer.AddChild(exportContainer);
            exportContainer.Set("theme_override_constants/separation", 10);

            // Populate ItemList with tracks
            foreach (var trackEntry in _musicData.Values)
            {
                int idx = _soundtrackList.AddItem(trackEntry.GetValueOrDefault("Button Text"));
                _soundtrackList.SetItemTooltip(idx, trackEntry.GetValueOrDefault("Tooltip"));
                var metadata = new Godot.Collections.Dictionary<string, string>
                {
                    { "MethodCall", trackEntry.GetValueOrDefault("AudioManager Method Call") },
                    { "ExportFilename", trackEntry.GetValueOrDefault("Export Filename") },
                    { "Tooltip", trackEntry.GetValueOrDefault("Tooltip") }
                };
                _soundtrackList.SetItemMetadata(idx, metadata);

                // Create Export button for this track
                var exportButton = new TextureButton
                {
                    TextureNormal = LoadTexture("res://Assets/Sprites/MusicExport.png"),
                    TooltipText = $"Export {trackEntry.GetValueOrDefault("Button Text")} (.mp3, {trackEntry.GetValueOrDefault("Tooltip")})",
                    CustomMinimumSize = new Vector2(40, 40),
                    Name = "Export_" + trackEntry.GetValueOrDefault("Button Text").Replace(" ", "_")
                };
                exportButton.Pressed += () => OnExportMusicPressed(trackEntry.GetValueOrDefault("Export Filename"));
                exportContainer.AddChild(exportButton);
            }

            // Connect ItemList selection signal
            _soundtrackList.ItemSelected += OnSoundtrackItemSelected;

            // Adjust ItemList row height to match buttons
            _soundtrackList.Set("theme_override_constants/separation", 10);
        }
        catch (Exception e)
        {
            GD.PrintErr("Soundtrack population error: " + e.Message);
            _soundtrackList.AddChild(new Label { Text = "Error: Music data unavailable" });
        }
    }

    // Populate article tabs
    private void PopulateArticleTabs()
    {
        try
        {
            _historyText.Text = LoadFileText("res://Extras/Articles/History Of Air Strategy.txt");
            _furtherReadingText.Text = LoadFileText("res://Docs/FurtherReading.txt");
            _designersNotesText.Text = LoadFileText("res://Extras/Articles/Design Notes.txt");

            // Export buttons for articles
            var historyExport = new Button { Text = "Export History" };
            historyExport.Pressed += () => OnExportArticlePressed("res://Extras/Articles/History Of Air Strategy.txt");
            _historyText.AddChild(historyExport);

            var furtherExport = new Button { Text = "Export Bibliography" };
            furtherExport.Pressed += () => OnExportArticlePressed("res://Docs/FurtherReading.txt");
            _furtherReadingText.AddChild(furtherExport);

            var notesExport = new Button { Text = "Export Notes" };
            notesExport.Pressed += () => OnExportArticlePressed("res://Extras/Articles/Design Notes.txt");
            _designersNotesText.AddChild(notesExport);
        }
        catch (Exception e)
        {
            GD.PrintErr("Article population error: " + e.Message);
        }
    }

    // Export model handler
    private void OnExportModelPressed(string zipFile)
    {
        try
        {
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

    private void OnSoundtrackItemSelected(long index)
    {
        try
        {
            var metadataVariant = _soundtrackList.GetItemMetadata((int)index);
            if (metadataVariant.Obj is Godot.Collections.Dictionary metadata)
            {
                string methodCall = metadata.ContainsKey("MethodCall") ? metadata["MethodCall"].AsString() : null;
                if (!string.IsNullOrEmpty(methodCall))
                {
                    _audioManager?.Call(methodCall);
                }
                else
                {
                    GD.PrintErr($"No AudioManager method call for track: {_soundtrackList.GetItemText((int)index)}");
                }
            }
            else
            {
                GD.PrintErr("No metadata for selected track or invalid metadata type");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr("Item selection error: " + e.Message);
        }
    }

    // Export music handler
    private void OnExportMusicPressed(string filename)
    {
        try
        {
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

    // Export article handler
    private void OnExportArticlePressed(string filePath)
    {
        try
        {
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

    // FileDialog save handler
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

    // Back button handler
    private void OnBackButtonPressed()
    {
        try
        {
            _audioManager?.StopMusic();
            GetTree().ChangeSceneToFile("res://Scenes/PauseMenu.tscn");
        }
        catch (Exception e)
        {
            GD.PrintErr("Navigation error: " + e.Message);
        }
    }

    // Helper: Load CSV
    private Dictionary<string, Dictionary<string, string>> LoadCsv(string filePath)
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

    // Helper: Load texture
    private Texture2D LoadTexture(string path)
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

    // Helper: Load file text
    private string LoadFileText(string path)
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
