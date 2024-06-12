using Godot;

public partial class Hitbox : Area2D {
    [Export] float damage;
    [Signal] public delegate void OnDealingDamageEventHandler();
	public void OnAreaEntered(Area2D area) {
        EmitSignal(SignalName.OnDealingDamage);
        (area as Hurtbox).TakingDamage(damage);
	}
}
