using Godot;
public partial class CAPTest : Node3D
{
    private AirUnit _airUnit;
    private Zone _zone;
    private Control _tooltip;
    private Node3D _ta152Model;

    public override void _Ready()
    {
        var unitDatabase = GetNode<UnitDatabase>("/root/UnitDatabase");
        _zone = GetNode<Zone>("ZoneBerlin");
        _airUnit = GetNode<AirUnit>("Ta152Unit");
        _tooltip = GetNode<Control>("TooltipLayer/Tooltip");
        _ta152Model = GetNode<Node3D>("Ta152Unit/Ta152Model");
        if (_zone == null || _airUnit == null || _tooltip == null || _ta152Model == null)
        {
            GD.PrintErr("Failed to find nodes: ZoneBerlin=", _zone, ", Ta152Unit=", _airUnit, ", Tooltip=", _tooltip, ", Ta152Model=", _ta152Model);
            return;
        }
        _zone.LoadFromCsv(unitDatabase.GetZoneAsDictionary("Berlin"));
        _airUnit.LoadFromCsv(unitDatabase.GetAirUnitAsDictionary("Ta-152"));
        GD.Print("Zone loaded: ", _zone.TargetName);
        GD.Print("AirUnit loaded: ", _airUnit.Unit);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
        {
            GD.Print($"Moving {_airUnit.Unit} to CAP area of {_zone.TargetName}");
            GD.Print($"Range: {_airUnit.Range}, Can move: true");
        }
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            var camera = GetViewport().GetCamera3D();
            var from = camera.ProjectRayOrigin(mouseEvent.Position);
            var to = from + camera.ProjectRayNormal(mouseEvent.Position) * 1000;
            var spaceState = GetWorld3D().DirectSpaceState;
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            var result = spaceState.IntersectRay(query);
            if (result.Count > 0 && result["collider"].AsGodotObject() == _ta152Model)
            {
                var label = _tooltip.GetNode<Label>("Label");
                label.Text = $"Unit: {_airUnit.Unit}\nRange: {_airUnit.Range}\nAir Attack: {_airUnit.AirAttackDice}";
                _tooltip.Visible = true;
                GD.Print("Tooltip shown for ", _airUnit.Unit);
            }
        }
    }
}