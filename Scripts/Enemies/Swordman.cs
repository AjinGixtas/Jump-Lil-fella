using Godot;

public partial class Swordman : Enemy
{
	[Export] Timer CHANGE_DIRECTION_VELOCITY_TIMER, ATTACK_COOLDOWN_TIMER;
	[Export] float MOVE_SPEED, CHARGE_SPEED, CHARGE_RANGE_SQUARED, MIN_DISTANCE, MAX_DISTANCE, ATTACK_COOLDOWN_TIMER_DEFAULT_WAITIME, CHANGE_DIRECTION_VELOCITY_TIMER_DEFAULT_WAITIME;
	[Export] bool canCharge, isCharging, isDead;
	Vector2 randomDirection, lockedChargeDirection;
	float c_playerDistance;
	public override void _Ready() { randomDirection = Vector2Extensions.GetRandomVector() * MOVE_SPEED; }
	public override void _PhysicsProcess(double delta)
	{
		if(!isCharging) SPRITE.FlipH = GlobalPosition.X < PLAYER.Position.X;
		if (isDead)
		{
			Velocity += GRAVITY_VECTOR;
			if (IsOnFloor())
				ANIMATION_TREE.Set("parameters/conditions/isOnGround", true);
		}
		else if (canCharge)
		{
			c_playerDistance = Vector2Extensions.GetSquaredEuclideanDistance(PLAYER.GlobalPosition, GlobalPosition);
			Velocity = (PLAYER.GlobalPosition - GlobalPosition).Normalized();
			if (c_playerDistance > CHARGE_RANGE_SQUARED) { Velocity *= MOVE_SPEED; }
			else { ANIMATION_TREE.Set("parameters/conditions/isAttacking", true); Velocity = lockedChargeDirection = Velocity * CHARGE_SPEED; }
		}
		else if (isCharging)
		{
			Velocity = lockedChargeDirection;
		}
		else
		{
			c_playerDistance = Vector2Extensions.GetSquaredEuclideanDistance(PLAYER.GlobalPosition, GlobalPosition);
			if (c_playerDistance < MIN_DISTANCE)
			{
				Velocity = (PLAYER.GlobalPosition - GlobalPosition).Normalized() * -MOVE_SPEED;
				randomDirection = Vector2Extensions.GetRandomVector() * MOVE_SPEED;
			}
			else if (c_playerDistance > MAX_DISTANCE)
			{
				Velocity = (PLAYER.GlobalPosition - GlobalPosition).Normalized() * MOVE_SPEED;
				randomDirection = Vector2Extensions.GetRandomVector() * MOVE_SPEED;
			}
			else
			{
				Velocity = randomDirection;
				if (CHANGE_DIRECTION_VELOCITY_TIMER.IsStopped()) { StartChangeDirectionVelocityTimer(); }
			}
		}
		MoveAndSlide();
	}
	public void OnChangeDirectionVelocityTimerTimeout()
	{ randomDirection = Vector2Extensions.GetRandomVector() * MOVE_SPEED; }
    public void OnDeath()
    {
        ANIMATION_TREE.Set("parameters/conditions/isDead", true);
        isDead = true;
    }
    public void OnTakingDamage(Area2D area)
    {
        ANIMATION_TREE.Set("parameters/conditions/isTakingDamage", true);
    }
	public void OnAttackCooldowntTimerTimeout() { canCharge = true; }
	public void StartAttackCooldownTimer() { ATTACK_COOLDOWN_TIMER.Start(ATTACK_COOLDOWN_TIMER_DEFAULT_WAITIME + (GD.Randf() - .5f) * 2f); }
	public void StartChangeDirectionVelocityTimer() { CHANGE_DIRECTION_VELOCITY_TIMER.Start(CHANGE_DIRECTION_VELOCITY_TIMER_DEFAULT_WAITIME + (GD.Randf() - .5f) * 2f); }
}
