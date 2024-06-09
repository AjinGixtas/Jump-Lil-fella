using Godot;

public partial class Dagger : Node2D
{
	[Export] float SPEED; public Vector2 Velocity;
	public void Intialize(Vector2 direction)
	{
		Velocity = direction.Normalized() * SPEED;
		GlobalRotation = Mathf.Atan2(Velocity.Y, Velocity.X);
	}
	public override void _Process(double delta) { Position += Velocity * (float)delta; }
}
