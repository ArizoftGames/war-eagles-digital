using Godot;
using System;

public partial class AirUnit : Node
{
	// Properties matching WEBasicAirUnits.csv columns

	[Export]
	public string Unit { get; set; } // e.g., "Ki-43 Hayabusa"
	public string Nationality { get; set; } // e.g., "Japan"
	public string Type { get; set; } // e.g., "Fighter", "Bomber"
	public string Silhouette { get; set; } // e.g., "Light", "Medium", "Heavy"
	public int Cost { get; set; } // Production Points cost
	public int AirAttackDice { get; set; } // Dice for air-to-air combat
	public int BombingDice { get; set; } // Dice for bombing surface units
	public int DamageCapacity { get; set; } // Hits needed to destroy
	public int Range { get; set; } // Zones movable per phase
	public int Year { get; set; } // Year available
	public string Special1 { get; set; } // e.g., "Dive Bomber: Each hit vs maritime targets counts as 2 hits."
	public string Special2 { get; set; } // Second special ability, if any

	// Constructor
	public AirUnit() { }

    // Load data from a CSV row (assuming CSV parsed into a Dictionary)
    public void LoadFromCsv(Godot.Collections.Dictionary<string, Variant> csvRow)
    {

        Unit = csvRow["Unit"].AsString();
        Nationality = csvRow["Nationality"].AsString();
        Type = csvRow["Type"].AsString();
        Silhouette = csvRow["Silhouette"].AsString();
        Cost = csvRow["Cost"].AsInt32();
        AirAttackDice = csvRow["Air Attack Dice"].AsInt32();
        BombingDice = csvRow["Bombing Dice"].AsInt32();
        DamageCapacity = csvRow["Damage Capacity"].AsInt32();
        Range = csvRow["Range"].AsInt32();
        Year = csvRow["Year"].AsInt32();
        Special1 = csvRow["Special 1"].AsString();
        Special2 = csvRow["Special 2"].AsString();
    }

    // Example method: Roll air attack dice (hits on 6)
    public int RollAirAttackDice()
	{
		int hits = 0;
		Random rand = new Random();
		for (int i = 0; i < AirAttackDice; i++)
		{
			if (rand.Next(1, 7) == 6) // Per rules, air attack hits on 6
				hits++;
		}
		return hits;
	}
}
