using Godot;
using System;

namespace WarEaglesDigital.Scripts
{
    public partial class Zone : Node
    {
        // Target Unit properties (from WEBasicZones.csv)
        [Export]
        public string TargetName { get; set; } // e.g., "Berlin"
        public string TargetNationality { get; set; } // e.g., "Germany"
        public string TargetDomain { get; set; } // e.g., "Land"
        public int TargetAAStrength { get; set; } // Anti-aircraft dice
        public int TargetDamageCapacity { get; set; } // Hits to destroy
        public int TargetProduction { get; set; } // Production Points
        public int TargetVPValue { get; set; } // Victory Points if undestroyed

        // Base Unit properties (from WEBasicZones.csv)
        public string FacilityName { get; set; } // e.g., "Oberdorf"
        public string FacilityCategory { get; set; } // e.g., "Airstrip", "Airfield", "Airbase"
        public int FacilityAAStrength { get; set; } // Anti-aircraft dice
        public int FacilityDamageCapacity { get; set; } // Hits to destroy
        public string Model { get; set; } // e.g., "Berlin.glb"

        // Constructor
        public Zone() { }

        // Load data from a CSV row
        public void LoadFromCsv(Godot.Collections.Dictionary<string, Variant> csvRow)
        {
            try
            {
                GD.Print("Zone dictionary keys: ", string.Join(", ", csvRow.Keys));
                TargetName = csvRow["Target Name"].AsString();
                TargetNationality = csvRow["Nationality"].AsString();
                TargetDomain = csvRow["Domain"].AsString();
                TargetAAStrength = csvRow["AA Strength"].AsInt32();
                TargetDamageCapacity = csvRow["Damage Capacity"].AsInt32();
                TargetProduction = csvRow["Production"].AsInt32();
                TargetVPValue = csvRow["VP Value"].AsInt32();
                FacilityName = csvRow["Facility"].AsString();
                FacilityCategory = csvRow["Category"].AsString();
                FacilityAAStrength = csvRow["Facility AA"].AsInt32();
                FacilityDamageCapacity = csvRow["Facility Damage Cap"].AsInt32();
                Model = csvRow["Model"].AsString();
                GD.Print($"Loaded Zone: {TargetName}");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading Zone from CSV: {e.Message}");
            }
        }

        // Roll combined AA dice for Target and Base (hits on 6, per Standard Rules)
        public int RollAADice()
        {
            int hits = 0;
            Random rand = new();
            for (int i = 0; i < TargetAAStrength + FacilityAAStrength; i++)
            {
                if (rand.Next(1, 7) == 6) // Per rules, AA hits on 6
                    hits++;
            }
            GD.Print($"Rolling AA for {TargetName}: {hits} hits");
            return hits;
        }
    }
}