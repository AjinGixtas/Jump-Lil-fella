using Godot;

public partial class TrajectoryPoint : Node2D
{
	public static Vector2 GRAVITY_VECTOR;
	public bool Update(Vector2 intialPosition, bool canSlide, Vector2 direction, float timeStep, float FRICTION_FACTOR) {
		 GlobalPosition = intialPosition +  direction * timeStep + (canSlide? Vector2.Zero : .5f * timeStep * timeStep * GRAVITY_VECTOR / 60);
		if(Mathf.Abs(GlobalPosition.X) > 205 || Mathf.Abs(GlobalPosition.Y) > 104) return false;
		return true;
	}
}
