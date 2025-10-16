using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GameObject foodPrefab;          // assign Food prefab
    public BoxCollider2D gridArea;         // assign GridArea BoxCollider2D
    public TextMeshProUGUI scoreText;      // assign Score TMP Text
    public GameObject gameOverUI;          // assign GameOver UI panel (optional)

    private int score = 0;

    void Start()
    {
        Time.timeScale = 1f;
        if (gameOverUI != null) gameOverUI.SetActive(false);
        UpdateScore(0);
        SpawnFood();
    }

    // Ensure only one food exists and spawn on exact integer grid coords
    public void SpawnFood()
    {
        // Remove existing food (if any) to avoid multiples
        GameObject existing = GameObject.FindGameObjectWithTag("Food");
        if (existing != null) Destroy(existing);

        Bounds bounds = gridArea.bounds;

        int x = Mathf.RoundToInt(Random.Range(bounds.min.x + 1, bounds.max.x - 1));
        int y = Mathf.RoundToInt(Random.Range(bounds.min.y + 1, bounds.max.y - 1));
        Vector3 spawnPos = new Vector3(x, y, 0f);

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
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    public void PlayerDied()
    {
        if (gameOverUI != null) gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
