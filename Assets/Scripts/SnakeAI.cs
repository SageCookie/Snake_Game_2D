using System.Collections.Generic;
using UnityEngine;

public class SnakeAI : MonoBehaviour
{
    private Snake snake;
    private GameManager gm;
    private int gridStep = 1;

    void Start()
    {
        snake = GetComponent<Snake>();
        gm = FindFirstObjectByType<GameManager>();
        InvokeRepeating(nameof(UpdateAIMove), 0.1f, snake.moveRate);
    }

    void UpdateAIMove()
    {
        if (!snake.enabled) return;
        if (snake == null || gm == null) return;

        GameObject food = GameObject.FindGameObjectWithTag("Food");
        if (food == null) return;

        Vector2 start = RoundVector(snake.transform.position);
        Vector2 goal = RoundVector(food.transform.position);

        // Run A* pathfinding
        List<Vector2> path = FindPath(start, goal);
        if (path == null || path.Count < 2) return;

        Vector2 nextCell = path[1]; // next cell to move into
        Vector2 moveDir = (nextCell - start).normalized;

        // Prevent reversing into itself
        if (moveDir == -snake.GetDirection()) return;

        snake.SetDirection(moveDir);
    }

    Vector2 RoundVector(Vector3 v)
    {
        return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    }

    // -------------------------------
    //        A* Pathfinding
    // -------------------------------
    List<Vector2> FindPath(Vector2 start, Vector2 goal)
    {
        Bounds b = gm.gridArea.bounds;

        var open = new List<Node>();
        var closed = new HashSet<Vector2>();
        var cameFrom = new Dictionary<Vector2, Vector2>();
        var gScore = new Dictionary<Vector2, float>();
        var fScore = new Dictionary<Vector2, float>();

        open.Add(new Node(start, 0, Heuristic(start, goal)));
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        // Record snake body positions to avoid
        HashSet<Vector2> blocked = new HashSet<Vector2>();
        foreach (Transform seg in snake.GetSegments())
            blocked.Add(RoundVector(seg.position));

        while (open.Count > 0)
        {
            // Get lowest fScore node
            open.Sort((a, b2) => a.f.CompareTo(b2.f));
            Node current = open[0];
            open.RemoveAt(0);

            if (current.pos == goal)
                return ReconstructPath(cameFrom, current.pos);

            closed.Add(current.pos);

            foreach (Vector2 neighbor in GetNeighbors(current.pos, b))
            {
                if (blocked.Contains(neighbor) || closed.Contains(neighbor))
                    continue;

                float tentativeG = gScore[current.pos] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current.pos;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!open.Exists(n => n.pos == neighbor))
                        open.Add(new Node(neighbor, gScore[neighbor], fScore[neighbor]));
                }
            }
        }

        return null; // no path
    }

    float Heuristic(Vector2 a, Vector2 b)
    {
        // Manhattan distance works best for grid movement
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 current)
    {
        List<Vector2> totalPath = new List<Vector2> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    List<Vector2> GetNeighbors(Vector2 node, Bounds b)
    {
        List<Vector2> neighbors = new List<Vector2>();
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (var d in dirs)
        {
            Vector2 newPos = node + d * gridStep;
            if (b.Contains(new Vector3(newPos.x, newPos.y, 0)))
                neighbors.Add(newPos);
        }

        return neighbors;
    }

    private class Node
    {
        public Vector2 pos;
        public float g, f;
        public Node(Vector2 pos, float g, float f)
        {
            this.pos = pos;
            this.g = g;
            this.f = f;
        }
    }
}