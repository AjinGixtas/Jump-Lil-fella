using Godot;

public partial class Enemy : CharacterBody2D {
	protected enum SurfaceType { NONE, FLOOR, WALL_LEFT, WALL_RIGHT, CEILING }
    public static Player PLAYER;
    protected static Vector2 GRAVITY_VECTOR = new(0, ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle());
	protected SurfaceType surfaceCurrentlyInContact;
    [Export] public static Node2D PROJECTILE_CONTAINER;
	[Export] protected AnimationTree ANIMATION_TREE;
	[Export] protected Sprite2D SPRITE;
}