public partial class SpikyBall : Enemy
{
	bool isAwake = false;
	public override void _PhysicsProcess(double delta) {
		
	}
	public void OnTakingDamage() {
		if(!isAwake) {
			isAwake = true;
			ANIMATION_TREE.Set("parameters/conditions/isAwake", true);
			return;
		}
		ANIMATION_TREE.Set("parameters/conditions/isTakingDamage", true);
	}
	public void OnDead() {
		ANIMATION_TREE.Set("parameters/conditions/isDead", true);
	}
}
