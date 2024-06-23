
using Godot;

public partial class SpearMan : Enemy
{
    [Export] Timer CHANGE_DIRECTION_VELOCITY_TIMER, ATTACK_COOLDOWN_TIMER;
    [Export] float MOVE_SPEED, CHARGE_SPEED, MIN_DISTANCE, MAX_DISTANCE, CHANGE_DIRECTION_VELOCITY_TIMER_DEFAULT_WAITIME, ATTACK_COOLDOWN_TIMER_DEFAULT_WAITIME;
    [Export] bool canThrowSpear = true, canCharge = true, isThrowingSpear, isCharging;
    bool IsBusy { get { return isThrowingSpear || isCharging; } }
    float c_playerDistance;
    Vector2 velocity
    {
        get { return Velocity; }
        set { Velocity = value; }
    }
    Vector2 randomDirection;
    [Export] PackedScene SPEAR_SCENE;
    Spear c_spear;
    public override void _Ready()
    {
        randomDirection = Vector2Extensions.GetRandomVector() * MOVE_SPEED;
    }
    public override void _PhysicsProcess(double delta)
    {
        SPRITE.FlipH = GlobalPosition.X < PLAYER.Position.X;
        if (isDead)
        {
            Velocity += GRAVITY_VECTOR * (float)delta;
            if (IsOnFloor())
                ANIMATION_TREE.Set("parameters/conditions/isOnGround", true);
        }
        else if (!IsBusy)
        {
            if (canCharge)
            {
                ANIMATION_TREE.Set("parameters/conditions/isCharge", true);
                ANIMATION_TREE.Set("parameters/conditions/isWaiting", false);
            }
            else if (canThrowSpear)
            {
                ANIMATION_TREE.Set("parameters/conditions/isThrowspear", true);
                ANIMATION_TREE.Set("parameters/conditions/isWaiting", false);
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
        }
        else if (isCharging) { Velocity = (PLAYER.GlobalPosition - GlobalPosition).Normalized() * CHARGE_SPEED; }
        MoveAndSlide();
    }
    void MoveTowardPlayer()
    {
        velocity = (PLAYER.GlobalPosition - GlobalPosition).Normalized() * MOVE_SPEED;
    }
    public void ThrowSpear()
    {
        c_spear = SPEAR_SCENE.Instantiate<Spear>();
        c_spear.Intialize(PLAYER.GlobalPosition - GlobalPosition);
        c_spear.GlobalPosition = GlobalPosition;
        PROJECTILE_CONTAINER.AddChild(c_spear);
    }
    bool attackTypeFlip;
    public void OnAttackCooldowntTimerTimeout()
    {
        attackTypeFlip = !attackTypeFlip;
        if (attackTypeFlip) { canThrowSpear = true; } else { canCharge = true; }
    }
    public void OnChangeDirectionVelocityTimerTimeout()
    {
        randomDirection = Vector2Extensions.GetRandomVector() * MOVE_SPEED;
    }
    public void OnDealingDamage()
    {
        ANIMATION_TREE.Set("parameters/conditions/isWaiting", true);
        ANIMATION_TREE.Set("parameters/conditions/isThrowspear", false);
        ANIMATION_TREE.Set("parameters/conditions/isCharge", false);
        ANIMATION_TREE.Set("parameters/conditions/endedCharge", true);
        isCharging = false;
    }
    bool isDead;
    public override void OnDeath()
    {
		base.OnDeath();
        ANIMATION_TREE.Set("parameters/conditions/isDead", true);
        isDead = true;
    }
    public void OnTakingDamage(Area2D area)
    {
        ANIMATION_TREE.Set("parameters/conditions/isTakingDamage", true);
    }
    public void StartAttackCooldownTimer() { ATTACK_COOLDOWN_TIMER.Start(ATTACK_COOLDOWN_TIMER_DEFAULT_WAITIME + (GD.Randf() - .5f) * 2f); }
    public void StartChangeDirectionVelocityTimer() { CHANGE_DIRECTION_VELOCITY_TIMER.Start(CHANGE_DIRECTION_VELOCITY_TIMER_DEFAULT_WAITIME + (GD.Randf() - .5f) * 2f); }
}
