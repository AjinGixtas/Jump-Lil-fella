using Godot;

public partial class Enemy : CharacterBody2D {
	protected enum SurfaceType { NONE, FLOOR, WALL_LEFT, WALL_RIGHT, CEILING }
    public static Player PLAYER;
    [Export] protected int MAX_HEALTH;
    protected static Vector2 GRAVITY_VECTOR = new(0, 10.0f);
	protected SurfaceType surfaceCurrentlyInContact;
    [Export] public static Node2D PROJECTILE_CONTAINER;
}