using Godot;

public partial class HealthBar : NinePatchRect
{
	[Export] public ProgressBar VALUE_BAR;
	[Export] Vector2 INITIAL_SCALE, SCALE_SCALE;
    public void Intialize(float MAX_HEALTH) {
		Size = VALUE_BAR.Size = INITIAL_SCALE + SCALE_SCALE * (MAX_HEALTH - 1);
		VALUE_BAR.MaxValue = VALUE_BAR.Value = MAX_HEALTH;
	}
}
