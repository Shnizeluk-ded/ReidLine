
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private GameObject[] closedWalls; // N, E, S, W
    [SerializeField] private GameObject[] openWalls;   // N, E, S, W

    [SerializeField] private EnemySpawnPoint[] spawnPoints;

    public Vector2Int GridPos;
    public int Distance;

    public void SetDoor(int dir, bool open)
    {
        if (dir < 0 || dir >= 4) return;

        closedWalls[dir].SetActive(!open);
        openWalls[dir].SetActive(open);
    }

    public void CloseAllDoors()
    {
        for (int i = 0; i < 4; i++)
            SetDoor(i, false);
    }

    private void PlayerEnter()
    {
        foreach (EnemySpawnPoint sp in spawnPoints)
            sp.Spawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerEnter();
        }
    }
}
