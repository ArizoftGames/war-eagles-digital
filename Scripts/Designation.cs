using Godot;
using System;
using System.Collections.Generic;

namespace WarEaglesDigital.Scripts
{
    public partial class Designation : Node
    {
        // Properties matching Designations.csv columns
        [Export]
        public string SquadronName { get; set; } // e.g., "I/JG 2", "27th FS"
        public string Nationality { get; set; } // e.g., "Germany", "USA"
        public string Type { get; set; } // e.g., "Fighter", "Bomber"
        public string Special { get; set; } // e.g., "Dive Bomber" or empty

        // List to store loaded designations
        private static readonly List<Designation> designations = [];
        private static readonly List<string> assignedDesignations = []; // Track assigned designations

        // Constructor
        public Designation() { }

        // Load data from a CSV row
        public void LoadFromCsv(Godot.Collections.Dictionary<string, Variant> csvRow)
        {
            try
            {
                SquadronName = csvRow["SquadronName"].AsString().Trim();
                Nationality = csvRow["Nationality"].AsString().Trim();
                Type = csvRow["Type"].AsString().Trim();
                Special = csvRow["Special"].AsString().Trim();
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading Designation from CSV: {e.Message}");
            }
        }

        // Load all designations from CSV
        public static List<Designation> LoadDesignations(string path)
        {
            designations.Clear();
            assignedDesignations.Clear();
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Failed to open {path}");
                return designations;
            }

            // Skip header
            file.GetLine();

            while (!file.EofReached())
            {
                string line = file.GetLine();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    var values = line.Split(",");
                    if (values.Length < 4) // Expect 4 columns
                    {
                        GD.PrintErr($"Invalid designation row: {line}");
                        continue;
                    }

                    var dict = new Godot.Collections.Dictionary<string, Variant>
                    {
                        { "SquadronName", values[0].Trim() },
                        { "Nationality", values[1].Trim() },
                        { "Type", values[2].Trim() },
                        { "Special", values[3].Trim() }
                    };

                    var designation = new Designation();
                    designation.LoadFromCsv(dict);
                    designations.Add(designation);
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error parsing designation row: {line}. Error: {e.Message}");
                }
            }

            GD.Print($"Loaded {designations.Count} designations.");
            return designations;
        }

        // Reset assigned designations (e.g., for new game)
        public static void ResetAssignedDesignations()
        {
            assignedDesignations.Clear();
            GD.Print("Assigned designations reset.");
        }

        // Get a random designation by nationality and type
        public static Designation GetRandomDesignation(string nationality, string type)
        {
            try
            {
                // Map roles to designation types
                string designationType = type switch
                {
                    "Fighter/Bomber" => "Fighter",
                    "Fighter" => "Fighter",
                    "Bomber" => "Bomber",
                    _ => null
                };

                if (designationType == null)
                {
                    GD.PrintErr($"Unsupported role: {type} for Nationality: {nationality}");
                    return null;
                }

                var matchingDesignations = designations.FindAll(d =>
                    d.Nationality.Equals(nationality, StringComparison.OrdinalIgnoreCase) &&
                    d.Type.Equals(designationType, StringComparison.OrdinalIgnoreCase) &&
                    !assignedDesignations.Contains(d.SquadronName));

                if (matchingDesignations.Count == 0)
                {
                    GD.PrintErr($"No available designations found for Nationality: {nationality}, Type: {designationType} (Original: {type})");
                    return null;
                }

                Random rand = new();
                var selected = matchingDesignations[rand.Next(matchingDesignations.Count)];
                assignedDesignations.Add(selected.SquadronName);
                GD.Print($"Assigned designation: {selected.SquadronName} ({nationality}, {type})");
                return selected;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error getting random designation: {e.Message}");
                return null;
            }
        }
    }
}