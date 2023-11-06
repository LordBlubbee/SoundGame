using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    private Enemy[] enemies;

    private void Start()
    {
        enemies = FindObjectsOfType<Enemy>();
    }

    public void SpawnEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.SpawnEnemy();
        }
    }
}
