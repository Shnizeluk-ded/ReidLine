
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject enemy;

    public void Spawn(Room room)
    {
        GameObject spawnedEnemy = Instantiate(
            enemy,
            transform.position,
            Quaternion.identity
        );

        Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();

        if (enemyScript != null)
            enemyScript.SetRoom(room);
    }
}
