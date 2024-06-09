using Godot;

public partial class Hitbox : Area2D {
    [Export] float damage;
	public void OnAreaEntered(Area2D area) {
        (area as Hurtbox).TakingDamage(damage);
	}
}
