using Godot;
using System;

namespace WarEaglesDigital.Scripts
{

    public partial class Aces : Node
    {
        // Properties matching WEBasicAces.csv columns
        public string Pilot { get; set; } // e.g., "Saburo Sakai"
        public string Nationality { get; set; } // e.g., "Japan"
        public int Cost { get; set; } // Production Points cost
        public int Bonus { get; set; } // Bonus to air attack dice
        public string Model { get; set; } // e.g., "sakai.png"
        public string FlavorText { get; set; } // e.g., "Tainan Kokutai ace with true samurai spirit..."

        // Constructor
        public Aces() { }

        // Load data from a CSV row
        public void LoadFromCsv(Godot.Collections.Dictionary<string, Variant> csvRow)
        {
            try
            {
                Pilot = csvRow["Pilot"].AsString();
                Nationality = csvRow["Nationality"].AsString();
                Cost = csvRow["Cost"].AsInt32();
                Bonus = csvRow["Bonus"].AsInt32();
                Model = csvRow["Model"].AsString();
                FlavorText = csvRow["FlavorText"].AsString();

            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading Ace from CSV: {e.Message}");
            }
        }
    }
}