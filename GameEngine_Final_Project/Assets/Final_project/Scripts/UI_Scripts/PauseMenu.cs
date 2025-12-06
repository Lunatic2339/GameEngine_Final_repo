using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI 패널")]
    public GameObject pausePanel; // 아까 만든 검은색 패널 연결

    private bool isPaused = false; // 현재 멈췄는지 체크
    void Start()
    {
        // 1. 게임 시작 시 패널을 강제로 끕니다. (핵심!)
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // 2. 변수 초기화 (꺼진 상태로 시작)
        isPaused = false;

        // 3. 시간 정상화 (혹시라도 멈춘 상태로 씬이 로드되는 것 방지)
        Time.timeScale = 1f;
    }
    void Update()
    {
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame(); // 이미 멈췄으면 -> 게임 재개
            }
            else
            {
                PauseGame();  // 진행 중이면 -> 일시 정지
            }
        }
    }

    // --- 기능 함수들 ---

    public void ResumeGame()
    {
        pausePanel.SetActive(false); // 창 닫기
        Time.timeScale = 1f;         // 시간 다시 흐르게 (정상 속도)
        isPaused = false;
    }

    void PauseGame()
    {
        pausePanel.SetActive(true);  // 창 열기
        Time.timeScale = 0f;         // 시간 멈춤 (물리, 애니메이션 정지)
        isPaused = true;
    }

    public void RestartFromCheckpoint()
    {
        Time.timeScale = 1f; // (중요) 시간 다시 흐르게 하고 이동해야 함!
        
        // GameManager에 있는 재시작 기능 활용
        if (GameManager.instance != null)
        {
            GameManager.instance.RestartGame();
        }
        else
        {
            // 매니저 없으면 그냥 씬 재로딩
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // (중요) 시간 정상화 필수!
        SceneManager.LoadScene("MainMenu"); // 메인 메뉴 씬 이름 정확히!
    }

    public void OpenSettings()
    {
        Debug.Log("설정 창은 아직 준비 중입니다!");
        // 나중에 여기에 설정 패널을 켜는 코드를 넣으면 됨
    }
}