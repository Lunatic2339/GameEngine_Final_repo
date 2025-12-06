using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public GameObject uiPanel; // 게임오버 패널 (텍스트, 버튼 포함)

    void Start()
    {
        // 시작할 땐 꺼둠
        uiPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        uiPanel.SetActive(true);
        // Time.timeScale = 0f; // 게임 시간을 멈추고 싶다면 주석 해제
    }

    // 재시작 버튼에 연결할 함수
    public void OnRestartButton()
    {
        // Time.timeScale = 1f; // 시간 다시 흐르게
        
        if (GameManager.instance != null)
        {
            GameManager.instance.RestartGame();
        }
        else
        {
            // 매니저 없으면 그냥 씬 로드
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}