using Godot;

public partial class CAPTest : Node3D
{
    private AirUnit _airUnit;
    private Zone _zone;
    private Control _tooltip;
    private Label _statsLabel;
    private Node3D _ta152Model;
    private StaticBody3D _colliderBody; // Added for ColliderBody
    private bool _isDragging = false;
    private bool _isOrbiting = false;
    private Button _endPhaseButton;
    private float _orbitAngle = 0f;
    private const float OrbitRadius = 3f; // Distance from CAPArea center
    private const float OrbitSpeed = 1f; // Radians per second

    public override void _Ready()
    {
        var unitDatabase = GetNode<UnitDatabase>("/root/UnitDatabase");
        _zone = GetNode<Zone>("ZoneBerlin");
        _airUnit = GetNode<AirUnit>("Ta152Unit");
        _tooltip = GetNode<Control>("Tooltip Layer/Tooltip");
        _statsLabel = _tooltip.GetNode<Label>("StatsLabel");
        _ta152Model = GetNode<Node3D>("Ta152Unit/Ta152Model");
        _colliderBody = GetNode<StaticBody3D>("Ta152Unit/Ta152Model/Ta152LOD1/ColliderBody");

        if (_zone == null || _airUnit == null || _tooltip == null || _statsLabel == null || _ta152Model == null || _colliderBody == null)
        {
            GD.PrintErr($"Failed to find nodes: ZoneBerlin={_zone}, Ta152Unit={_airUnit}, Tooltip={_tooltip}, StatsLabel={_statsLabel}, Ta152Model={_ta152Model}, ColliderBody={_colliderBody}");
            return;
        }

        _zone.LoadFromCsv(unitDatabase.GetZoneAsDictionary("Berlin"));
        _airUnit.LoadFromCsv(unitDatabase.GetAirUnitAsDictionary("Ta-152"));

        _tooltip.Visible = false;

        _endPhaseButton = GetNode<Button>("UI/EndPhaseButton");
        _endPhaseButton.Visible = false;

        var capArea = GetNode<Area3D>("ZoneBerlin/CAPArea");
        if (capArea == null)
        {
            GD.PrintErr("CAPArea not found in ZoneBerlin");
        }
        else
        {
            GD.Print("CAPArea found: ", capArea.Name);
        }
    }

    public override void _Input(InputEvent @event)
    {


        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            var camera = GetViewport().GetCamera3D();
            var from = camera.ProjectRayOrigin(mouseEvent.Position);
            var to = from + camera.ProjectRayNormal(mouseEvent.Position) * 1000;
            var spaceState = GetWorld3D().DirectSpaceState;
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            var result = spaceState.IntersectRay(query);

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

                if (mouseEvent.IsPressed())
                {
                    _isDragging = true;
                    GD.Print("Started dragging Ta152Model");
                }
                else if (!mouseEvent.IsPressed() && _isDragging)
                {
                    _isDragging = false;
                    _tooltip.Visible = false;
                    var capArea = GetNode<Area3D>("ZoneBerlin/CAPArea");
                    if (capArea == null)
                    {
                        GD.PrintErr("CAPArea null on drop");
                        return;
                    }
                    var shapeCast = new ShapeCast3D();
                    shapeCast.Shape = new BoxShape3D { Size = new Vector3(10, 10, 10) };
                    shapeCast.GlobalTransform = new Transform3D(Basis.Identity, capArea.GlobalTransform.Origin);
                    shapeCast.TargetPosition = Vector3.Zero;
                    shapeCast.MaxResults = 1;
                    shapeCast.CollisionMask = 1;
                    AddChild(shapeCast);
                    shapeCast.ForceShapecastUpdate();
                    var modelPos = _colliderBody.GlobalTransform.Origin;
                    var areaPos = capArea.GlobalTransform.Origin;
                    var hit = shapeCast.IsColliding() && shapeCast.GetCollider(0) == _colliderBody;
                    GD.Print($"Drop debug: ModelPos={modelPos}, AreaPos={areaPos}, ShapeCastHit={hit}");
                    shapeCast.QueueFree();
                    if (hit)
                    {
                        GD.Print("Dropped Ta152Model in CAPArea - CAP designated");
                        _isOrbiting = true;
                        _endPhaseButton.Visible = true;
                    }
                    else
                    {
                        GD.Print("Dropped Ta152Model outside CAPArea");
                        _endPhaseButton.Visible = false;
                    }
                }
            }
            else if (!mouseEvent.IsPressed() && _isDragging)
            {
                _isDragging = false;
                _tooltip.Visible = false;
                GD.Print("Dropped Ta152Model outside CAPArea (no collider hit)");
            }
        }

        if (_isDragging && @event is InputEventMouseMotion mouseMotion)
        {
            var camera = GetViewport().GetCamera3D();
            var rayOrigin = camera.ProjectRayOrigin(mouseMotion.Position);
            var rayNormal = camera.ProjectRayNormal(mouseMotion.Position);
            var plane = new Plane(Vector3.Up, 0); // Ground plane at y=0
            var intersect = plane.IntersectsRay(rayOrigin, rayOrigin + rayNormal * 1000);
            if (intersect.HasValue)
            {
                _ta152Model.GlobalTransform = new Transform3D(_ta152Model.GlobalTransform.Basis, intersect.Value);
                GD.Print("Dragging Ta152Model to: ", intersect.Value);
            }
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isOrbiting) return;
        var capArea = GetNode<Area3D>("ZoneBerlin/CAPArea");
        if (capArea == null)
        {
            GD.PrintErr("CAPArea null in orbit");
            return;
        }
        _orbitAngle += (float)(OrbitSpeed * delta);
        var center = capArea.GlobalTransform.Origin;
        var newPos = center + new Vector3(
            Mathf.Sin(_orbitAngle) * OrbitRadius,
            0,
            Mathf.Cos(_orbitAngle) * OrbitRadius
        );
        _ta152Model.GlobalTransform = new Transform3D(_ta152Model.GlobalTransform.Basis, newPos);
        GD.Print($"Orbiting Ta152Model at: {newPos}");
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CS8321")]
    private void _on_end_phase_button_pressed()
    {
        GD.Print("End Phase button clicked - Closing game");
        GetTree().Quit();
    }
}