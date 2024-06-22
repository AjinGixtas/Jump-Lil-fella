using Godot;

public partial class SawBlade : CharacterBody2D
{
	public static Player PLAYER;
	[Export] Sprite2D SPRITE;
	[Export] AnimationTree ANIMATION_TREE;
	[Export] float FLYING_SPEED, MOVE_SPEED = 30;
	enum SurfaceType { NONE, FLOOR, WALL_LEFT, WALL_RIGHT, CEILING }
	float GRAVITY = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	SurfaceType surfaceCurrentlyInContact;
	SurfaceType SurfaceCurrentlyInContact
	{
		get { return surfaceCurrentlyInContact; }
		set
		{
			surfaceCurrentlyInContact = value;
			if (SurfaceType.NONE != surfaceCurrentlyInContact)
			{
				ANIMATION_TREE.Set("parameters/conditions/isOnSurface", true);
				affectByGravity = false;
			}
		}
	}
	public void Intialize(Vector2 direction, Vector2 inheritedMomentum) { Velocity = direction.Normalized() * FLYING_SPEED + inheritedMomentum; }
	Node2D c_collisionNode; KinematicCollision2D c_collision;
	Vector2 oldNormal = Vector2.Zero; bool affectByGravity = true;
	float stopDuration, liveDuration = 1;
	public override void _PhysicsProcess(double delta)
	{
		liveDuration -= (float)delta;
		if (liveDuration <= 0)
		{
			ANIMATION_TREE.Set("parameters/conditions/isDestroy", true);
		}
		if (affectByGravity) Velocity = new(Velocity.X, Velocity.Y + GRAVITY * (float)delta);
		if (stopDuration <= 0)
		{
			c_collision = MoveAndCollide(Velocity * (float)delta);
			if (c_collision != null)
			{
				if (oldNormal != Vector2.Zero) { Velocity = oldNormal; }
				else
				{
					oldNormal = c_collision.GetNormal();
					if (Vector2Extensions.GetPerpendicularVector(oldNormal).Dot(GlobalPosition - PLAYER.GlobalPosition) >= 0)
					{ Velocity = Vector2Extensions.GetPerpendicularVector(oldNormal); SPRITE.FlipH = true; }
					else { Velocity = Vector2Extensions.GetOppositePerpendicularVector(oldNormal); }
				}
				oldNormal = c_collision.GetNormal();
				Velocity = Velocity.Normalized() * MOVE_SPEED;
				SPRITE.GlobalRotation = Mathf.Atan2(oldNormal.Y, oldNormal.X) + Mathf.Pi / 2f;
				c_collisionNode = c_collision.GetCollider() as Node2D;
				if (c_collisionNode.IsInGroup("Floor")) { SurfaceCurrentlyInContact = SurfaceType.FLOOR; }
				else if (c_collisionNode.IsInGroup("WallLeft")) { SurfaceCurrentlyInContact = SurfaceType.WALL_LEFT; }
				else if (c_collisionNode.IsInGroup("WallRight")) { SurfaceCurrentlyInContact = SurfaceType.WALL_RIGHT; }
				else if (c_collisionNode.IsInGroup("Ceiling")) { SurfaceCurrentlyInContact = SurfaceType.CEILING; }
				else { SurfaceCurrentlyInContact = SurfaceType.NONE; }
			}
			else { SurfaceCurrentlyInContact = SurfaceType.NONE; }
		}
		else stopDuration -= (float)delta;
	}
	public void OnLifetimeTimerTimeout()
	{
	}
	public void OnDealingDamage()
	{
		ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
		stopDuration = .33f;
		liveDuration += .2f;
	}
}
