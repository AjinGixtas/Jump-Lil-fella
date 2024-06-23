using Godot;

public partial class RobotDuelist : Enemy
{
	[Export] Node2D FIRE_SPOT;
	[Export] Timer JUMP_COOLDOWN_TIMER, ATTACK_COOLDOWN_TIMER;
	[Export] float JUMP_FORCE;
	[Export] bool affectByGravity, canAttack = true, canJump, isAttacking;
	[Export] PackedScene DAGGER_SCENE;
	Vector2 velocity
	{
		get { return Velocity; }
		set
		{
			Velocity = value;
			if (value.X != 0 && !isAttacking) SPRITE.FlipH = value.X > 0;
			else if (isAttacking) SPRITE.FlipH = PLAYER.Position.X > Position.X;
			ANIMATION_TREE.Set("parameters/FloatMidAir/blend_position", Velocity.Normalized());
			ANIMATION_TREE.Set("parameters/GrabSurface/blend_position", Velocity.Normalized());
			ANIMATION_TREE.Set("parameters/LandOnSurface/blend_position", Velocity.Normalized());
		}
	}
	KinematicCollision2D c_kinematicCollision; Node2D c_collisionNode;
	SurfaceType SurfaceCurrentlyInContact
	{
		get { return surfaceCurrentlyInContact; }
		set
		{
			surfaceCurrentlyInContact = value;
			if (c_kinematicCollision != null) velocity = -c_kinematicCollision.GetNormal();
			if (surfaceCurrentlyInContact == SurfaceType.NONE || surfaceCurrentlyInContact == SurfaceType.CEILING)
			{
				affectByGravity = true;
				ANIMATION_TREE.Set("parameters/conditions/isJumping", true);
				ANIMATION_TREE.Set("parameters/conditions/isOnSurface", false);
			}
			else
			{
				affectByGravity = false;
				ANIMATION_TREE.Set("parameters/conditions/isJumping", false);
				ANIMATION_TREE.Set("parameters/conditions/isOnSurface", true);
			}
		}
	}
	public override void _Process(double delta)
	{
		if (canAttack && SurfaceCurrentlyInContact == SurfaceType.NONE && Mathf.Abs(Velocity.Y) < 2) { ANIMATION_TREE.Set("parameters/conditions/isAttacking", true); canAttack = false; ATTACK_COOLDOWN_TIMER.Start(); }
		if (canJump) Jump();
	}
	float deltaF;
	public override void _PhysicsProcess(double delta)
	{
		deltaF = (float)delta;
		velocity += affectByGravity ? GRAVITY_VECTOR * deltaF : Vector2.Zero;
		c_kinematicCollision = MoveAndCollide(velocity * deltaF);
		if (c_kinematicCollision != null)
		{
			c_collisionNode = c_kinematicCollision.GetCollider() as Node2D;
			if (c_collisionNode.IsInGroup("Floor")) { SurfaceCurrentlyInContact = SurfaceType.FLOOR; }
			else if (c_collisionNode.IsInGroup("WallLeft")) { SurfaceCurrentlyInContact = SurfaceType.WALL_LEFT; }
			else if (c_collisionNode.IsInGroup("WallRight")) { SurfaceCurrentlyInContact = SurfaceType.WALL_RIGHT; }
			else if (c_collisionNode.IsInGroup("Ceiling")) { velocity = Vector2.Zero; }
			else { SurfaceCurrentlyInContact = SurfaceType.NONE; }
		}
		else { SurfaceCurrentlyInContact = SurfaceType.NONE; }
	}
	public void Jump()
	{
		if (c_kinematicCollision == null || !canJump) return;
		canJump = false;
		velocity = (c_kinematicCollision.GetNormal() + new Vector2(GD.Randf() - .5f, (GD.Randf() - .5f) / 10)).Normalized() * JUMP_FORCE;
		JUMP_COOLDOWN_TIMER.Start();
	}
	Dagger c_daggerInstance; int c_spawnPointIndex;
	public void ThrowDagger()
	{
		velocity = new(velocity.X / 2f, 0);
		c_daggerInstance = DAGGER_SCENE.Instantiate<Dagger>();
		PROJECTILE_CONTAINER.AddChild(c_daggerInstance);
		c_daggerInstance.GlobalPosition = FIRE_SPOT.GlobalPosition;
		SPRITE.FlipH = c_daggerInstance.Velocity.X > 0;
		c_daggerInstance.Intialize((PLAYER.GlobalPosition - FIRE_SPOT.GlobalPosition).Normalized() + PLAYER.Velocity.Normalized() * .33f);
	}
	public void OnJumpCooldownTimerTimeout() { canJump = true; }
	public void OnAttackCooldownTimerTimeout() { canAttack = true; }
	public void OnTakingDamage(Area2D area) { ANIMATION_TREE.Set("parameters/conditions/isTakingDamage", true); }
	public override void OnDeath()
	{
		base.OnDeath();
		surfaceCurrentlyInContact = SurfaceType.NONE; 
		Velocity = Vector2.Zero; 
		ANIMATION_TREE.Set("parameters/conditions/isDead", true);
	}
}
