
using Godot;

public partial class SpearMan : Enemy
{
    [Export] Timer CHANGE_DIRECTION_VELOCITY_TIMER;
    [Export] float MOVE_SPEED, CHARGE_SPEED, MIN_DISTANCE, MAX_DISTANCE;
    [Export] bool canThrowSpear = true, canCharge = true, isThrowingSpear, isCharging;
    bool IsBusy { get { return isThrowingSpear || isCharging; } }
    float c_playerDistance;
    Vector2 velocity
    {
        get { return Velocity; }
        set { Velocity = value; }
    }
    Vector2 randomDirection;
    public override void _Ready() {
        randomDirection = Vector2Extensions.GetRandomVector() * MOVE_SPEED;
    }
    public override void _Process(double delta)
    {
        if (!IsBusy)
        {
            if(canCharge) ANIMATION_TREE.Set("parameters/conditions/isCharge", true);
            else if(canThrowSpear) ANIMATION_TREE.Set("parameters/conditions/isThrowspear", true);
            else {
                c_playerDistance = Vector2Extensions.GetSquaredEuclideanDistance(PLAYER.GlobalPosition, GlobalPosition);
                if(c_playerDistance < MIN_DISTANCE) Velocity = (PLAYER.GlobalPosition - GlobalPosition).Normalized() * -MOVE_SPEED;
                else if(c_playerDistance > MAX_DISTANCE) Velocity = (PLAYER.GlobalPosition - GlobalPosition).Normalized() * MOVE_SPEED;
                else {
                    Velocity = randomDirection;
                    if(CHANGE_DIRECTION_VELOCITY_TIMER.IsStopped()) { CHANGE_DIRECTION_VELOCITY_TIMER.Start(); }
                }
                MoveAndSlide();
            }
        } else if(isCharging) { Charge(); }
        // GD.Print(canThrowSpear, canCharge, isThrowingSpear, isCharging, ANIMATION_TREE.Get("parameters/conditions/isThrowspear"), ANIMATION_TREE.Get("parameters/conditions/isWaiting"), ANIMATION_TREE.Get("parameters/conditions/isCharge"));
    }
    void MoveTowardPlayer()
    {
        velocity = (PLAYER.GlobalPosition - GlobalPosition).Normalized() * MOVE_SPEED;
        MoveAndSlide();
    }
    public void ThrowSpear() { GD.Print("Throw spear~"); }
    void Charge() { 
        Velocity = (PLAYER.GlobalPosition - GlobalPosition).Normalized() * CHARGE_SPEED;
        MoveAndSlide(); 
    }
    bool attackTypeFlip;
    public void OnAttackCooldowntTimerTimeout() {
        attackTypeFlip = !attackTypeFlip;
        if(attackTypeFlip) { canThrowSpear = true; } else { canCharge = true; }
    }
    public void OnChangeDirectionVelocityTimerTimeout() {
        randomDirection = Vector2Extensions.GetRandomVector() * MOVE_SPEED;
    }
    public void OnDealingDamage() {
        
    }
}
