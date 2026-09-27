using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;

    [Header("References")]
    [SerializeField] private GameUI gameUI;

    private int currentLives;
    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        currentLives = startingLives;

        if (gameUI == null)
        {
            gameUI = FindFirstObjectByType<GameUI>();
        }

        if (gameUI != null)
        {
            gameUI.SetLives(currentLives);
        }
    }

    public bool PlayerDied()
    {
        if (gameEnded)
            return false;

        currentLives--;

        if (gameUI != null)
        {
            gameUI.SetLives(currentLives);
        }

        if (currentLives <= 0)
        {
            gameEnded = true;

            if (gameUI != null)
            {
                gameUI.ShowGameOver();
            }

            return false;
        }

        return true;
    }

    public void PlayerWon()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (gameUI != null)
        {
            gameUI.ShowWin();
        }
    }
}