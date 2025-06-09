using Godot;
using System;

namespace WarEaglesDigital.Scripts
{

    public partial class Event : Node
    {
        // Properties matching WEBasicEvents.csv columns
        [Export]
        public string Title { get; set; } // e.g., "Ballistic Missiles"
        public string Nationality { get; set; } // e.g., "Germany"
        public int Cost { get; set; } // Production Points cost
        public string Effect { get; set; } // e.g., "If the Peenemünde target is in play..."
        public string AreaOfEffect { get; set; } // e.g., "1 Zone"
        public string Duration { get; set; } // e.g., "Immediate"
        public int MaximumQuantity { get; set; } // e.g., 2
        public string Model { get; set; } // e.g., "missile.png"
        public string Sound { get; set; } // e.g., "missile"

        // Constructor
        public Event() { }

        // Load data from a CSV row
        public void LoadFromCsv(Godot.Collections.Dictionary<string, Variant> csvRow)
        {
            try
            {
                Name = csvRow["Title"].AsString();
                Nationality = csvRow["Nationality"].AsString();
                Cost = csvRow["Cost"].AsInt32();
                Effect = csvRow["Effect"].AsString();
                AreaOfEffect = csvRow["Area of Effect"].AsString();
                Duration = csvRow["Duration"].AsString();
                MaximumQuantity = csvRow["Maximum Quantity"].AsInt32();
                Model = csvRow["Model"].AsString();
                Sound = csvRow["Sound"].AsString();
            }
            catch (Exception e)
            {
                GD.PrintErr($"Event.cs: Error loading CSV row: {e.Message}");
            }
        }
    }
}
    