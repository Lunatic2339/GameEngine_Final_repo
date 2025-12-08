using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class MainMenu : MonoBehaviour
{
    public Button continueButton; 

    void Start()
    {
        // 저장된 데이터가 없으면 이어하기 버튼 끄기
        if (!PlayerPrefs.HasKey("HasSaveData"))
        {
            continueButton.interactable = false; 
        }
    }

    public void OnContinueGame()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadGameData();
        }
    }

    public void OnNewGame()
    {
        PlayerPrefs.DeleteAll(); 
        SceneManager.LoadScene("Tutorial"); 
    }

    // ★ [추가됨] 게임 종료 함수
    public void OnQuitGame()
    {
        Debug.Log("게임 종료!"); // 에디터에서 눌렀는지 확인용 로그

        // 유니티 에디터에서 플레이 중일 때는 플레이 모드를 멈춤
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // 실제 빌드된 게임(exe 파일 등)에서는 창을 닫음
            Application.Quit();
        #endif
    }
}