using Godot;

public partial class Spear : CharacterBody2D
{
	[Export] float SPEED;
	[Export] int amoutOfTarget;
	[Export] AnimationPlayer ANIMATION_PLAYER;
	KinematicCollision2D c_kinematicCollision;
	public void Intialize(Vector2 direction)
	{
		Velocity = direction.Normalized() * SPEED;
		GlobalRotation = Mathf.Atan2(Velocity.Y, Velocity.X);
	}
	public override void _PhysicsProcess(double delta)
	{
		c_kinematicCollision = MoveAndCollide(Velocity);
		if (c_kinematicCollision != null && (c_kinematicCollision.GetCollider() as Node2D).IsInGroup("Wall"))
		{
			ANIMATION_PLAYER.Play("StuckToWall");
		}
	}
}
