using Godot;

public partial class RobotDuelist : Enemy
{
	[Export] float JUMP_FORCE;
	[Export] Sprite2D SPRITE; [Export] AnimationTree ANIMATION_TREE; 
	[Export] Timer jumpAttackCooldownTimer, jumpDurationTimer; 
	float GRAVITY = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	bool canJump = true, isJumping = false;
	bool IsJumping { get { return isJumping; } set { isJumping = value; ANIMATION_TREE.Set("parameters/conditions/isJumping", value); }}
	KinematicCollision2D c_collision; Node2D c_collisionNode;
	SurfaceType SurfaceCurrentlyInContact {
		get { return surfaceCurrentlyInContact; }
		set {
			if(surfaceCurrentlyInContact != SurfaceType.NONE) { isJumping = false; ANIMATION_TREE.Set("parameters/conditions/isOnSurface", true); } else { ANIMATION_TREE.Set("parameters/conditions/isOnSurface", false); }
			if (c_collision != null) velocity = -c_collision.GetNormal();
			surfaceCurrentlyInContact = value;
		}
	}
	Vector2 velocity
	{
		get { return Velocity; }
		set
		{
			Velocity = value;
			if(value.X > 0) SPRITE.FlipH = true; else SPRITE.FlipH = false;
			ANIMATION_TREE.Set("parameters/FloatMidAir/blend_position", Velocity.Normalized());
			ANIMATION_TREE.Set("parameters/GrabSurface/blend_position", Velocity.Normalized());
		}
	}
	public override void _Process(double delta)
	{
		if (canJump)
		{
			velocity = (PLAYER.Position - Position).Normalized() * JUMP_FORCE;
			canJump = false; isJumping = true;
			jumpAttackCooldownTimer.Start(); jumpDurationTimer.Start();
		}
		GD.Print(canJump, isJumping, isJumping, (PLAYER.Position - Position).Normalized(), (PLAYER.Position - Position).Normalized() * JUMP_FORCE, SurfaceCurrentlyInContact, velocity, JUMP_FORCE);
		c_collision = MoveAndCollide(velocity);
		if(c_collision != null) {
			c_collisionNode = c_collision.GetCollider() as Node2D;
			if (c_collisionNode.IsInGroup("Floor")) { SurfaceCurrentlyInContact = SurfaceType.FLOOR; }
			else if (c_collisionNode.IsInGroup("WallLeft")) { SurfaceCurrentlyInContact = SurfaceType.WALL_LEFT; }
			else if (c_collisionNode.IsInGroup("WallRight")) { SurfaceCurrentlyInContact = SurfaceType.WALL_RIGHT; }
			else { SurfaceCurrentlyInContact = SurfaceType.NONE; }
		} else { SurfaceCurrentlyInContact = SurfaceType.NONE; }
		velocity = new(surfaceCurrentlyInContact == SurfaceType.NONE ? velocity.X : velocity.X * .97f, isJumping ? velocity.Y : velocity.Y + GRAVITY * (float)delta * (float)Engine.TimeScale);
	}
	public void OnJumpAttackCooldownTimerTimeout() { canJump = true; }
	public void OnJumpDurationTimerTimeout() { isJumping = false; }
}
