using Godot;
using System;

public partial class Aces : Node
{
	// Properties matching WEBasicAces.csv columns
	public string Pilot { get; set; } // e.g., "Saburo Sakai"
	public string Nationality { get; set; } // e.g., "Japan"
	public int Cost { get; set; } // Production Points cost
	public int Bonus { get; set; } // Bonus to air attack dice

	// Constructor
	public Aces() { }

	// Load data from a CSV row
	public void LoadFromCsv(Godot.Collections.Dictionary<string, Variant> csvRow)
	{
		Pilot = csvRow["Pilot"].AsString();
		Nationality = csvRow["Nationality"].AsString();
		Cost = csvRow["Cost"].AsInt32();
		Bonus = csvRow["Bonus"].AsInt32();
	}
}
