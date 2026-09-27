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
        UnlockCursor();
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        winPanel.SetActive(false);
        UnlockCursor();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}