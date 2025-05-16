using Godot;

public partial class CAPTest : Node3D
{
    private AirUnit _airUnit;
    private Zone _zone;
    private Control _tooltip;
    private Label _statsLabel;
    private Node3D _ta152Model;
    private StaticBody3D _colliderBody; // Added for ColliderBody

    public override void _Ready()
    {
        var unitDatabase = GetNode<UnitDatabase>("/root/UnitDatabase");
        _zone = GetNode<Zone>("ZoneBerlin");
        _airUnit = GetNode<AirUnit>("Ta152Unit");
        _tooltip = GetNode<Control>("Tooltip Layer/Tooltip");
        GD.Print("Tooltip node: ", _tooltip?.ToString() ?? "null");
        _statsLabel = _tooltip.GetNode<Label>("StatsLabel");
        _ta152Model = GetNode<Node3D>("Ta152Unit/Ta152Model");
        _colliderBody = GetNode<StaticBody3D>("Ta152Unit/Ta152Model/Ta152LOD1/ColliderBody");

        if (_zone == null || _airUnit == null || _tooltip == null || _statsLabel == null || _ta152Model == null || _colliderBody == null)
        {
            GD.PrintErr($"Failed to find nodes: ZoneBerlin={_zone}, Ta152Unit={_airUnit}, Tooltip={_tooltip}, StatsLabel={_statsLabel}, Ta152Model={_ta152Model}, ColliderBody={_colliderBody}");
            return;
        }

        _zone.LoadFromCsv(unitDatabase.GetZoneAsDictionary("Berlin"));
        GD.Print("Loading air unit with name: Ta-152");
        _airUnit.LoadFromCsv(unitDatabase.GetAirUnitAsDictionary("Ta-152"));
        GD.Print("Zone loaded: ", _zone.TargetName);
        GD.Print("AirUnit loaded: ", _airUnit.Unit);

        _tooltip.Visible = false;
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

            GD.Print("Raycast result: Count=", result.Count, ", Collider=", result.ContainsKey("collider") ? result["collider"].ToString() : "none");

            if (result.Count > 0 && result["collider"].AsGodotObject() == _colliderBody)
            {
                _statsLabel.Text = $"Unit: {_airUnit.Unit}\n" +
                                   $"Nationality: {_airUnit.Nationality}\n" +
                                   $"Type: {_airUnit.Type}\n" +
                                   $"Silhouette: {_airUnit.Silhouette}\n" +
                                   $"Cost: {_airUnit.Cost}\n" +
                                   $"Air Attack: {_airUnit.AirAttackDice}\n" +
                                   $"Bombing: {_airUnit.BombingDice}\n" +
                                   $"Damage Capacity: {_airUnit.DamageCapacity}\n" +
                                   $"Range: {_airUnit.Range}\n" +
                                   $"Year: {_airUnit.Year}\n" +
                                   $"Special 1: {_airUnit.Special1}\n" +
                                   $"Special 2: {_airUnit.Special2}";

                var screenPos = camera.UnprojectPosition(_ta152Model.GlobalTransform.Origin);
                _tooltip.Position = screenPos + new Vector2(20, -_tooltip.Size.Y / 2);

                _tooltip.Visible = true;
                GD.Print("Tooltip shown for ", _airUnit.Unit);
            }
            else
            {
                _tooltip.Visible = false;
                GD.Print("Click missed Ta152Model");
            }
        }
    }
}