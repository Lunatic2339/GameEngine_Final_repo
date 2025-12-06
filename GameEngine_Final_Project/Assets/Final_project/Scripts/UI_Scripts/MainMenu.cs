using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnStartGame()
    {
        // "GameScene"이라는 이름의 씬을 로드합니다.
        // 이때 GameManager가 이미 있다면, 데이터가 유지된 채로 넘어갑니다.
        SceneManager.LoadScene("Tutorial");
        
        // (참고) 만약 새 게임을 할 때마다 데이터를 초기화하고 싶다면
        // 여기서 GameManager.instance의 변수들을 초기화해주는 함수를 호출하면 됩니다.
    }
}