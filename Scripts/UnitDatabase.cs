using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WarEaglesDigital.Scripts
{
    public partial class UnitDatabase : Node
    {
        // Lists to store loaded data
        private List<AirUnit> airUnits = [];
        private List<AntiAircraftUnit> antiAircraftUnits = [];
        private List<Zone> zones = [];
        private List<Aces> aces = [];
        private List<Event> events =[];

        // Public access to loaded data
        public List<AirUnit> AirUnits => airUnits;
        public List<AntiAircraftUnit> AntiAircraftUnits => antiAircraftUnits;
        public List<Zone> Zones => zones;
        public List<Aces> Aces => aces;
        public List<Event> Events => events;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            LoadAllData();
        }

        // Load all CSV data
        private void LoadAllData()
        {
            try
            {
                GD.Print($"LoadAllData called at {Time.GetTicksMsec()}ms\nStack Trace: {new System.Diagnostics.StackTrace()}");
                airUnits = LoadAirUnits("res://Data/RawData/WEBasicAirUnits.csv");
                antiAircraftUnits = LoadAntiAircraftUnits("res://Data/RawData/WEBasicAntiAircraftUnits.csv");
                zones = LoadZones("res://Data/RawData/WEBasicZones.csv");
                aces = LoadAces("res://Data/RawData/WEBasicAces.csv");
                events = LoadEvents("res://Data/RawData/WEBasicEvents.csv");

                // Log counts to verify loading
                GD.Print($"Loaded {airUnits.Count} air units.");
                GD.Print($"Loaded {antiAircraftUnits.Count} anti-aircraft units.");
                GD.Print($"Loaded {zones.Count} zones.");
                GD.Print($"Loaded {aces.Count} aces.");
                GD.Print($"Loaded {events.Count} events.");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in LoadAllData: {e.Message}");
            }
        }

        // Load air units from CSV
        private static List<AirUnit> LoadAirUnits(string path)
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
                    if (values.Length < 16) // Expect 16 columns
                    {
                        GD.PrintErr($"Invalid air unit row: {line}");
                        continue;
                    }

                    var dict = new Godot.Collections.Dictionary<string, Variant>
                    {
                        { "Unit", values[0].Trim() },
                        { "Nationality", values[1].Trim() },
                        { "Role", values[2].Trim() },
                        { "Weight", values[3].Trim() },
                        { "Cost", ParseInt(values[4].Trim(), "Cost", line) },
                        { "Air Attack Strength", ParseInt(values[5].Trim(), "Air Attack Strength", line) },
                        { "Bombing Strength", ParseInt(values[6].Trim(), "Bombing Strength", line) },
                        { "Damage Capacity", ParseInt(values[7].Trim(), "Damage Capacity", line) },
                        { "Fuel", ParseInt(values[8].Trim(), "Fuel", line) },
                        { "Year", ParseInt(values[9].Trim(), "Year", line) },
                        { "Special 1", values[10].Trim() },
                        { "Special 2", values[11].Trim() },
                        { "Model LOD0", values[12].Trim() },
                        { "Model LOD1", values[13].Trim() },
                        { "Motor", values[14].Trim() },
                        { "Gun", values[15].Trim() }
                    };

                    var unit = new AirUnit();
                    unit.LoadFromCsv(dict);
                    GD.Print($"Loaded air unit: {unit.Unit}");
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
        private static List<AntiAircraftUnit> LoadAntiAircraftUnits(string path)
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
                    if (values.Length < 9) // Expect 9 columns
                    {
                        GD.PrintErr($"Invalid anti-aircraft unit row: {line}");
                        continue;
                    }

                    var dict = new Godot.Collections.Dictionary<string, Variant>
                    {
                        { "Unit", values[0].Trim() },
                        { "Nationality", values[1].Trim() },
                        { "Cost", ParseInt(values[2].Trim(), "Cost", line) },
                        { "AntiAircraft Strength", ParseInt(values[3].Trim(), "AntiAircraft Strength", line) },
                        { "Damage Capacity", ParseInt(values[4].Trim(), "Damage Capacity", line) },
                        { "Domain", values[5].Trim() },
                        { "Special", values[6].Trim() },
                        { "Model", values[7].Trim() },
                        { "Gun", values[8].Trim() }
                    };

                    var unit = new AntiAircraftUnit();
                    unit.LoadFromCsv(dict);
                    GD.Print($"Loaded anti-aircraft unit: {unit.Unit}");
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
        private static List<Zone> LoadZones(string path)
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
                    if (values.Length < 12) // Expect 12 columns
                    {
                        GD.PrintErr($"Invalid zone row: {line}");
                        continue;
                    }

                    var dict = new Godot.Collections.Dictionary<string, Variant>
                    {
                        { "Target Name", values[0].Trim() },
                        { "Nationality", values[1].Trim() },
                        { "Domain", values[2].Trim() },
                        { "AA Strength", ParseInt(values[3].Trim(), "AA Strength", line) },
                        { "Damage Capacity", ParseInt(values[4].Trim(), "Damage Capacity", line) },
                        { "Production", ParseInt(values[5].Trim(), "Production", line) },
                        { "VP Value", ParseInt(values[6].Trim(), "VP Value", line) },
                        { "Facility", values[7].Trim() },
                        { "Category", values[8].Trim() },
                        { "Facility AA", ParseInt(values[9].Trim(), "Facility AA", line) },
                        { "Facility Damage Cap", ParseInt(values[10].Trim(), "Facility Damage Cap", line) },
                        { "Model", values[11].Trim() }
                    };

                    var zone = new Zone();
                    zone.LoadFromCsv(dict);
                    GD.Print($"Loaded zone: {zone.TargetName}");
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
        private static List<Aces> LoadAces(string path)
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
                    if (values.Length < 6) // Expect 6 columns
                    {
                        GD.PrintErr($"Invalid ace row: {line}");
                        continue;
                    }

                    var dict = new Godot.Collections.Dictionary<string, Variant>
                    {
                        { "Pilot", values[0].Trim() },
                        { "Nationality", values[1].Trim() },
                        { "Cost", ParseInt(values[2].Trim(), "Cost", line) },
                        { "Bonus", ParseInt(values[3].Trim(), "Bonus", line) },
                        { "Model", values[4].Trim() },
                        { "FlavorText", values[5].Trim() }
                    };

                    var ace = new Aces();
                    ace.LoadFromCsv(dict);
                    GD.Print($"Loaded ace: {ace.Pilot}");
                    aces.Add(ace);
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error parsing ace row: {line}. Error: {e.Message}");
                }
            }
            return aces;
        }

        // Load events from CSV
        private static List<Event> LoadEvents(string path)
        {
            var events = new List<Event>();
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Failed to open {path}");
                return events;
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
                    if (values.Length < 9) // Expect 9 columns
                    {
                        GD.PrintErr($"Invalid event row: {line}");
                        continue;
                    }

                    var dict = new Godot.Collections.Dictionary<string, Variant>
                    {
                        { "Name", values[0].Trim() },
                        { "Nationality", values[1].Trim() },
                        { "Cost", ParseInt(values[2].Trim(), "Cost", line) },
                        { "Effect", values[3].Trim() },
                        { "Area of Effect", values[4].Trim() },
                        { "Duration", values[5].Trim() },
                        { "Maximum Quantity", ParseInt(values[6].Trim(), "Maximum Quantity", line) },
                        { "Model", values[7].Trim() },
                        { "Sound", values[8].Trim() }
                    };

                    var evt = new Event();
                    evt.LoadFromCsv(dict);
                    GD.Print($"Loaded event: {evt.Name}");
                    events.Add(evt);
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error parsing event row: {line}. Error: {e.Message}");
                }
            }
            return events;
        }

        // Helper method to parse integers with error handling
        private static int ParseInt(string value, string fieldName, string line)
        {
            if (int.TryParse(value, out int result))
                return result;
            throw new FormatException($"Invalid {fieldName} value '{value}' in row: {line}");
        }

        // Utility method: Get air unit by name
        public AirUnit GetAirUnitByName(string unit)
        {
            GD.Print($"GetAirUnitByName called for: \"{unit}\"");
            GD.Print($"Available air unit names: {string.Join(", ", airUnits.Select(u => $"\"{u.Unit}\""))}");
            var foundAirUnit = airUnits.Find(u => u.Unit.Equals(unit, StringComparison.OrdinalIgnoreCase));
            if (foundAirUnit == null)
            {
                GD.PrintErr($"Unit not found: \"{unit}\"");
            }
            else
            {
                GD.Print($"Found unit: \"{foundAirUnit.Unit}\"");
            }
            return foundAirUnit;
        }

        // Utility method: Get antiaircraft unit by name
        public AntiAircraftUnit GetAntiAircraftUnitByName(string unit)
        {
            var foundUnit = antiAircraftUnits.Find(u => u.Unit.Equals(unit, StringComparison.OrdinalIgnoreCase));
            if (foundUnit == null)
            {
                GD.PrintErr($"Anti-aircraft unit not found: \"{unit}\"");
            }
            else
            {
                GD.Print($"Found anti-aircraft unit: \"{foundUnit.Unit}\"");
            }
            return foundUnit;
        }

        // Utility method: Get zone by target name
        public Zone GetZoneByTargetName(string targetName)
        {
            var foundZone = zones.Find(z => z.TargetName.Equals(targetName, StringComparison.OrdinalIgnoreCase));
            if (foundZone == null)
            {
                GD.PrintErr($"Zone not found: \"{targetName}\"");
            }
            else
            {
                GD.Print($"Found zone: \"{foundZone.TargetName}\"");
            }
            return foundZone;
        }

        // Utility method: Get ace by pilot name
        public Aces GetAceByPilot(string pilot)
        {
            var foundAce = aces.Find(a => a.Pilot.Equals(pilot, StringComparison.OrdinalIgnoreCase));
            if (foundAce == null)
            {
                GD.PrintErr($"Ace not found: \"{pilot}\"");
            }
            else
            {
                GD.Print($"Found ace: \"{foundAce.Pilot}\"");
            }
            return foundAce;
        }

        // Utility method: Get event by name
        public Event GetEventByName(string name)
        {
            GD.Print($"GetEventByName called for: \"{name}\"");
            GD.Print($"Available event names: {string.Join(", ", events.Select(e => $"\"{e.Name}\""))}");
            var foundEvent = events.Find(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (foundEvent == null)
            {
                GD.PrintErr($"Event not found: \"{name}\"");
            }
            else
            {
                GD.Print($"Found event: \"{foundEvent.Name}\"");
            }
            return foundEvent;
        }

        // Utility method: Get zone as dictionary
        public static Godot.Collections.Dictionary<string, Variant> GetZoneAsDictionary(string targetName)
        {
            var unitDatabase = new UnitDatabase();
            var zone = unitDatabase.GetZoneByTargetName(targetName);
            if (zone == null) return [];
            return new Godot.Collections.Dictionary<string, Variant>
            {
                { "Target Name", zone.TargetName },
                { "Nationality", zone.TargetNationality },
                { "Domain", zone.TargetDomain },
                { "AA Strength", zone.TargetAAStrength },
                { "Damage Capacity", zone.TargetDamageCapacity },
                { "Production", zone.TargetProduction },
                { "VP Value", zone.TargetVPValue },
                { "Facility", zone.FacilityName },
                { "Category", zone.FacilityCategory },
                { "Facility AA", zone.FacilityAAStrength },
                { "Facility Damage Cap", zone.FacilityDamageCapacity },
                { "Model", zone.Model }
            };
        }

        // Utility method: Get air unit as dictionary
        public static Godot.Collections.Dictionary<string, Variant> GetAirUnitAsDictionary(string unitName)
        {
            var unitDatabase = new UnitDatabase();
            var unit = unitDatabase.GetAirUnitByName(unitName);
            if (unit == null) return [];
            return new Godot.Collections.Dictionary<string, Variant>
            {
                { "Unit", unit.Unit },
                { "Nationality", unit.Nationality },
                { "Role", unit.Role },
                { "Weight", unit.Weight },
                { "Cost", unit.Cost },
                { "Air Attack Strength", unit.AirAttackStrength },
                { "Bombing Strength", unit.BombingStrength },
                { "Damage Capacity", unit.DamageCapacity },
                { "Fuel", unit.Fuel },
                { "Year", unit.Year },
                { "Special 1", unit.Special1 },
                { "Special 2", unit.Special2 },
                { "Model LOD0", unit.ModelLOD0 },
                { "Model LOD1", unit.ModelLOD1 },
                { "Motor", unit.Motor },
                { "Gun", unit.Gun }
            };
        }

        // Utility method: Get event as dictionary
        public static Godot.Collections.Dictionary<string, Variant> GetEventAsDictionary(string name)
        {
            var unitDatabase = new UnitDatabase();
            var evt = unitDatabase.GetEventByName(name);
            if (evt == null) return [];
            return new Godot.Collections.Dictionary<string, Variant>
            {
                { "Name", evt.Name },
                { "Nationality", evt.Nationality },
                { "Cost", evt.Cost },
                { "Effect", evt.Effect },
                { "Area of Effect", evt.AreaOfEffect },
                { "Duration", evt.Duration },
                { "Maximum Quantity", evt.MaximumQuantity },
                { "Model", evt.Model },
                { "Sound", evt.Sound }
            };
        }

        
    }
}