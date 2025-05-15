using Godot;
using System;
using System.Collections.Generic;
using CsvHelper;
using System.IO;
using System.Linq;

public partial class UnitDatabase : Node
{
	// Lists to store loaded data
	private List<AirUnit> airUnits = new List<AirUnit>();
	private List<AntiAircraftUnit> antiAircraftUnits = new List<AntiAircraftUnit>();
	private List<Zone> zones = new List<Zone>();
	private List<Aces> aces = new List<Aces>();

	// Public access to loaded data
	public List<AirUnit> AirUnits => airUnits;
	public List<AntiAircraftUnit> AntiAircraftUnits => antiAircraftUnits;
	public List<Zone> Zones => zones;
	public List<Aces> Aces => aces;

	// Called when the node enters the scene tree
	public override void _Ready()
	
	
	{
		LoadAllData();
	}

	// Load all CSV data
	private void LoadAllData()
	{
        GD.Print($"LoadAllData called at {Time.GetTicksMsec()}ms\nStack Trace: {new System.Diagnostics.StackTrace().ToString()}");
        airUnits = LoadAirUnits("res://Data/Raw Data/WEBasicAirUnits.csv");
		antiAircraftUnits = LoadAntiAircraftUnits("res://Data/Raw Data/WEBasicAntiAircraftUnits.csv");
		zones = LoadZones("res://Data/Raw Data/WEBasicZones.csv");
		aces = LoadAces("res://Data/Raw Data/WEBasicAces.csv");

		// Log counts to verify loading
		GD.Print($"Loaded {airUnits.Count} air units.");
		GD.Print($"Loaded {antiAircraftUnits.Count} anti-aircraft units.");
		GD.Print($"Loaded {zones.Count} zones.");
		GD.Print($"Loaded {aces.Count} aces.");
	}

	// Load air units from CSV
	private List<AirUnit> LoadAirUnits(string path)
	{
		var units = new List<AirUnit>();
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"Failed to open {path}");
			return units;
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
				if (values.Length < 12) // Expect 12 columns
				{
					GD.PrintErr($"Invalid air unit row: {line}");
					continue;
				}

				var dict = new Godot.Collections.Dictionary<string, Variant>
				{
					{ "Unit", values[0].Trim() },
					{ "Nationality", values[1].Trim() },
					{ "Type", values[2].Trim() },
					{ "Silhouette", values[3].Trim() },
					{ "Cost", ParseInt(values[4].Trim(), "Cost", line) },
					{ "Air Attack Dice", ParseInt(values[5].Trim(), "Air Attack Dice", line) },
					{ "Bombing Dice", ParseInt(values[6].Trim(), "Bombing Dice", line) },
					{ "Damage Capacity", ParseInt(values[7].Trim(), "Damage Capacity", line) },
					{ "Range", ParseInt(values[8].Trim(), "Range", line) },
					{ "Year", ParseInt(values[9].Trim(), "Year", line) },
					{ "Special 1", values[10].Trim() },
					{ "Special 2", values[11].Trim() }
				};

                var unit = new AirUnit();
				unit.LoadFromCsv(dict);
				units.Add(unit);
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error parsing air unit row: {line}. Error: {e.Message}");
			}
		}
		return units;
	}

	// Load anti-aircraft units from CSV
	private List<AntiAircraftUnit> LoadAntiAircraftUnits(string path)
	{
		var units = new List<AntiAircraftUnit>();
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"Failed to open {path}");
			return units;
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
				if (values.Length < 7) // Expect 7 columns
				{
					GD.PrintErr($"Invalid anti-aircraft unit row: {line}");
					continue;
				}

				var dict = new Godot.Collections.Dictionary<string, Variant>
				{
					{ "Unit", values[0].Trim() },
					{ "Nationality", values[1].Trim() },
					{ "Cost", ParseInt(values[2].Trim(), "Cost", line) },
					{ "Anti-Aircraft Dice", ParseInt(values[3].Trim(), "Anti-Aircraft Dice", line) },
					{ "Damage Capacity", ParseInt(values[4].Trim(), "Damage Capacity", line) },
					{ "Type", values[5].Trim() },
					{ "Special", values[6].Trim() }
				};

				var unit = new AntiAircraftUnit();
				unit.LoadFromCsv(dict);
				units.Add(unit);
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error parsing anti-aircraft unit row: {line}. Error: {e.Message}");
			}
		}
		return units;
	}

	// Load zones from CSV
	private List<Zone> LoadZones(string path)
	{
		var zones = new List<Zone>();
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"Failed to open {path}");
			return zones;
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
				if (values.Length < 11) // Expect 11 columns
				{
					GD.PrintErr($"Invalid zone row: {line}");
					continue;
				}

				var dict = new Godot.Collections.Dictionary<string, Variant>
				{
					{ "Target Name", values[0].Trim() },
					{ "Nationality", values[1].Trim() },
					{ "Type", values[2].Trim() },
					{ "AA Dice", ParseInt(values[3].Trim(), "AA Dice", line) },
					{ "Damage Capacity", ParseInt(values[4].Trim(), "Damage Capacity", line) },
					{ "Production", ParseInt(values[5].Trim(), "Production", line) },
					{ "VP Value", ParseInt(values[6].Trim(), "VP Value", line) },
					{ "Base Unit Name", values[7].Trim() },
					{ "Base Type", values[8].Trim() },
					{ "Base AA", ParseInt(values[9].Trim(), "Base AA", line) },
					{ "Base Damage Cap", ParseInt(values[10].Trim(), "Base Damage Cap", line) }
				};

				var zone = new Zone();
				zone.LoadFromCsv(dict);
				zones.Add(zone);
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error parsing zone row: {line}. Error: {e.Message}");
			}
		}
		return zones;
	}

	// Load aces from CSV
	private List<Aces> LoadAces(string path)
	{
		var aces = new List<Aces>();
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"Failed to open {path}");
			return aces;
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
					GD.PrintErr($"Invalid ace row: {line}");
					continue;
				}

				var dict = new Godot.Collections.Dictionary<string, Variant>
				{
					{ "Pilot", values[0].Trim() },
					{ "Nationality", values[1].Trim() },
					{ "Cost", ParseInt(values[2].Trim(), "Cost", line) },
					{ "Bonus", ParseInt(values[3].Trim(), "Bonus", line) }
				};

				var ace = new Aces();
				ace.LoadFromCsv(dict);
				aces.Add(ace);
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error parsing ace row: {line}. Error: {e.Message}");
			}
		}
		return aces;
	}

	// Helper method to parse integers with error handling
	private int ParseInt(string value, string fieldName, string line)
	{
		if (int.TryParse(value, out int result))
			return result;
		throw new FormatException($"Invalid {fieldName} value '{value}' in row: {line}");
	}

	// Utility method: Get air unit by name
	public AirUnit GetAirUnitByName(string unit)
	{
		return airUnits.Find(u => u.Unit == unit);
	}

	// Utility method: Get antiaircraft unit by name
	public AntiAircraftUnit GetAntiAircraftUnitByName(string unit)
	{
		return antiAircraftUnits.Find(u => u.Unit == unit);
	}

	// Utility method: Get zone by target name
	public Zone GetZoneByTargetName(string targetName)
	{
		return zones.Find(zone => zone.TargetName == targetName);
	}

	// Utility method: Get ace by pilot name
	public Aces GetAceByPilot(string pilot)
	{
		return aces.Find(ace => ace.Pilot == pilot);
    }

    public Godot.Collections.Dictionary<string, Variant> GetZoneAsDictionary(string targetName)
    {
        var zone = GetZoneByTargetName(targetName);
        if (zone == null) return new Godot.Collections.Dictionary<string, Variant>();
        return new Godot.Collections.Dictionary<string, Variant>
    {
        { "Target Name", zone.TargetName },
        { "Nationality", zone.TargetNationality },
        { "Type", zone.TargetType },
        { "AA Dice", zone.TargetAADice },
        { "Damage Capacity", zone.TargetDamageCapacity },
        { "Production", zone.TargetProduction },
        { "VP Value", zone.TargetVPValue },
        { "Base Unit Name", zone.BaseName },
        { "Base Type", zone.BaseType },
        { "Base AA", zone.BaseAADice },
        { "Base Damage Cap", zone.BaseDamageCapacity }
    };
    }

    public Godot.Collections.Dictionary<string, Variant> GetAirUnitAsDictionary(string unitName)
    {
        var unit = GetAirUnitByName(unitName);
        if (unit == null) return new Godot.Collections.Dictionary<string, Variant>();
        return new Godot.Collections.Dictionary<string, Variant>
    {
        { "Unit", unit.Unit },
        { "Nationality", unit.Nationality },
        { "Type", unit.Type },
        { "Silhouette", unit.Silhouette },
        { "Cost", unit.Cost },
        { "Air Attack Dice", unit.AirAttackDice },
        { "Bombing Dice", unit.BombingDice },
        { "Damage Capacity", unit.DamageCapacity },
        { "Range", unit.Range },
        { "Year", unit.Year },
        { "Special 1", unit.Special1 },
        { "Special 2", unit.Special2 }
    };
    }

}
