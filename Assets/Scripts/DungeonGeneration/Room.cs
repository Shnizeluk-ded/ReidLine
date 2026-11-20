using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private GameObject[] closedWalls; // N, E, S, W
    [SerializeField] private GameObject[] openWalls;
    [SerializeField] private GameObject[] doors;

    [SerializeField] private EnemySpawnPoint[] spawnPoints;

    [SerializeField] private bool isStartRoom;

    public Vector2Int GridPos;
    public int Distance;

    private int aliveEnemies;

    private bool isCleared;

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
        if (isCleared) return;

        CloseDoors();

        aliveEnemies = 0;

        foreach (EnemySpawnPoint sp in spawnPoints)
        {
            sp.Spawn(this);
            aliveEnemies++;
        }

        if (aliveEnemies == 0)
            OpenDoors();
    }

    public void EnemyDied()
    {
        aliveEnemies--;

        if (aliveEnemies <= 0)
        {
            aliveEnemies = 0;
            isCleared = true;
            OpenDoors();
        }
    }

    private void CloseDoors()
    {
        foreach (var door in doors)
            door.SetActive(true);
    }

    private void OpenDoors()
    {
        foreach (var door in doors)
            door.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isStartRoom)
                PlayerEnter();
        }
    }
}