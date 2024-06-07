using Godot;

public partial class TrajectoryPoint : Node2D
{
	public static Vector2 GRAVITY_VECTOR;
	public static Player PLAYER;
	public bool Update(Vector2 direction, float timeStep) {
		GlobalPosition = PLAYER.GlobalPosition + direction * timeStep + .5f * timeStep * timeStep * GRAVITY_VECTOR;
		if(Mathf.Abs(GlobalPosition.X) > 205 || Mathf.Abs(GlobalPosition.Y) > 104) return false;
		return true;
	}
}
