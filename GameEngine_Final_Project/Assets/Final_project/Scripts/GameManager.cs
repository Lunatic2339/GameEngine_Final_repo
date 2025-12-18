using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ★ 싱글톤 (Singleton): 어디서든 GameManager.instance로 나를 부를 수 있게 함
    public static GameManager instance;

    [Header("플레이어 능력 정보 (저장됨)")]
    public bool hasDoubleJump = true; // 더블 점프 배웠니?
    public bool hasDash = false;       // 대쉬 배웠니? (나중에 확장)
    public int coinCount = 0;          // 코인 개수

    public Vector2 lastCheckPointPos; // ★ 마지막 체크포인트 위치 기억
    public bool isCheckpointActive = false; // 체크포인트를 한 번이라도 찍었는지 확인

    [Header("UI 연결 (Inspector에서 할당 X, 코드로 찾음)")]
    public GameOverUI gameOverUI; // 게임 오버 화면 스크립트

    void Awake()
    {
        // 1. 싱글톤 패턴 구현
        if (instance == null)
        {
            instance = this; // 내가 바로 그 유일한 매니저다!
            DontDestroyOnLoad(gameObject); // 씬이 바껴도 나를 파괴하지 마라!
        }
        else
        {
            // 만약 씬을 이동했는데 또 다른 GameManager가 있다면?
            // 짝퉁은 바로 제거한다. (중복 방지)
            Destroy(gameObject);
        }
    }
    // ★ [핵심] 데이터 저장하기 (체크포인트 닿을 때 호출)
    // -----------------------------------------------------------
    public void SaveGameData(int currentHealth)
    {
        // 1. 위치 저장 (Vector3는 바로 저장이 안 돼서 X, Y 따로 저장)
        PlayerPrefs.SetFloat("SaveX", lastCheckPointPos.x);
        PlayerPrefs.SetFloat("SaveY", lastCheckPointPos.y);

        // 2. 체력 저장
        PlayerPrefs.SetInt("SaveHealth", currentHealth);

        // 3. 현재 씬 이름 저장 (어느 맵이었는지)
        PlayerPrefs.SetString("SaveScene", SceneManager.GetActiveScene().name);

        // 4. 저장했다고 도장 쾅! (유효성 체크용)
        PlayerPrefs.SetInt("HasSaveData", 1);
        
        PlayerPrefs.Save(); // 디스크에 쓰기
        Debug.Log("게임 저장 완료!");
    }

    // -----------------------------------------------------------
    // ★ [핵심] 데이터 불러오기 (게임 시작/이어하기 때 호출)
    // -----------------------------------------------------------
    public void LoadGameData()
    {
        // 저장된 데이터가 없으면 실행 안 함
        if (!PlayerPrefs.HasKey("HasSaveData")) return;

        // 1. 위치 불러오기
        float x = PlayerPrefs.GetFloat("SaveX");
        float y = PlayerPrefs.GetFloat("SaveY");
        lastCheckPointPos = new Vector2(x, y);

        // 2. 씬 이름 불러오기 -> 해당 씬으로 이동
        string sceneName = PlayerPrefs.GetString("SaveScene");
        SceneManager.LoadScene(sceneName);
        
        // (참고: 체력이나 실제 플레이어 이동은 씬이 로딩된 후,
        // Player 스크립트의 Start()에서 GameManager의 정보를 가져가서 적용함)
    }


    // 능력 해금 함수
    public void UnlockAbility(string abilityName)
    {
        if (abilityName == "DoubleJump")
        {
            hasDoubleJump = true;
            Debug.Log("매니저: 더블 점프 능력이 영구 저장되었습니다.");
        }
        // else if (abilityName == "Dash") ...
    }

    // 게임 오버 시 호출
    public void GameOver()
    {
        Debug.Log("게임 오버! UI를 띄웁니다.");
        
        // 씬에 있는 UI를 찾아서 띄움
            
        if (gameOverUI != null) gameOverUI.ShowGameOver();
    }

    // 재시작 (버튼 연결용)
    public void RestartGame()
    {
        // 현재 씬을 다시 불러옴
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        // (참고: 씬이 로드되면 PlayerMovement가 Start에서 lastCheckPointPos로 이동할 것임)
    }

    // 체크포인트 도달 시 호출
    public void UpdateCheckpoint(Vector2 pos)
    {
        lastCheckPointPos = pos;
        isCheckpointActive = true;
        Debug.Log("체크포인트 저장됨: " + pos);
    }
}


