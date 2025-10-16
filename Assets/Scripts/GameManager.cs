using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GameObject foodPrefab;
    public BoxCollider2D gridArea;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverUI;

    private int score = 0;

    void Start()
    {
        Time.timeScale = 1f;
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        UpdateScore(0);
        SpawnFood();
    }

    public void SpawnFood()
    {
        Bounds bounds = gridArea.bounds;
        Snake snake = FindAnyObjectByType<Snake>();

        Vector3 spawnPos;
        bool validPosition = false;
        int safety = 0;

        do
        {
            // Random grid-aligned position
            int x = Mathf.RoundToInt(Random.Range(bounds.min.x + 1, bounds.max.x - 1));
            int y = Mathf.RoundToInt(Random.Range(bounds.min.y + 1, bounds.max.y - 1));
            spawnPos = new Vector3(x, y, 0.0f);

            validPosition = true;

            // Make sure this spot is not occupied by the snake
            if (snake != null)
            {
                List<Transform> segments = snake.GetSegments() != null ? snake.GetSegments() : new List<Transform> { snake.transform };
                foreach (Transform segment in segments)
                {
                    if (segment.position == spawnPos)
                    {
                        validPosition = false;
                        break;
                    }
                }
            }

            safety++;
            if (safety > 100) break; // safety limit

        } while (!validPosition);

        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }

    public void IncrementScore()
    {
        score++;
        UpdateScore(score);
    }

    private void UpdateScore(int value)
    {
        score = value;
        scoreText.text = "Score: " + score;
    }

    public void PlayerDied()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
