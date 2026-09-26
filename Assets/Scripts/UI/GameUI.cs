using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private GameObject[] hearts;

    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        SetLives(3);
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void SetLives(int lives)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(i < lives);
        }
    }

    public void ShowWin()
    {
        winPanel.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        winPanel.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}