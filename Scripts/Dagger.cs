using Godot;

public partial class Dagger : CharacterBody2D
{
	[Export] float SPEED;
	[Export] bool IsMoving;
	[Export] AnimationPlayer ANIMATION_PLAYER;
	KinematicCollision2D c_kinematicCollision;
	public void Intialize(Vector2 direction)
	{
		Velocity = direction.Normalized() * SPEED;
		GlobalRotation = Mathf.Atan2(Velocity.Y, Velocity.X);
	}
	public override void _Process(double delta)
	{
		if (IsMoving)
		{
			c_kinematicCollision = MoveAndCollide(Velocity * (float)delta);
			if (c_kinematicCollision != null && (c_kinematicCollision.GetCollider() as Node2D).IsInGroup("Wall"))
			{
				ANIMATION_PLAYER.Play("StuckToWall");
			}
		}
	}
}
