using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float JUMP_FORCE, MAX_STAMINA, GRAVITY, FRICTION_FACTOR;
	[Export] TrajectoryPoint[] TRAJECTORY_POINTS;
	[Export] int AMOUNT_OF_TRAJECTORY_POINT;
	[Export] PackedScene TRAJECTORY_POINT_SCENE;
	[Export] Node2D TRAJECTORY_POINT_CONTAINER;
	[Export] AnimationPlayer ANIMATION_PLAYER;
	[Export] AnimationTree ANIMATION_TREE;
	[Export] Sprite2D SPRITE;
	enum SurfaceType { NONE, FLOOR, WALL_LEFT, WALL_RIGHT, CEILING }
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
				return;
			}

			ANIMATION_TREE.Set("parameters/conditions/isOnSurface", true);
			ANIMATION_TREE.Set("parameters/conditions/isFloating", false);
			CurrentStamina = 3;
			if (c_collision != null) velocity = -c_collision.GetNormal();
			surfaceCurrentlyInContact = value;
			affectByGravity = false;
			switch (surfaceCurrentlyInContact)
			{
				case SurfaceType.WALL_LEFT:
					break;
				case SurfaceType.WALL_RIGHT:
					break;
				case SurfaceType.CEILING:
					break;
				case SurfaceType.FLOOR:
					break;
			}
		}
	}
	Vector2 velocity
	{
		get { return Velocity; }
		set
		{
			if(Mathf.Abs(value.X) < 1) { isSliding = false; }
			Velocity = value;
			if(value.X > 0) SPRITE.FlipH = true; else SPRITE.FlipH = false;
			ANIMATION_TREE.Set("parameters/FloatMidAir/blend_position", Velocity.Normalized());
			ANIMATION_TREE.Set("parameters/GrabSurface/blend_position", Velocity.Normalized());
		}
	}
	Vector2 c_direction, c_CalculatingVector; KinematicCollision2D c_collision; Node2D c_collisionNode;
	public override void _Ready()
	{
		GRAVITY /= 60f;
		TrajectoryPoint.GRAVITY_VECTOR = new(0, GRAVITY);
		TrajectoryPoint.PLAYER = this;
		TRAJECTORY_POINTS = new TrajectoryPoint[AMOUNT_OF_TRAJECTORY_POINT];
		for (int i = 0; i < AMOUNT_OF_TRAJECTORY_POINT; ++i)
		{
			TRAJECTORY_POINTS[i] = TRAJECTORY_POINT_SCENE.Instantiate<TrajectoryPoint>();
			TRAJECTORY_POINT_CONTAINER.AddChild(TRAJECTORY_POINTS[i]);
		}
	}
	int counter, max_counter = 100;
	public override void _Process(double delta)
	{
		HandlePlayerInput();
		if (counter > max_counter)
			GD.Print(
				ANIMATION_TREE.Get("parameters/conditions/isAttacking"), ' ',
				ANIMATION_TREE.Get("parameters/Attack/conditions/whirlwindAttack"), ' ',
				ANIMATION_TREE.Get("parameters/conditions/isFloating"), ' ',
				ANIMATION_TREE.Get("parameters/conditions/isJumping"), ' ',
				ANIMATION_TREE.Get("parameters/conditions/isOnSurface"), ' ',
				ANIMATION_TREE.Get("parameters/FloatMidAir/blend_position"), ' ',
				ANIMATION_TREE.Get("parameters/GrabSurface/blend_position"), ' ',
				velocity);
		else ++counter;
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
				for (int i = 0; i < AMOUNT_OF_TRAJECTORY_POINT; ++i) { TRAJECTORY_POINTS[i].Update(canSlide, c_direction, i * .75f); }
			}
			else
			{
				TRAJECTORY_POINT_CONTAINER.Visible = false;
				// Engine.TimeScale = 1;
				if (Input.IsActionJustReleased("ui_jump") && (SurfaceCurrentlyInContact != SurfaceType.NONE || isSliding))
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
		}
		else
		{
			if (Input.IsActionJustPressed("ui_whirlwindAttack"))
			{
				ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
				ANIMATION_TREE.Set("parameters/Attack/conditions/whirlwindAttack", true);
			}
		}
	}
	void HandlePhysics()
	{
		if (!isSliding) velocity = new(velocity.X, affectByGravity ? velocity.Y + GRAVITY * (float)Engine.TimeScale : velocity.Y);
		else velocity = new(velocity.X * FRICTION_FACTOR, 0);
		c_collision = MoveAndCollide(velocity * (float)Engine.TimeScale);

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
	void HandleCounter()
	{
		CurrentStamina -= deltaF;
	}
}
