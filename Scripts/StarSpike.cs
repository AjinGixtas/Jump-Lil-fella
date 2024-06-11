using Godot;

public partial class StarSpike : CharacterBody2D
{
	[Export] AnimationPlayer ANIMATION_PLAYER;
	[Export] Timer FUSE_TIMER;
	[Export] float SPEED;
	float GRAVITY = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	public void Intialize(Vector2 direction) {
		Velocity = direction.Normalized() * SPEED;
		FUSE_TIMER.Start();
	}
	public override void _PhysicsProcess(double delta)
	{
		Velocity += new Vector2(0, GRAVITY * (float)delta);
		if(MoveAndCollide(Velocity) != null) { ANIMATION_PLAYER.Play("SpikeBurst"); }
	}
	public void OnFuseTimerTimeout() { ANIMATION_PLAYER.Play("SpikeBurst"); }
}
