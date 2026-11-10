using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject enemy;

    public void Spawn()
    {
        Instantiate(enemy, transform.position, Quaternion.identity);
    }
}
