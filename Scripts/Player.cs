using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float JUMP_FORCE, MAX_STAMINA, FRICTION_FACTOR, DASH_FORCE;
	[Export] TextureProgressBar SMALL_DAGGER_BAR, BIG_DAGGER_BAR, SAW_BLADE_BAR, SPIKE_STAR_BAR;
	[Export] TrajectoryPoint[] TRAJECTORY_POINTS;
	[Export] int AMOUNT_OF_TRAJECTORY_POINT;
	[Export] PackedScene TRAJECTORY_POINT_SCENE, SMALL_DAGGER_SCENE, BIG_DAGGER_SCENE, BOMB_SCENE, SAW_BLADE_SCENE;
	[Export] Node2D TRAJECTORY_POINT_CONTAINER, PROJECTILE_CONTAINER, DAGGER_SPAWN_POINTS_CONTAINER;
	[Export] EnemiesManager ENEMIES_MANAGER;
	[Export] Node2D[] DAGGER_SPAWN_POINTS;
	[Export] AnimationPlayer ANIMATION_PLAYER;
	[Export] AnimationTree ANIMATION_TREE;
	[Export] Sprite2D SPRITE;
	enum SurfaceType { NONE, FLOOR, WALL_LEFT, WALL_RIGHT, CEILING }
	float GRAVITY = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	[Export] int MAX_WHIRLWIND_CHARGE = 1, MAX_DASH_CHARGE = 1;
	[Export] bool affectByGravity = true, isSliding = false;
	bool isDashing = false;
	[Export]
	bool IsDashing
	{
		get { return isDashing; }
		set
		{
			if (isDashing && !value) { velocity = velocity.Normalized() * JUMP_FORCE / 2f; }
			isDashing = value;
		}
	}
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
	int currentWhirlwindCharge, currentDashCharge;
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
				ANIMATION_TREE.Set("parameters/Attack/ThrowThing/blend_position", Vector2.Zero);
				return;
			}

			currentWhirlwindCharge = MAX_WHIRLWIND_CHARGE; currentDashCharge = MAX_DASH_CHARGE;
			ANIMATION_TREE.Set("parameters/conditions/isOnSurface", true);
			ANIMATION_TREE.Set("parameters/conditions/isFloating", false);
			CurrentStamina = 3;
			if (c_collision != null) velocity = -c_collision.GetNormal() * 120;
			surfaceCurrentlyInContact = value;
			affectByGravity = false;
			ANIMATION_TREE.Set("parameters/Attack/ThrowThing/blend_position", velocity);
		}
	}
	Vector2 velocity
	{
		get { return Velocity; }
		set
		{
			Velocity = value;
			if (IsDashing) return;
			if (Mathf.Abs(value.X) < 100) { isSliding = false; }
			if (value.X != 0) SPRITE.Scale = new(value.X > 0 ? -1 : 1, SPRITE.Scale.Y);
			ANIMATION_TREE.Set("parameters/FloatMidAir/blend_position", Velocity.Normalized());
			ANIMATION_TREE.Set("parameters/GrabSurface/blend_position", Velocity.Normalized());
		}
	}
	Vector2 c_direction, c_CalculatingVector; KinematicCollision2D c_collision; Node2D c_collisionNode;
	public override void _Ready()
	{
		SawBlade.PLAYER = this;
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
				for (int i = 0; i < AMOUNT_OF_TRAJECTORY_POINT; ++i) { TRAJECTORY_POINTS[i].Update(Position, canSlide, c_direction, i * .05f, FRICTION_FACTOR); }
				return;
			}
			else
			{
				TRAJECTORY_POINT_CONTAINER.Visible = false;
				if (Input.IsActionJustReleased("ui_jump"))
				{
					Jump();
					ANIMATION_TREE.Set("parameters/conditions/isJumping", true);
					return;
				}
			}
		}
		else
		{
			if (Input.IsActionJustPressed("ui_whirlwindAttack") && currentWhirlwindCharge > 0)
			{
				currentWhirlwindCharge--;
				WhirlwindAttack();
				ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
				ANIMATION_TREE.Set("parameters/Attack/conditions/whirlwindAttack", true);
				return;
			}
		}
		if (Input.IsActionJustPressed("ui_throwSmallDagger") && SMALL_DAGGER_BAR.Value > 0)
		{
			SMALL_DAGGER_BAR.Value--;
			ThrowDagger(true);
			ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
			ANIMATION_TREE.Set("parameters/Attack/conditions/throwThing", true);
			return;
		}
		if (Input.IsActionJustPressed("ui_throwBigDagger") && BIG_DAGGER_BAR.Value > 0)
		{
			BIG_DAGGER_BAR.Value--;
			ThrowDagger(false);
			ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
			ANIMATION_TREE.Set("parameters/Attack/conditions/throwThing", true);
			return;
		}
		if (Input.IsActionJustPressed("ui_throwBomb") && SPIKE_STAR_BAR.Value > 0)
		{
			SPIKE_STAR_BAR.Value--;
			ThrowStarSpike();
			ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
			ANIMATION_TREE.Set("parameters/Attack/conditions/throwThing", true);
			return;
		}
		if (Input.IsActionJustPressed("ui_throwSawBlade") && SAW_BLADE_BAR.Value > 0)
		{
			SAW_BLADE_BAR.Value--;
			ThrowSawBlade();
			ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
			ANIMATION_TREE.Set("parameters/Attack/conditions/throwThing", true);
			return;
		}
		if (Input.IsActionJustPressed("ui_dash") && currentDashCharge > 0)
		{
			currentDashCharge--;
			Dash();
			ANIMATION_TREE.Set("parameters/conditions/isAttacking", true);
			ANIMATION_TREE.Set("parameters/Attack/conditions/dash", true);
		}
	}
	void HandlePhysics()
	{
		if (!isSliding && !IsDashing) velocity = new(velocity.X, affectByGravity ? velocity.Y + GRAVITY * deltaF : velocity.Y);
		else if(!isDashing) velocity = new(velocity.X * FRICTION_FACTOR, 0);
		c_collision = MoveAndCollide(velocity * deltaF);
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
	void Jump()
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
	}
	void WhirlwindAttack()
	{
		velocity = new(velocity.X, -Mathf.Abs(velocity.Y));
	}
	Dagger c_daggerInstance;
	void ThrowDagger(bool isThrowingSmallDagger)
	{
		DAGGER_SPAWN_POINTS_CONTAINER.LookAt(GetGlobalMousePosition());
		if (isThrowingSmallDagger)
		{
			for (int i = 0; i < DAGGER_SPAWN_POINTS.Length; ++i)
			{
				c_daggerInstance = SMALL_DAGGER_SCENE.Instantiate<Dagger>();
				PROJECTILE_CONTAINER.AddChild(c_daggerInstance);
				c_daggerInstance.Transform = new(DAGGER_SPAWN_POINTS[i].GlobalRotation, DAGGER_SPAWN_POINTS[i].GlobalPosition);
				c_daggerInstance.Intialize(new(Mathf.Cos(c_daggerInstance.GlobalRotation), Mathf.Sin(c_daggerInstance.GlobalRotation)));
			}
			return;
		}
		c_daggerInstance = BIG_DAGGER_SCENE.Instantiate<Dagger>();
		PROJECTILE_CONTAINER.AddChild(c_daggerInstance);
		c_daggerInstance.Transform = new(DAGGER_SPAWN_POINTS[0].GlobalRotation, DAGGER_SPAWN_POINTS[0].GlobalPosition);
		c_daggerInstance.Intialize(new(Mathf.Cos(c_daggerInstance.GlobalRotation), Mathf.Sin(c_daggerInstance.GlobalRotation)));
	}
	StarSpike c_starSpikeInstance;
	void ThrowStarSpike()
	{
		DAGGER_SPAWN_POINTS_CONTAINER.LookAt(GetGlobalMousePosition());
		c_starSpikeInstance = BOMB_SCENE.Instantiate<StarSpike>();
		PROJECTILE_CONTAINER.AddChild(c_starSpikeInstance);
		c_starSpikeInstance.GlobalPosition = DAGGER_SPAWN_POINTS[0].GlobalPosition;
		c_starSpikeInstance.Intialize(new(Mathf.Cos(DAGGER_SPAWN_POINTS[0].GlobalRotation), Mathf.Sin(DAGGER_SPAWN_POINTS[0].GlobalRotation)));
	}
	SawBlade c_sawBladeInstance;
	void ThrowSawBlade()
	{
		DAGGER_SPAWN_POINTS_CONTAINER.LookAt(GetGlobalMousePosition());
		c_sawBladeInstance = SAW_BLADE_SCENE.Instantiate<SawBlade>();
		PROJECTILE_CONTAINER.AddChild(c_sawBladeInstance);
		c_sawBladeInstance.GlobalPosition = DAGGER_SPAWN_POINTS[0].GlobalPosition;
		c_sawBladeInstance.Intialize(new(Mathf.Cos(DAGGER_SPAWN_POINTS[0].GlobalRotation), Mathf.Sin(DAGGER_SPAWN_POINTS[0].GlobalRotation)), Velocity);
	}
	void Dash()
	{
		c_direction = GetGlobalMousePosition() - GlobalPosition;
		if (SurfaceCurrentlyInContact == SurfaceType.CEILING) { c_direction = new(c_direction.X, Mathf.Max(0, c_direction.Y)); }
		else if (SurfaceCurrentlyInContact == SurfaceType.FLOOR)
		{
			c_CalculatingVector = c_direction * c_direction;
			isSliding = c_CalculatingVector.X / (c_CalculatingVector.X + c_CalculatingVector.Y) >= 0.933012701894f;
			if (isSliding) c_direction = new(c_direction.X, 0);
		}
		else if (SurfaceCurrentlyInContact == SurfaceType.WALL_LEFT) { c_direction = new(Mathf.Max(0, c_direction.X), c_direction.Y); }
		else if (SurfaceCurrentlyInContact == SurfaceType.WALL_RIGHT) { c_direction = new(Mathf.Min(0, c_direction.X), c_direction.Y); }
		velocity = c_direction.Normalized() * DASH_FORCE;
	}
	public void RestoreWeaponCharge(int amount) {
		if(SPIKE_STAR_BAR.Value != SPIKE_STAR_BAR.MaxValue) { SPIKE_STAR_BAR.Value += amount; }
		else if(BIG_DAGGER_BAR.Value != BIG_DAGGER_BAR.MaxValue) { BIG_DAGGER_BAR.Value += amount; }
		else if(SAW_BLADE_BAR.Value != SAW_BLADE_BAR.MaxValue) { SAW_BLADE_BAR.Value += amount; }
		else { SMALL_DAGGER_BAR.Value += amount; }
	}
}
