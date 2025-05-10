using Godot;
using System;

public partial class Zone : Node
{
	// Target Unit properties (from WEBasicZones.csv)
	public string TargetName { get; set; } // e.g., "Berlin"
	public string TargetNationality { get; set; } // e.g., "Germany"
	public string TargetType { get; set; } // e.g., "Land"
	public int TargetAADice { get; set; } // Anti-aircraft dice
	public int TargetDamageCapacity { get; set; } // Hits to destroy
	public int TargetProduction { get; set; } // Production Points
	public int TargetVPValue { get; set; } // Victory Points if undestroyed

	// Base Unit properties (from WEBasicZones.csv)
	public string BaseName { get; set; } // e.g., "Oberdorf"
	public string BaseType { get; set; } // e.g., "Airstrip", "Airfield", "Airbase"
	public int BaseAADice { get; set; } // Anti-aircraft dice
	public int BaseDamageCapacity { get; set; } // Hits to destroy

	// Constructor
	public Zone() { }

	// Load data from a CSV row
	public void LoadFromCsv(Godot.Collections.Dictionary<string, Variant> csvRow)
	{
		TargetName = csvRow["Target Name"].AsString();
		TargetNationality = csvRow["Nationality"].AsString();
		TargetType = csvRow["Type"].AsString();
		TargetAADice = csvRow["AA Dice"].AsInt32();
		TargetDamageCapacity = csvRow["Damage Capacity"].AsInt32();
		TargetProduction = csvRow["Production"].AsInt32();
		TargetVPValue = csvRow["VP Value"].AsInt32();
		BaseName = csvRow["Base Unit Name"].AsString();
		BaseType = csvRow["Base Type"].AsString();
		BaseAADice = csvRow["Base AA"].AsInt32();
		BaseDamageCapacity = csvRow["Base Damage Cap"].AsInt32();
	}

	// Example method: Roll combined AA dice for Target and Base
	public int RollAADice()
	{
		int hits = 0;
		Random rand = new Random();
		for (int i = 0; i < TargetAADice + BaseAADice; i++)
		{
			if (rand.Next(1, 7) == 6) // Per rules, AA hits on 6
				hits++;
		}
		return hits;
	}
}
