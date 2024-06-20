using Godot;

public partial class LaserMenace : Enemy {
	bool hasReachedCeiling;
	SurfaceType SurfaceCurrentlyInContact
	{
		get { return surfaceCurrentlyInContact; }
		set { 
			surfaceCurrentlyInContact = value; 
			if(surfaceCurrentlyInContact == SurfaceType.CEILING) { hasReachedCeiling = true; } 
		}
	}
    public override void _Process(double delta) {
		if(hasReachedCeiling) return;
    }
}
