using Godot;
using System;

namespace WarEaglesDigital.Scripts
{
    public partial class AirUnit : Node
    {
        // Properties matching WEBasicAirUnits.csv columns
        [Export]
        public string Unit { get; set; } // e.g., "A6M5 Zero"
        public string Nationality { get; set; } // e.g., "Japan"
        public string Role { get; set; } // e.g., "Fighter", "Bomber"
        public string Weight { get; set; } // e.g., "Light", "Medium", "Heavy"
        public int Cost { get; set; } // Production Points cost
        public int AirAttackStrength { get; set; } // Dice for air-to-air combat
        public int BombingStrength { get; set; } // Dice for bombing surface units
        public int DamageCapacity { get; set; } // Hits needed to destroy
        public int Fuel { get; set; } // Zones movable per phase
        public int Year { get; set; } // Year available
        public string Special1 { get; set; } // e.g., "Dive Bomber: Each hit vs Antiaircraft Units counts as 2 hits."
        public string Special2 { get; set; } // Second special ability, if any
        public string ModelLOD0 { get; set; } // e.g., "A6M5LOD0.glb"
        public string ModelLOD1 { get; set; } // e.g., "A6M5LOD1.glb"
        public string Motor { get; set; } // e.g., "radial"
        public string Gun { get; set; } // e.g., "twenty"

        // Constructor
        public AirUnit() { }

        // Load data from a CSV row
        public void LoadFromCsv(Godot.Collections.Dictionary<string, Variant> csvRow)
        {
            try
            {
                GD.Print("AirUnit dictionary keys: ", string.Join(", ", csvRow.Keys));
                Unit = csvRow["Unit"].AsString();
                Nationality = csvRow["Nationality"].AsString();
                Role = csvRow["Role"].AsString();
                Weight = csvRow["Weight"].AsString();
                Cost = csvRow["Cost"].AsInt32();
                AirAttackStrength = csvRow["Air Attack Strength"].AsInt32();
                BombingStrength = csvRow["Bombing Strength"].AsInt32();
                DamageCapacity = csvRow["Damage Capacity"].AsInt32();
                Fuel = csvRow["Fuel"].AsInt32();
                Year = csvRow["Year"].AsInt32();
                Special1 = csvRow["Special 1"].AsString();
                Special2 = csvRow["Special 2"].AsString();
                ModelLOD0 = csvRow["Model LOD0"].AsString();
                ModelLOD1 = csvRow["Model LOD1"].AsString();
                Motor = csvRow["Motor"].AsString();
                Gun = csvRow["Gun"].AsString();
                GD.Print($"Loaded AirUnit: {Unit}");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading AirUnit from CSV: {e.Message}");
            }
        }

        // Roll air attack dice (hits on 6, per Standard Rules)
        public int RollAirAttackDice()
        {
            int hits = 0;
            Random rand = new();
            for (int i = 0; i < AirAttackStrength; i++)
            {
                if (rand.Next(1, 7) == 6) // Per rules, air attack hits on 6
                    hits++;
            }
            GD.Print($"Rolling air attack for {Unit}: {hits} hits");
            return hits;
        }
    }
}
