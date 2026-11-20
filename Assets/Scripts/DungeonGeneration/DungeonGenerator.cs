using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private Room[] roomPrefabs;
    [SerializeField] private float roomSize = 10f;
    [SerializeField] private int roomCount = 10;

    // N, E, S, W
    private static readonly Vector2Int[] Dirs =
    {
        new(0, 1),
        new(1, 0),
        new(0, -1),
        new(-1, 0)
    };

    private static readonly int[] Opposite = { 2, 3, 0, 1 };

    private Dictionary<Vector2Int, Room> rooms = new();
    private System.Random rng;

    public Room StartRoom { get; private set; }
    public Room EndRoom { get; private set; }

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Clear();

        StartRoom = GetComponent<Room>();

        if (StartRoom == null || roomPrefabs.Length == 0)
            return;

        rng = new System.Random();

        StartRoom.GridPos = Vector2Int.zero;
        StartRoom.Distance = 0;
        StartRoom.name = "FirstRoom";
        StartRoom.CloseAllDoors();

        rooms.Add(Vector2Int.zero, StartRoom);

        List<Vector2Int> layout = GenerateLayout();

        SpawnRooms(layout);
        ConnectDoors();
        CalculateDistances();
    }

    private List<Vector2Int> GenerateLayout()
    {
        List<Vector2Int> cells = new() { Vector2Int.zero };
        HashSet<Vector2Int> occupied = new() { Vector2Int.zero };

        int attempts = 10000;

        while (cells.Count < roomCount && attempts-- > 0)
        {
            Vector2Int cell = cells[rng.Next(cells.Count)];
            Vector2Int next = cell + Dirs[rng.Next(4)];

            if (occupied.Contains(next))
                continue;

            int neighbors = 0;

            foreach (Vector2Int dir in Dirs)
                if (occupied.Contains(next + dir))
                    neighbors++;

            if (neighbors > 1)
                continue;

            occupied.Add(next);
            cells.Add(next);
        }

        return cells;
    }

    private void SpawnRooms(List<Vector2Int> cells)
    {
        Vector3 startPosition = StartRoom.transform.position;

        foreach (Vector2Int cell in cells)
        {
            if (cell == Vector2Int.zero)
                continue;

            Room prefab = roomPrefabs[rng.Next(roomPrefabs.Length)];

            Vector3 position = startPosition + new Vector3(
                cell.x * roomSize,
                0,
                cell.y * roomSize
            );

            Room room = Instantiate(prefab, position, Quaternion.identity, transform);

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
            Room room = pair.Value;

            for (int i = 0; i < 4; i++)
            {
                Vector2Int next = pair.Key + Dirs[i];
                bool connected = rooms.TryGetValue(next, out Room neighbor);

                room.SetDoor(i, connected);

                if (connected)
                    neighbor.SetDoor(Opposite[i], true);
            }
        }
    }

    private void CalculateDistances()
    {
        Queue<Room> queue = new();
        HashSet<Vector2Int> visited = new() { StartRoom.GridPos };

        StartRoom.Distance = 0;
        EndRoom = StartRoom;
        queue.Enqueue(StartRoom);

        while (queue.Count > 0)
        {
            Room current = queue.Dequeue();

            if (current.Distance > EndRoom.Distance)
                EndRoom = current;

            foreach (Vector2Int dir in Dirs)
            {
                Vector2Int next = current.GridPos + dir;

                if (visited.Contains(next) || !rooms.TryGetValue(next, out Room neighbor))
                    continue;

                visited.Add(next);
                neighbor.Distance = current.Distance + 1;
                queue.Enqueue(neighbor);
            }
        }
    }

    private void Clear()
    {
        foreach (Room room in rooms.Values)
        {
            if (room != null && room != StartRoom)
                Destroy(room.gameObject);
        }

        rooms.Clear();
        StartRoom = null;
        EndRoom = null;
    }
}