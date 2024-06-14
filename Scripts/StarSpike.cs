using Godot;

public partial class StarSpike : CharacterBody2D
{
	[Export] AnimationTree ANIMATION_TREE;
	[Export] Timer FUSE_TIMER;
	[Export] float SPEED;
	[Export] float MAX_HEALTH;
	[Export] float currentHealth;
	float CurrentHealth { 
		get { return currentHealth; } 
		set { 
			currentHealth = value;
			if(currentHealth <= 0) { ANIMATION_TREE.Set("parameters/conditions/isDestroyed", true); } 
		} 
	}
	[Export] bool isMoving;
	float GRAVITY = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * 2f;
	public void Intialize(Vector2 direction)
	{
		CurrentHealth = MAX_HEALTH;
		Velocity = direction.Normalized() * SPEED;
		FUSE_TIMER.Start();
	}
	public override void _PhysicsProcess(double delta)
	{
		CurrentHealth -= (float)delta;
		if (isMoving) {
			Velocity += new Vector2(0, GRAVITY * (float)delta);
			if (MoveAndCollide(Velocity) != null) { ANIMATION_TREE.Set("parameters/conditions/isStopped", true); isMoving = false;  }
		}
		GD.Print(CurrentHealth);
	}
	public void OnFuseTimerTimeout() { ANIMATION_TREE.Set("parameters/conditions/isStopped", true); isMoving = false; }
	public void OnDealingDamage() { CurrentHealth -= .33f; ANIMATION_TREE.Set("parameters/conditions/isAttacking", true); }
}
