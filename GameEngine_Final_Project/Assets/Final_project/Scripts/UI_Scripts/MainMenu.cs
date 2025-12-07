using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button continueButton; // 인스펙터에서 이어하기 버튼 연결

    void Start()
    {
        // 저장된 데이터가 없으면 이어하기 버튼을 꺼버림 (비활성화)
        if (!PlayerPrefs.HasKey("HasSaveData"))
        {
            continueButton.interactable = false; 
            // 또는 continueButton.gameObject.SetActive(false); 로 아예 숨김
        }
    }

    public void OnContinueGame()
    {
        // 매니저에게 "저장된 거 불러와!" 명령
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadGameData();
        }
    }

    public void OnNewGame() // 기존 시작 버튼
    {
        // 새 게임이니까 기존 저장 데이터 삭제 (선택사항)
        PlayerPrefs.DeleteAll(); 
        
        // 기본 씬 로드
        UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial"); // 첫 맵 이름
    }
}