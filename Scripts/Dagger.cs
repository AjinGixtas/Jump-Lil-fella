using Godot;

public partial class Dagger : CharacterBody2D
{
	[Export] float SPEED; 
	[Export] bool IsMoving, affectByGravity;
	[Export] int amoutOfTarget;
	float GRAVITY = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	[Export] AnimationPlayer ANIMATION_PLAYER;
	KinematicCollision2D c_kinematicCollision;
	public void Intialize(Vector2 direction)
	{
		Velocity = direction.Normalized() * SPEED;
		GlobalRotation = Mathf.Atan2(Velocity.Y, Velocity.X);
	}
	public override void _PhysicsProcess(double delta)
	{
		if (IsMoving)
		{
			if(affectByGravity) {
				Velocity += new Vector2(0, GRAVITY * (float)delta);
				GlobalRotation = Mathf.Atan2(Velocity.Y, Velocity.X);
			}
			c_kinematicCollision = MoveAndCollide(Velocity);
			if (c_kinematicCollision != null && (c_kinematicCollision.GetCollider() as Node2D).IsInGroup("Wall"))
			{
				ANIMATION_PLAYER.Play("StuckToWall");
			}
		}
	}
	public void OnDealingDamage() {
		amoutOfTarget--;
		if(amoutOfTarget == 0) QueueFree();
	}
}
