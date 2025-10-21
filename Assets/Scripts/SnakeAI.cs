using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Snake))]
public class SnakeAI : MonoBehaviour
{
    private Snake snake;
    private GameManager gm;

    void Start()
    {
        snake = GetComponent<Snake>();
        gm = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (gm == null || gm.gridArea == null) return;

        // Find food position
        GameObject food = GameObject.FindGameObjectWithTag("Food");
        if (food == null) return;

        Vector2Int headPos = Vector2Int.RoundToInt(snake.transform.position);
        Vector2Int foodPos = Vector2Int.RoundToInt(food.transform.position);

        List<Vector2Int> bodyPositions = new List<Vector2Int>();
        foreach (var seg in snake.GetSegments())
            bodyPositions.Add(Vector2Int.RoundToInt(seg.position));

        // 1. Try to find a path to food
        List<Vector2Int> pathToFood = FindPath(headPos, foodPos, bodyPositions);

        // 2. Check if that path is safe
        if (pathToFood != null && IsPathSafe(pathToFood, bodyPositions))
        {
            // Move toward first step in path
            if (pathToFood.Count > 1)
            {
                Vector2Int nextPos = pathToFood[1];
                Vector2 dir = (nextPos - headPos);
                snake.SetDirection(dir);
            }
        }
        else
        {
            // 3. Fallback: follow tail or move safely
            Vector2Int tailPos = bodyPositions[bodyPositions.Count - 1];
            List<Vector2Int> pathToTail = FindPath(headPos, tailPos, bodyPositions);
            if (pathToTail != null && pathToTail.Count > 1)
            {
                Vector2Int nextPos = pathToTail[1];
                Vector2 dir = (nextPos - headPos);
                snake.SetDirection(dir);
            }
            else
            {
                // last resort: random safe move
                TryRandomSafeMove(headPos, bodyPositions);
            }
        }
    }

    // ------------------------------------------
    // PATHFINDING (BFS)
    // ------------------------------------------
    List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, List<Vector2Int> body)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        q.Enqueue(start);
        cameFrom[start] = start;

        Bounds b = gm.gridArea.bounds;
        Vector2Int min = Vector2Int.FloorToInt(b.min);
        Vector2Int max = Vector2Int.CeilToInt(b.max);

        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();
            if (cur == goal) break;

            foreach (Vector2Int dir in new Vector2Int[] {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int nxt = cur + dir;
                if (nxt.x < min.x || nxt.x > max.x || nxt.y < min.y || nxt.y > max.y)
                    continue;
                if (body.Contains(nxt)) continue;
                if (cameFrom.ContainsKey(nxt)) continue;

                cameFrom[nxt] = cur;
                q.Enqueue(nxt);
            }
        }

        if (!cameFrom.ContainsKey(goal)) return null;

        // Reconstruct path
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int temp = goal;
        while (temp != start)
        {
            path.Insert(0, temp);
            temp = cameFrom[temp];
        }
        path.Insert(0, start);
        return path;
    }

    // ------------------------------------------
    // SAFE PATH CHECK
    // ------------------------------------------
    bool IsPathSafe(List<Vector2Int> pathToFood, List<Vector2Int> bodyPositions)
    {
        if (pathToFood == null || pathToFood.Count == 0) return false;

        List<Vector2Int> simulatedBody = new List<Vector2Int>(bodyPositions);
        Vector2Int newHead = pathToFood[pathToFood.Count - 1];
        Vector2Int tail = simulatedBody[simulatedBody.Count - 1];

        // Simulate snake moving along path (tail follows head)
        for (int i = 1; i < pathToFood.Count; i++)
        {
            simulatedBody.Insert(0, pathToFood[i]); // new head
            simulatedBody.RemoveAt(simulatedBody.Count - 1); // remove tail
        }

        // After simulation, check if new head can reach tail
        List<Vector2Int> pathToTail = FindPath(newHead, tail, simulatedBody);
        return pathToTail != null;
    }

    // ------------------------------------------
    // RANDOM SAFE MOVE (FALLBACK)
    // ------------------------------------------
    void TryRandomSafeMove(Vector2Int head, List<Vector2Int> body)
    {
        List<Vector2Int> dirs = new List<Vector2Int>
        { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Shuffle(dirs);

        Bounds b = gm.gridArea.bounds;
        Vector2Int min = Vector2Int.FloorToInt(b.min);
        Vector2Int max = Vector2Int.CeilToInt(b.max);

        foreach (Vector2Int d in dirs)
        {
            Vector2Int nxt = head + d;
            if (nxt.x < min.x || nxt.x > max.x || nxt.y < min.y || nxt.y > max.y)
                continue;
            if (body.Contains(nxt)) continue;
            snake.SetDirection(d);
            return;
        }
    }

    void Shuffle(List<Vector2Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
