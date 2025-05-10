using Godot;
using System;

public partial class AntiAircraftUnit : Node
{
	// Properties matching WEBasicAntiAircraftUnits.csv columns
	public string Unit { get; set; } // e.g., "Type 96 25mm"
	public string Nationality { get; set; } // e.g., "Japan"
	public int Cost { get; set; } // Production Points cost
	public int AntiAircraftDice { get; set; } // Dice for attacking aircraft
	public int DamageCapacity { get; set; } // Hits needed to destroy
	public string Type { get; set; } // e.g., "Land"
	public string Special { get; set; } // e.g., "Radar: When present in a zone, owner is always the initiative player..."

	// Constructor
	public AntiAircraftUnit() { }

	// Load data from a CSV row
	public void LoadFromCsv(Godot.Collections.Dictionary<string, Variant> csvRow)
	{
		Unit = csvRow["Unit"].AsString();
		Nationality = csvRow["Nationality"].AsString();
		Cost = csvRow["Cost"].AsInt32();
		AntiAircraftDice = csvRow["Anti-Aircraft Dice"].AsInt32();
		DamageCapacity = csvRow["Damage Capacity"].AsInt32();
		Type = csvRow["Type"].AsString();
		Special = csvRow["Special"].AsString();
	}

	// Example method: Roll anti-aircraft dice (hits on 6)
	public int RollAntiAircraftDice()
	{
		int hits = 0;
		Random rand = new Random();
		for (int i = 0; i < AntiAircraftDice; i++)
		{
			if (rand.Next(1, 7) == 6) // Per rules, AA hits on 6
				hits++;
		}
		return hits;
	}
}
