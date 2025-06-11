using Godot;
using System;

namespace WarEaglesDigital.Scripts
{

    public partial class AntiAircraftUnit : Node
    {
        // Properties matching WEBasicAntiAircraftUnits.csv columns
        public string Unit { get; set; } // e.g., "Type 96 25mm"
        public string Nationality { get; set; } // e.g., "Japan"
        public int Cost { get; set; } // Production Points cost
        public int AntiAircraftStrength { get; set; } // Dice for attacking aircraft
        public int DamageCapacity { get; set; } // Hits needed to destroy
        public string Domain { get; set; } // e.g., "Land"
        public string Special { get; set; } // e.g., "Radar: When present in a zone, owner is always the initiative player..."
        public string Model { get; set; } // e.g., "Type9625mmLOD0.glb"
        public string Gun { get; set; } // e.g., "Auto"

        // Constructor
        public AntiAircraftUnit() { }

        // Load data from a CSV row
        public void LoadFromCsv(Godot.Collections.Dictionary<string, Variant> csvRow)
        {
            try
            {
                Unit = csvRow["Unit"].AsString();
                Nationality = csvRow["Nationality"].AsString();
                Cost = csvRow["Cost"].AsInt32();
                AntiAircraftStrength = csvRow["AntiAircraft Strength"].AsInt32();
                DamageCapacity = csvRow["Damage Capacity"].AsInt32();
                Domain = csvRow["Domain"].AsString();
                Special = csvRow["Special"].AsString();
                Model = csvRow["Model"].AsString();
                Gun = csvRow["Gun"].AsString();
                GD.Print($"Loaded AntiAircraftUnit: {Unit}");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading AntiAircraftUnit from CSV: {e.Message}");
            }
        }

        // Roll anti-aircraft dice (hits on 6, per Standard Rules)
        public int RollAntiAircraftDice()
        {
            int hits = 0;
            Random rand = new();
            for (int i = 0; i < AntiAircraftStrength; i++)
            {
                if (rand.Next(1, 7) == 6) // Per rules, AA hits on 6
                    hits++;
            }
            GD.Print($"Rolling AA for {Unit}: {hits} hits");
            return hits;
        }
    }
}
