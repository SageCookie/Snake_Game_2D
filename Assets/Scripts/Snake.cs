using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    [Header("Movement")]
    public float moveStep = 1f;            // Grid step size (usually 1)
    public float moveRate = 0.15f;         // Time between movement steps
    private float moveTimer;

    private Vector2 direction = Vector2.right;
    private bool canChangeDirection = true;

    [Header("Parts")]
    public Transform segmentPrefab;        // Tail prefab reference
    private List<Transform> segments = new List<Transform>();

    void Start()
    {
        // Initialize with head only
        segments.Clear();
        segments.Add(transform);

        // Snap to grid
        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            0f
        );
    }

    void Update()
    {
        // Handle player input only if AI is not active
        SnakeAI ai = GetComponent<SnakeAI>();
        if (ai == null || !ai.enabled)
        {
            HandleInput();
        }

        // Move the snake periodically
        moveTimer += Time.deltaTime;
        if (moveTimer >= moveRate)
        {
            moveTimer = 0f;
            Move();
            canChangeDirection = true;
        }
    }

    void HandleInput()
    {
        if (!canChangeDirection) return;

        if (Input.GetKeyDown(KeyCode.W) && direction != Vector2.down)
        {
            direction = Vector2.up;
            canChangeDirection = false;
        }
        else if (Input.GetKeyDown(KeyCode.S) && direction != Vector2.up)
        {
            direction = Vector2.down;
            canChangeDirection = false;
        }
        else if (Input.GetKeyDown(KeyCode.A) && direction != Vector2.right)
        {
            direction = Vector2.left;
            canChangeDirection = false;
        }
        else if (Input.GetKeyDown(KeyCode.D) && direction != Vector2.left)
        {
            direction = Vector2.right;
            canChangeDirection = false;
        }
    }

    void Move()
    {
        // Move tail segments (from end to front)
        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].position = segments[i - 1].position;
        }

        // Calculate new head position on grid
        Vector3 targetPos = new Vector3(
            Mathf.Round(transform.position.x) + direction.x * moveStep,
            Mathf.Round(transform.position.y) + direction.y * moveStep,
            0f
        );

        transform.position = targetPos;

        // Fetch GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("GameManager not found in scene!");
            return;
        }

        // 1️⃣ Check wall collision
        if (!gm.gridArea.bounds.Contains(transform.position))
        {
            gm.PlayerDied();
            return;
        }

        // 2️⃣ Check self-collision
        for (int i = 1; i < segments.Count; i++)
        {
            if (transform.position == segments[i].position)
            {
                gm.PlayerDied();
                return;
            }
        }

        // 3️⃣ Check for food
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject f in foods)
        {
            Vector3 fPos = new Vector3(
                Mathf.Round(f.transform.position.x),
                Mathf.Round(f.transform.position.y),
                0f
            );

            if (transform.position == fPos)
            {
                Grow();
                gm.IncrementScore();
                Destroy(f);
                gm.SpawnFood();
                break;
            }
        }
    }

    public void Grow()
    {
        if (segmentPrefab == null)
        {
            Debug.LogError("Segment prefab not assigned on Snake!");
            return;
        }

        Transform newSeg = Instantiate(segmentPrefab);
        newSeg.position = segments[segments.Count - 1].position;
        newSeg.gameObject.tag = gameObject.tag;
        segments.Add(newSeg);
    }

    public void ResetState()
    {
        // Destroy tail parts
        for (int i = 1; i < segments.Count; i++)
        {
            if (segments[i] != null)
                Destroy(segments[i].gameObject);
        }

        segments.Clear();
        segments.Add(transform);

        // Reset position and direction
        transform.position = Vector3.zero;
        direction = Vector2.right;
        moveTimer = 0f;
        canChangeDirection = true;
    }

    // Public accessors for AI & GameManager
    public List<Transform> GetSegments()
    {
        return segments;
    }

    public Vector2 GetDirection()
    {
        return direction;
    }

    public void SetDirection(Vector2 newDir)
    {
        // Prevent reversing directly
        if ((newDir == Vector2.up && direction != Vector2.down) ||
            (newDir == Vector2.down && direction != Vector2.up) ||
            (newDir == Vector2.left && direction != Vector2.right) ||
            (newDir == Vector2.right && direction != Vector2.left))
        {
            direction = newDir;
            canChangeDirection = false;
        }
    }
}
