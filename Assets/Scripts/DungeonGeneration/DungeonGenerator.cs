
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Room[] roomPrefabs;

    [Header("Generation")]
    [SerializeField] private float roomSize = 40f;
    [SerializeField] private int roomCount = 12;
    [SerializeField] private int seed = 0;

    // N, E, S, W
    static readonly Vector2Int[] Dirs =
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

    static readonly int[] Opposite = { 2, 3, 0, 1 };

    private readonly Dictionary<Vector2Int, Room> rooms = new();

    public Room StartRoom { get; private set; }
    public Room EndRoom { get; private set; }

    private System.Random rng;

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Clear();

        StartRoom = GetComponent<Room>();

        if (StartRoom == null)
        {
            return;
        }

        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            return;
        }

        rng = seed == 0
            ? new System.Random()
            : new System.Random(seed);

        StartRoom.GridPos = Vector2Int.zero;
        StartRoom.Distance = 0;
        StartRoom.name = "FirstRoom";

        StartRoom.CloseAllDoors();

        rooms.Add(Vector2Int.zero, StartRoom);

        List<Vector2Int> layout = BuildLayout();

        SpawnRooms(layout);

        ConnectDoors();

        AssignDistances();
    }

    private List<Vector2Int> BuildLayout()
    {
        List<Vector2Int> cells = new()
        {
            Vector2Int.zero
        };

        HashSet<Vector2Int> occupied = new()
        {
            Vector2Int.zero
        };

        int safety = 10000;

        while (cells.Count < roomCount && safety-- > 0)
        {
            Vector2Int from = cells[rng.Next(cells.Count)];
            Vector2Int next = from + Dirs[rng.Next(4)];

            if (occupied.Contains(next))
                continue;

            if (CountNeighbors(next, occupied) > 1)
                continue;

            occupied.Add(next);
            cells.Add(next);
        }

        return cells;
    }

    private int CountNeighbors(
        Vector2Int cell,
        HashSet<Vector2Int> occupied)
    {
        int count = 0;

        foreach (Vector2Int dir in Dirs)
        {
            if (occupied.Contains(cell + dir))
                count++;
        }

        return count;
    }

    private void SpawnRooms(List<Vector2Int> cells)
    {
        Vector3 startPosition = StartRoom.transform.position;

        foreach (Vector2Int cell in cells)
        {
            if (cell == Vector2Int.zero)
                continue;

            Room randomPrefab = roomPrefabs[
                rng.Next(roomPrefabs.Length)
            ];

            Vector3 position = startPosition + new Vector3(
                cell.x * roomSize,
                0,
                cell.y * roomSize
            );

            Room room = Instantiate(
                randomPrefab,
                position,
                Quaternion.identity,
                transform
            );

            room.GridPos = cell;
            room.Distance = 0;
            room.name = $"Room {cell.x},{cell.y}";

            room.CloseAllDoors();

            rooms.Add(cell, room);
        }
    }
    private void ConnectDoors()
    {
        foreach (var pair in rooms)
        {
            Vector2Int position = pair.Key;
            Room currentRoom = pair.Value;

            for (int dir = 0; dir < 4; dir++)
            {
                Vector2Int neighborPosition = position + Dirs[dir];

                if (!rooms.TryGetValue(neighborPosition, out Room neighbor))
                {
                    currentRoom.SetDoor(dir, false);
                    continue;
                }

                currentRoom.SetDoor(dir, true);
                neighbor.SetDoor(Opposite[dir], true);
            }
        }
    }
    private void AssignDistances()
    {
        Queue<Room> queue = new();
        HashSet<Vector2Int> visited = new()
        {
            StartRoom.GridPos
        };

        StartRoom.Distance = 0;
        queue.Enqueue(StartRoom);

        EndRoom = StartRoom;

        while (queue.Count > 0)
        {
            Room current = queue.Dequeue();

            if (current.Distance > EndRoom.Distance)
                EndRoom = current;

            foreach (Vector2Int dir in Dirs)
            {
                Vector2Int nextPosition = current.GridPos + dir;

                if (visited.Contains(nextPosition))
                    continue;

                if (!rooms.TryGetValue(nextPosition, out Room neighbor))
                    continue;

                visited.Add(nextPosition);
                neighbor.Distance = current.Distance + 1;

                queue.Enqueue(neighbor);
            }
        }
    }

    private void Clear()
    {
        foreach (var pair in rooms)
        {
            Room room = pair.Value;

            if (room != null && room.gameObject != gameObject)
                Destroy(room.gameObject);
        }

        rooms.Clear();

        StartRoom = null;
        EndRoom = null;
    }
}
