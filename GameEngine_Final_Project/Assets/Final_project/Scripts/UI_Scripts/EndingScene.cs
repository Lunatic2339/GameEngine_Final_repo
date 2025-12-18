using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScene : MonoBehaviour
{
    public void GoToTitle()
    {
        Time.timeScale = 1f; // 혹시 모르니 시간 정상화
        SceneManager.LoadScene("MainMenu"); // 님 게임의 시작화면 씬 이름
    }
}