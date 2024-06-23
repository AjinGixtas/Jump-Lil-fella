using Godot;

public partial class LaserMenace : Enemy
{

	[Export] float MOVE_SPEED;
	float GRAVITY = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	SurfaceType SurfaceCurrentlyInContact
	{
		get { return surfaceCurrentlyInContact; }
		set
		{
			if (hasReachedCeiling) return;
			surfaceCurrentlyInContact = value;
			if (SurfaceType.NONE != surfaceCurrentlyInContact)
			{
				affectByGravity = false;
			}
			if (SurfaceType.CEILING == surfaceCurrentlyInContact)
			{
				hasReachedCeiling = true;
				GetNewTargetPosition();
			}
		}
	}
	Node2D c_collisionNode; KinematicCollision2D c_collision;
	Vector2 oldNormal = Vector2.Zero; [Export] Vector2 targetPosition = Vector2.Zero;
	[Export] bool affectByGravity = true, hasReachedCeiling = false, canAttack = true; bool hasReachedTargetPoint = false, isAttacking = false;
	[Export]
	bool HasReachedTargetPoint
	{
		get { return hasReachedTargetPoint; }
		set
		{
			hasReachedTargetPoint = value;
			ANIMATION_TREE.Set("parameters/conditions/isWaiting", value);
		}
	}
	[Export]
	bool IsAttacking
	{
		get { return isAttacking; }
		set
		{
			isAttacking = value;
		}
	}
	float resetTargetPositionCounter = 5.0f;
    public override void _Ready()
    {
  		rayCast.TargetPosition = new((GD.Randf() - .5f) * 500, 0);
    }
    public override void _PhysicsProcess(double delta)
	{
		if (!hasReachedCeiling)
		{
			if (affectByGravity) Velocity = new(Velocity.X, Velocity.Y + GRAVITY * (float)delta);
			c_collision = MoveAndCollide(Velocity * (float)delta);
			if (c_collision != null)
			{
				if (oldNormal != Vector2.Zero) { Velocity = oldNormal; }
				else
				{
					oldNormal = c_collision.GetNormal();
					if (GlobalPosition.X < 0)
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
		else
		{
			if (!HasReachedTargetPoint)
			{
				Velocity = (targetPosition - GlobalPosition).Normalized() * MOVE_SPEED;
				MoveAndSlide();
				HasReachedTargetPoint = Vector2Extensions.GetSquaredEuclideanDistance(GlobalPosition, targetPosition) < 25*25;
				ANIMATION_TREE.Set("parameters/conditions/isWalking", true);
				ANIMATION_TREE.Set("parameters/conditions/isWaiting", false);
			}
			else { 
				if (canAttack) ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
				ANIMATION_TREE.Set("parameters/conditions/isWalking", false);
				ANIMATION_TREE.Set("parameters/conditions/isWaiting", true);
			} 
		}
	}
	public void OnAttackCooldownTimerTimeout() { canAttack = true; }
	public void GetNewTargetPosition()
	{
		HasReachedTargetPoint = false;
		if (rayCast.IsColliding()) targetPosition = rayCast.GetCollisionPoint();
		else { targetPosition = rayCast.GlobalPosition + rayCast.TargetPosition; }
		rayCast.TargetPosition = new((GD.Randf() - .5f) * 500, 0);
	}
	[Export] RayCast2D rayCast;
	public void OnTakingDamage(Area2D area) {
		ANIMATION_TREE.Set("parameters/conditions/isTakingDamage", true);
	}
	public override void OnDeath() {
		base.OnDeath();
		ANIMATION_TREE.Set("parameters/conditions/isDead", true);
	}
	[Export] PackedScene LASER_SCENE;
	[Export] Node2D LASER_SPAWN_POINT;
	Dagger c_laser; // Dagger behave by moving with an intial velocity and choose to be affected by gravity or not
	// It's weird to have the dagger class in a laser attack but uhmmm... deal with it :)
	public void ShootLaser() {
		c_laser = LASER_SCENE.Instantiate<Dagger>();
		PROJECTILE_CONTAINER.AddChild(c_laser);
		c_laser.GlobalPosition = LASER_SPAWN_POINT.GlobalPosition;
		c_laser.LookAt(PLAYER.GlobalPosition);
		c_laser.Intialize(PLAYER.GlobalPosition - c_laser.GlobalPosition);
	}
}