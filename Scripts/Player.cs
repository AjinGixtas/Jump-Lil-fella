using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float JUMP_FORCE, MAX_STAMINA, FRICTION_FACTOR;
	[Export] TrajectoryPoint[] TRAJECTORY_POINTS;
	[Export] int AMOUNT_OF_TRAJECTORY_POINT;
	[Export] PackedScene TRAJECTORY_POINT_SCENE, DAGGER_SCENE;
	[Export] Node2D TRAJECTORY_POINT_CONTAINER, PROJECTILE_CONTAINER, DAGGER_SPAWN_POINTS_CONTAINER;
	[Export] Node2D[] DAGGER_SPAWN_POINTS;
	[Export] AnimationPlayer ANIMATION_PLAYER;
	[Export] AnimationTree ANIMATION_TREE;
	[Export] Sprite2D SPRITE;
	enum SurfaceType { NONE, FLOOR, WALL_LEFT, WALL_RIGHT, CEILING }
	float GRAVITY = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	[Export] bool affectByGravity = true, isSliding = false;
	float currentStamina, deltaF;
	float CurrentStamina
	{
		get { return currentStamina; }
		set
		{
			currentStamina = Mathf.Max(0, value);
			if (currentStamina == 0) affectByGravity = true;
		}
	}
	SurfaceType surfaceCurrentlyInContact;
	SurfaceType SurfaceCurrentlyInContact
	{
		get { return surfaceCurrentlyInContact; }
		set
		{
			if (value == SurfaceType.NONE)
			{
				affectByGravity = true; surfaceCurrentlyInContact = value;
				ANIMATION_TREE.Set("parameters/conditions/isFloating", true);
				ANIMATION_TREE.Set("parameters/conditions/isOnSurface", false);
				ANIMATION_TREE.Set("parameters/Attack/ThrowDagger/blend_position", Vector2.Zero);
				return;
			}

			ANIMATION_TREE.Set("parameters/conditions/isOnSurface", true);
			ANIMATION_TREE.Set("parameters/conditions/isFloating", false);
			CurrentStamina = 3;
			if (c_collision != null) velocity = -c_collision.GetNormal();
			surfaceCurrentlyInContact = value;
			affectByGravity = false;
			switch(surfaceCurrentlyInContact) {
				case SurfaceType.FLOOR:
					ANIMATION_TREE.Set("parameters/Attack/ThrowDagger/blend_position", Vector2.Down);
					break;
				case SurfaceType.WALL_LEFT:
					ANIMATION_TREE.Set("parameters/Attack/ThrowDagger/blend_position", Vector2.Left);
					break;
				case SurfaceType.WALL_RIGHT:
					ANIMATION_TREE.Set("parameters/Attack/ThrowDagger/blend_position", Vector2.Right);
					break;
				case SurfaceType.CEILING:
					ANIMATION_TREE.Set("parameters/Attack/ThrowDagger/blend_position", Vector2.Up);
					break;
			}
		}
	}
	Vector2 velocity
	{
		get { return Velocity; }
		set
		{
			if (Mathf.Abs(value.X) < 1) { isSliding = false; }
			Velocity = value;
			if (value.X != 0) SPRITE.FlipH = value.X > 0;
			ANIMATION_TREE.Set("parameters/FloatMidAir/blend_position", Velocity.Normalized());
			ANIMATION_TREE.Set("parameters/GrabSurface/blend_position", Velocity.Normalized());
		}
	}
	Vector2 c_direction, c_CalculatingVector; KinematicCollision2D c_collision; Node2D c_collisionNode;
	public override void _Ready()
	{
		TrajectoryPoint.GRAVITY_VECTOR = new(0, GRAVITY);
		Enemy.PLAYER = this;
		TRAJECTORY_POINTS = new TrajectoryPoint[AMOUNT_OF_TRAJECTORY_POINT];
		for (int i = 0; i < AMOUNT_OF_TRAJECTORY_POINT; ++i)
		{
			TRAJECTORY_POINTS[i] = TRAJECTORY_POINT_SCENE.Instantiate<TrajectoryPoint>();
			TRAJECTORY_POINT_CONTAINER.AddChild(TRAJECTORY_POINTS[i]);
		}
		Enemy.PROJECTILE_CONTAINER = PROJECTILE_CONTAINER;
	}
	int counter, max_counter = 100;
	public override void _Process(double delta)
	{
		HandlePlayerInput();
	}
	public override void _PhysicsProcess(double delta)
	{
		deltaF = (float)delta;
		HandlePhysics();
		HandleCounter();
	}
	bool canSlide;
	void HandlePlayerInput()
	{
		if (SurfaceCurrentlyInContact != SurfaceType.NONE || isSliding)
		{
			TRAJECTORY_POINT_CONTAINER.Visible = false;
			if (Input.IsActionPressed("ui_jump"))
			{
				canSlide = false;
				TRAJECTORY_POINT_CONTAINER.Visible = true;
				c_direction = GetGlobalMousePosition() - GlobalPosition;
				if (SurfaceCurrentlyInContact == SurfaceType.CEILING) { c_direction = new(c_direction.X, Mathf.Max(0, c_direction.Y)); }
				else if (SurfaceCurrentlyInContact == SurfaceType.FLOOR)
				{
					c_CalculatingVector = c_direction * c_direction;
					if (c_CalculatingVector.X / (c_CalculatingVector.X + c_CalculatingVector.Y) >= 0.933012701894f) { c_direction = new(c_direction.X, 0); canSlide = true; }
				}
				else if (SurfaceCurrentlyInContact == SurfaceType.WALL_LEFT) { c_direction = new(Mathf.Max(0, c_direction.X), c_direction.Y); }
				else if (SurfaceCurrentlyInContact == SurfaceType.WALL_RIGHT) { c_direction = new(Mathf.Min(0, c_direction.X), c_direction.Y); }
				c_direction = c_direction.Normalized() * JUMP_FORCE;
				for (int i = 0; i < AMOUNT_OF_TRAJECTORY_POINT; ++i) { TRAJECTORY_POINTS[i].Update(Position, canSlide, c_direction, i * .75f, FRICTION_FACTOR); }
			}
			else if (Input.IsActionJustReleased("ui_jump"))
			{
				c_direction = GetGlobalMousePosition() - GlobalPosition;
				isSliding = false;
				if (SurfaceCurrentlyInContact == SurfaceType.CEILING) { c_direction = new(c_direction.X, Mathf.Max(0, c_direction.Y)); }
				else if (SurfaceCurrentlyInContact == SurfaceType.FLOOR)
				{
					c_CalculatingVector = c_direction * c_direction;
					isSliding = c_CalculatingVector.X / (c_CalculatingVector.X + c_CalculatingVector.Y) >= 0.933012701894f;
					if (isSliding) c_direction = new(c_direction.X, 0);
				}
				else if (SurfaceCurrentlyInContact == SurfaceType.WALL_LEFT) { c_direction = new(Mathf.Max(0, c_direction.X), c_direction.Y); }
				else if (SurfaceCurrentlyInContact == SurfaceType.WALL_RIGHT) { c_direction = new(Mathf.Min(0, c_direction.X), c_direction.Y); }
				velocity = c_direction.Normalized() * JUMP_FORCE;
				ANIMATION_TREE.Set("parameters/conditions/isJumping", true);
			}
		}
		else
		{
			if (Input.IsActionJustPressed("ui_whirlwindAttack"))
			{
				ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
				ANIMATION_TREE.Set("parameters/Attack/conditions/whirlwindAttack", true);
				velocity = new(velocity.X / 2f, Mathf.Min(-2.5f, velocity.Y - 1));
				ANIMATION_TREE.Set("parameters/conditions/isJumping", true);
			}
		}
		if (Input.IsActionJustPressed("ui_throwDagger"))
		{
			ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
			ANIMATION_TREE.Set("parameters/Attack/conditions/ThrowDagger", true);
		}
	}
	void HandlePhysics()
	{
		if (!isSliding) velocity = new(velocity.X, affectByGravity ? velocity.Y + GRAVITY * deltaF : velocity.Y);
		else velocity = new(velocity.X * FRICTION_FACTOR, 0);
		c_collision = MoveAndCollide(velocity);

		if (c_collision != null)
		{
			c_collisionNode = c_collision.GetCollider() as Node2D;
			if (c_collisionNode.IsInGroup("Floor")) { SurfaceCurrentlyInContact = SurfaceType.FLOOR; }
			else if (c_collisionNode.IsInGroup("WallLeft")) { SurfaceCurrentlyInContact = SurfaceType.WALL_LEFT; }
			else if (c_collisionNode.IsInGroup("WallRight")) { SurfaceCurrentlyInContact = SurfaceType.WALL_RIGHT; }
			else if (c_collisionNode.IsInGroup("Ceiling")) { SurfaceCurrentlyInContact = SurfaceType.CEILING; }
			else { SurfaceCurrentlyInContact = SurfaceType.NONE; }
		}
		else { SurfaceCurrentlyInContact = SurfaceType.NONE; }
	}
	void HandleCounter() { CurrentStamina -= deltaF; }
	Dagger c_daggerInstance;
	public void ThrowDagger() {
		DAGGER_SPAWN_POINTS_CONTAINER.LookAt(GetGlobalMousePosition());

		for(int i = 0; i < DAGGER_SPAWN_POINTS.Length; ++i) {
			c_daggerInstance = DAGGER_SCENE.Instantiate<Dagger>();
			PROJECTILE_CONTAINER.AddChild(c_daggerInstance);
			c_daggerInstance.GlobalPosition = DAGGER_SPAWN_POINTS[i].GlobalPosition;
			c_daggerInstance.GlobalRotation = DAGGER_SPAWN_POINTS[i].GlobalRotation;
			c_daggerInstance.Intialize(new(Mathf.Cos(c_daggerInstance.GlobalRotation), Mathf.Sin(c_daggerInstance.GlobalRotation)));
		}
	}
}
