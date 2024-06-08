using Godot;

public partial class Enemy : CharacterBody2D {
	protected enum SurfaceType { NONE, FLOOR, WALL_LEFT, WALL_RIGHT, CEILING }
    public static Player PLAYER;
    [Export] protected int MAX_HEALTH;
	protected SurfaceType surfaceCurrentlyInContact;
}