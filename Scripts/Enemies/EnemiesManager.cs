using Godot;
public partial class EnemiesManager : Node
{
    [Export] public Enemy[] enemies;
    [Export] PackedScene LASER_MENACE_SCENE, ROBOT_DUELIST_SCENE, SPEAR_MAN_SCENE, SPIKY_BALL_SCENE, SWORD_MAN_SCENE;
    int enemyRemaining;
    public int EnemyRemaining
    {
        get { return enemyRemaining; }
        set
        {
            enemyRemaining = value;
            if (enemyRemaining == 0)
            {
                SpawnNextWave();
            }
        }
    }
    int MAX_ENEMY_BUDGET = 3, currentEnemyBudget = 3;
    public override void _Ready()
    {
        Enemy.ENEMIES_MANAGER = this;
    }
    void SpawnNextWave()
    {
        MAX_ENEMY_BUDGET += 3; currentEnemyBudget = MAX_ENEMY_BUDGET;
    }
}
