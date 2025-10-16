using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    [Header("Movement")]
    public float moveStep = 1f;            // grid step (typically 1)
    public float moveRate = 0.15f;         // time between steps
    private float moveTimer;

    private Vector2 direction = Vector2.right;
    private bool canChangeDirection = true;

    [Header("Parts")]
    public Transform segmentPrefab;        // assign Tail prefab (Transform)
    private List<Transform> segments = new List<Transform>();

    void Start()
    {
        // Start with only the head in the segments list
        segments.Clear();
        segments.Add(transform);
        // Ensure head is exactly on integer grid
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0f);
    }

    void Update()
    {
        // Read input but only allow one change per move tick
        if (canChangeDirection)
        {
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

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveRate)
        {
            moveTimer = 0f;
            Move();
            canChangeDirection = true; // unlock inputs after movement
        }
    }

    void Move()
    {
        // Move tail segments from back to front
        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].position = segments[i - 1].position;
        }

        // Move head in exact integer grid steps
        Vector3 targetPos = new Vector3(
            Mathf.Round(transform.position.x) + direction.x * moveStep,
            Mathf.Round(transform.position.y) + direction.y * moveStep,
            0f
        );

        transform.position = targetPos;

        // 1) Check wall bounds (using GameManager's gridArea)
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("GameManager not found in scene.");
            return;
        }

        if (!gm.gridArea.bounds.Contains(transform.position))
        {
            gm.PlayerDied();
            return;
        }

        // 2) Check self-collision by exact position equality
        for (int i = 1; i < segments.Count; i++)
        {
            // exact positional equality is correct because we keep grid-aligned positions
            if (transform.position == segments[i].position)
            {
                gm.PlayerDied();
                return;
            }
        }

        // 3) Check for food at the same exact integer position(s)
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject f in foods)
        {
            Vector3 fPos = new Vector3(Mathf.Round(f.transform.position.x), Mathf.Round(f.transform.position.y), 0f);
            if (transform.position == fPos)
            {
                // Eat it
                Grow();
                gm.IncrementScore();
                Destroy(f);
                gm.SpawnFood();
                break; // only one food should exist, but break defensively
            }
        }
    }

    public void Grow()
    {
        // Instantiate a new tail segment at the last segment position
        if (segmentPrefab == null)
        {
            Debug.LogError("segmentPrefab (tail) not assigned on Snake.");
            return;
        }

        Transform newSeg = Instantiate(segmentPrefab);
        newSeg.position = segments[segments.Count - 1].position;
        segments.Add(newSeg);

        // Make sure the new segment uses the same tag as the head so inspector tagging matches
        newSeg.gameObject.tag = gameObject.tag;
    }

    // Public helper to reset the snake to the start state (used if needed)
    public void ResetState()
    {
        // Destroy all extra segments
        for (int i = 1; i < segments.Count; i++)
        {
            if (segments[i] != null)
                Destroy(segments[i].gameObject);
        }
        segments.Clear();
        segments.Add(transform);
        transform.position = Vector3.zero;
        direction = Vector2.right;
    }
    public List<Transform> GetSegments()
    {
        return segments;
    }

}
