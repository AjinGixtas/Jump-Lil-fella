using Godot;
public partial class ShieldBar : NinePatchRect {
    [Export] public ProgressBar SHIELD_BAR, SHIELD_DELAY_BAR, SHIELD_REGEN_BAR;
	[Export] Vector2 INITIAL_SCALE, SCALE_SCALE;
    public void Intialize(float MAX_SHIELD) {
        Size = SHIELD_BAR.Size = SHIELD_REGEN_BAR.Size = SHIELD_REGEN_BAR.Size = new(0, 0);
        if(MAX_SHIELD == 0) return;
        Size = SHIELD_BAR.Size = SHIELD_REGEN_BAR.Size = INITIAL_SCALE + SCALE_SCALE * (MAX_SHIELD - 1);
        SHIELD_DELAY_BAR.Size = new(SHIELD_BAR.Size.Y, SHIELD_BAR.Size.X);
        SHIELD_BAR.Value = SHIELD_REGEN_BAR.Value = MAX_SHIELD;
        SHIELD_BAR.MaxValue = SHIELD_REGEN_BAR.MaxValue = MAX_SHIELD;
        SHIELD_DELAY_BAR.Value = 3;
    }
}